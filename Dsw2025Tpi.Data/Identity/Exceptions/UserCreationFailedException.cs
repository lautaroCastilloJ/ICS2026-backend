using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Data.Identity.Exceptions;

public sealed class UserCreationFailedException : ExceptionBase
{
    public UserCreationFailedException(string? errors = null)
        : base("AUTH_USER_CREATION_FAILED")
    {
        Errors = errors;
    }

    public string? Errors { get; }
}
