namespace Dsw2025Tpi.Application.Dtos.Users;

/// <summary>
/// Cambio de contraseña del usuario autenticado. No incluye el usuario a
/// proposito: se toma del token, asi nadie puede cambiar la clave de otro.
/// </summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);
