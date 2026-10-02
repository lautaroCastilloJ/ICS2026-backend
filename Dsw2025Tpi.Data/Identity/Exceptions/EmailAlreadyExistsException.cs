using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Data.Identity.Exceptions;

public sealed class EmailAlreadyExistsException : ExceptionBase
{
    public override ErrorType Type => ErrorType.Conflict;

    public string Email { get; }

    public EmailAlreadyExistsException(string email)
        : base("AUTH_EMAIL_ALREADY_EXISTS")
    {
        Email = email;
    }
}
