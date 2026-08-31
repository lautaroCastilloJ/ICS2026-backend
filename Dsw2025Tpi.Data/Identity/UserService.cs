using Dsw2025Tpi.Application.Dtos.Users;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Data.Identity;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Data.Identity.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Data.Identity;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IRepository<Customer> customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando registro para el usuario: {Username}, Email: {Email}", 
            request.UserName, request.Email);
        
        string userId = string.Empty;

        try
        {
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                // Validar que el email no exista
                var existingUserByEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingUserByEmail != null)
                {
                    _logger.LogWarning("Intento de registro con email ya existente: {Email}", request.Email);
                    throw new EmailAlreadyExistsException(request.Email);
                }

                // Validar que el username no exista
                var existingUserByUsername = await _userManager.FindByNameAsync(request.UserName);
                if (existingUserByUsername != null)
                {
                    _logger.LogWarning("Intento de registro con username ya existente: {Username}", request.UserName);
                    throw new UsernameAlreadyExistsException(request.UserName);
                }

                // 1. Crear AppUser
                var user = new AppUser
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    DisplayName = request.DisplayName
                };

                _logger.LogDebug("Creando usuario en el sistema de Identity");
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    _logger.LogWarning("Falló la creación del usuario: {Errors}", errors);
                    throw new UserCreationFailedException(errors);
                }

                userId = user.Id;
                _logger.LogDebug("Usuario creado con ID: {UserId}", userId);

                // Asignar rol
                var role = string.IsNullOrWhiteSpace(request.Role) ? AppRoles.Cliente : request.Role;
                _logger.LogDebug("Asignando rol: {Role}", role);
                
                // Crear el rol si no existe
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    _logger.LogInformation("El rol {Role} no existe, creándolo", role);
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                var roleResult = await _userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    var roleErrors = string.Join(" | ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    _logger.LogWarning("Falló la asignación del rol: {Errors}", roleErrors);
                    throw new UserCreationFailedException(roleErrors);
                }

                _logger.LogDebug("Rol asignado exitosamente");

                // 2. Crear Customer vinculado al AppUser (solo para clientes)
                if (role.Equals(AppRoles.Cliente, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Creando entidad Customer para usuario cliente");
                    
                    var customer = Customer.Create(
                        request.Email,
                        request.DisplayName,
                        request.PhoneNumber
                    );

                    await _customerRepository.Add(customer);
                    _logger.LogDebug("Customer creado con ID: {CustomerId}", customer.Id);

                    // 3. Asignar CustomerId al AppUser
                    user.CustomerId = customer.Id;
                    user.PhoneNumber = request.PhoneNumber;
                    await _userManager.UpdateAsync(user);
                    
                    _logger.LogDebug("Usuario actualizado con CustomerId");
                }

            }, cancellationToken);

            _logger.LogInformation("Registro completado exitosamente para el usuario: {Username}", request.UserName);
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló el registro para el usuario: {Username}", request.UserName);
            throw;
        }
    }

    public async Task<(string Username, string Role, Guid? CustomerId)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Intento de inicio de sesión para el usuario: {Username}", request.Username);

        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Falló el inicio de sesión: Credenciales vacías");
                throw new InvalidCredentialsException();
            }

            _logger.LogDebug("Intentando inicio de sesión con contraseña");
            var result = await _signInManager.PasswordSignInAsync(
                request.Username, request.Password, false, false);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Falló el inicio de sesión: Credenciales inválidas para el usuario {Username}", request.Username);
                throw new InvalidCredentialsException();
            }

            _logger.LogDebug("Inicio de sesión con contraseña exitoso, obteniendo detalles del usuario");
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("Falló el inicio de sesión: Usuario no encontrado después de inicio de sesión exitoso");
                throw new InvalidCredentialsException();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? AppRoles.Cliente;

            _logger.LogInformation("Inicio de sesión exitoso para el usuario: {Username}, Rol: {Role}, CustomerId: {CustomerId}", 
                request.Username, userRole, user.CustomerId);

            return (request.Username, userRole, user.CustomerId);
        }
        catch (Exception ex) when (ex is not InvalidCredentialsException)
        {
            _logger.LogError(ex, "Error inesperado durante el inicio de sesión para el usuario: {Username}", request.Username);
            throw;
        }
    }
}
