using Dsw2025Tpi.Application.Dtos.Users;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Application.Validators;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Data.Identity.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dsw2025Tpi.Data.Identity;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IRepository<Customer> _customerRepository;
    private readonly Dsw2025TpiDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IRepository<Customer> customerRepository,
        Dsw2025TpiDbContext context,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _customerRepository = customerRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<string> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando registro para el usuario: {Username}, Email: {Email}",
            request.UserName, request.Email);

        // UserManager confirma cada operacion con su propio SaveChanges; la
        // transaccion explicita agrupa usuario, rol y Customer en un todo o nada.
        // Si algo lanza, el using la descarta y se revierte.
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        // El registro publico SIEMPRE crea clientes: el rol no lo elige quien se registra.
        var user = await CreateUserAsync(request.UserName, request.Email, request.DisplayName, request.Password, AppRoles.Cliente);

        // Customer vinculado al AppUser
        var customer = Customer.Create(request.Email, request.DisplayName, request.PhoneNumber);
        await _customerRepository.Add(customer);
        _logger.LogDebug("Customer creado con ID: {CustomerId}", customer.Id);

        user.CustomerId = customer.Id;
        user.PhoneNumber = request.PhoneNumber;
        await _userManager.UpdateAsync(user);

        await transaction.CommitAsync(cancellationToken);

        _logger.LogInformation("Registro completado exitosamente para el usuario: {Username}", request.UserName);
        return user.Id;
    }

    public async Task<string> CreateAdminAsync(CreateAdminRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var user = await CreateUserAsync(request.UserName, request.Email, request.DisplayName, request.Password, AppRoles.Administrador);

        await transaction.CommitAsync(cancellationToken);

        // Auditoria: queda registrado quien otorgo privilegios de administrador.
        _logger.LogWarning("Administrador {NewAdmin} (ID: {UserId}) creado por {CreatedBy}",
            user.UserName, user.Id, createdBy);

        return user.Id;
    }

    public async Task ChangePasswordAsync(string userName, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        // El token es valido pero el usuario pudo haberse borrado despues de emitirlo.
        var user = await _userManager.FindByNameAsync(userName)
            ?? throw new InvalidCredentialsException();

        // Un token robado no debe servir para adivinar la contraseña: los
        // intentos fallidos cuentan para el mismo bloqueo que el login.
        if (await _userManager.IsLockedOutAsync(user))
            throw new AccountLockedException();

        if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword))
        {
            await _userManager.AccessFailedAsync(user);
            _logger.LogWarning("Cambio de contraseña rechazado: contraseña actual incorrecta para {Username}", userName);

            if (await _userManager.IsLockedOutAsync(user))
                throw new AccountLockedException();

            throw new InvalidCurrentPasswordException();
        }

        if (await _userManager.IsInRoleAsync(user, AppRoles.Administrador)
            && request.NewPassword.Length < UserRules.MinAdminPasswordLength)
        {
            throw new PasswordChangeFailedException("AUTH_ADMIN_PASSWORD_TOO_SHORT");
        }

        // ChangePasswordAsync vuelve a verificar la actual, aplica la politica de
        // Identity y renueva el SecurityStamp.
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            _logger.LogWarning("Falló el cambio de contraseña de {Username}: {Errors}", userName, errors);
            throw new PasswordChangeFailedException(errors: errors);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        _logger.LogInformation("Contraseña cambiada para el usuario {Username}", userName);
    }

    public async Task<(string Username, string Role, Guid? CustomerId)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Intento de inicio de sesión para el usuario: {Username}", request.Username);

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Falló el inicio de sesión: Credenciales vacías");
            throw new InvalidCredentialsException();
        }

        // lockoutOnFailure: true -> tras varios intentos fallidos la cuenta se
        // bloquea temporalmente (ver options.Lockout en Program.cs).
        var result = await _signInManager.PasswordSignInAsync(
            request.Username, request.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Inicio de sesión rechazado: cuenta bloqueada para el usuario {Username}", request.Username);
            throw new AccountLockedException();
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Falló el inicio de sesión: Credenciales inválidas para el usuario {Username}", request.Username);
            throw new InvalidCredentialsException();
        }

        var user = await _userManager.FindByNameAsync(request.Username)
            ?? throw new InvalidCredentialsException();

        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? AppRoles.Cliente;

        _logger.LogInformation("Inicio de sesión exitoso para el usuario: {Username}, Rol: {Role}, CustomerId: {CustomerId}",
            request.Username, userRole, user.CustomerId);

        return (request.Username, userRole, user.CustomerId);
    }

    /// <summary>
    /// Crea el usuario de Identity y le asigna el rol. Los roles deben existir
    /// (los crea <see cref="IdentitySeeder"/> al iniciar la aplicacion).
    /// </summary>
    private async Task<AppUser> CreateUserAsync(string userName, string email, string displayName, string password, string role)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            _logger.LogWarning("Intento de alta con email ya existente: {Email}", email);
            throw new EmailAlreadyExistsException(email);
        }

        if (await _userManager.FindByNameAsync(userName) is not null)
        {
            _logger.LogWarning("Intento de alta con username ya existente: {Username}", userName);
            throw new UsernameAlreadyExistsException(userName);
        }

        var user = new AppUser
        {
            UserName = userName,
            Email = email,
            DisplayName = displayName
        };

        EnsureSucceeded(await _userManager.CreateAsync(user, password), "creación del usuario");
        EnsureSucceeded(await _userManager.AddToRoleAsync(user, role), "asignación del rol");

        _logger.LogDebug("Usuario {UserId} creado con rol {Role}", user.Id, role);
        return user;
    }

    private void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
            return;

        var errors = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
        _logger.LogWarning("Falló la {Operation}: {Errors}", operation, errors);
        throw new UserCreationFailedException(errors);
    }
}
