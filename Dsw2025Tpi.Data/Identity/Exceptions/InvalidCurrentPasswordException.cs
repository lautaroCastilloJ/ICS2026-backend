using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Data.Identity.Exceptions;

/// <summary>
/// La contraseña actual no coincide. Es un error de validacion (400) y no 401:
/// el usuario SI esta autenticado, y un 401 haria que el frontend cierre la sesion.
/// </summary>
public sealed class InvalidCurrentPasswordException : ExceptionBase
{
    public InvalidCurrentPasswordException()
        : base("AUTH_INVALID_CURRENT_PASSWORD") { }
}
