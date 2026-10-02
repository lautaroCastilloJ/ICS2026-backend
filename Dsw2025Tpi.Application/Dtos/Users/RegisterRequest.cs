namespace Dsw2025Tpi.Application.Dtos.Users;

/// <summary>
/// Registro publico. No incluye el rol a proposito: todo usuario que se
/// registra por esta via es Cliente. Los administradores se crean con
/// <see cref="CreateAdminRequest"/> desde un endpoint protegido.
/// </summary>
public record RegisterRequest(
    string UserName, 
    string Password, 
    string Email, 
    string DisplayName,
    string? PhoneNumber
);
