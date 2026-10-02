using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Data.Identity.Exceptions;

public sealed class PasswordChangeFailedException : ExceptionBase
{
    public PasswordChangeFailedException(string code = "AUTH_PASSWORD_CHANGE_FAILED", string? errors = null)
        : base(code)
    {
        Errors = errors;
    }

    public string? Errors { get; }
}
