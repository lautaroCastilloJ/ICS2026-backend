using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Data.Identity.Exceptions;

public sealed class InvalidCredentialsException : ExceptionBase
{
    public InvalidCredentialsException()
        : base("AUTH_INVALID_CREDENTIALS") { }
}
