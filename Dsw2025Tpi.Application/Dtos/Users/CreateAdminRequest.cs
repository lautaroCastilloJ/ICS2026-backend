namespace Dsw2025Tpi.Application.Dtos.Users;

/// <summary>
/// Alta de un administrador. Solo lo puede usar otro administrador.
/// </summary>
public record CreateAdminRequest(
    string UserName,
    string Password,
    string Email,
    string DisplayName
);
