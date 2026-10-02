using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Data.Identity.Exceptions;

public sealed class UsernameAlreadyExistsException : ExceptionBase
{
    public override ErrorType Type => ErrorType.Conflict;

    public string Username { get; }

    public UsernameAlreadyExistsException(string username)
        : base("AUTH_USERNAME_ALREADY_EXISTS")
    {
        Username = username;
    }
}
