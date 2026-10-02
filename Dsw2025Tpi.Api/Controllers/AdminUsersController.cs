using Dsw2025Tpi.Application.Dtos.Users;
using Dsw2025Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dsw2025Tpi.Api.Controllers;

/// <summary>
/// Gestion de administradores. Solo un administrador puede crear otro: el
/// registro publico (/api/auth/register) crea unicamente clientes.
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Administrador")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminUsersController(IUserService userService)
    {
        _userService = userService;
    }

    // POST /api/admin/users
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request, CancellationToken cancellationToken = default)
    {
        // El claim "sub" del JWT (username) llega mapeado a NameIdentifier.
        var createdBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "desconocido";

        var userId = await _userService.CreateAdminAsync(request, createdBy, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, new { userId });
    }
}
