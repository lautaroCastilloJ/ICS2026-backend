using Dsw2025Tpi.Application.Dtos.Users;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticateController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthenticateController> _logger;

    public AuthenticateController(
        IUserService userService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthenticateController> logger)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Intento de inicio de sesión para el usuario: {Username}", request.Username);

        var (username, role, customerId) = await _userService.LoginAsync(request, cancellationToken);

        var token = _jwtTokenService.GenerateToken(username, role, customerId);
        
        _logger.LogInformation("Inicio de sesión exitoso para el usuario: {Username}, Rol: {Role}", username, role);

        return Ok(new 
        { 
            token,
            role,
            customerId
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Intento de registro para el usuario: {Username}, Email: {Email}", request.UserName, request.Email);

        var userId = await _userService.RegisterAsync(request, cancellationToken);

        _logger.LogInformation("Usuario registrado exitosamente con ID: {UserId}", userId);

        return Ok(new
        {
            userId
        });
    }
}
