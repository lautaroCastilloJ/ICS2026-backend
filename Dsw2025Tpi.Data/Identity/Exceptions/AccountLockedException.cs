using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Data.Identity.Exceptions;

public sealed class AccountLockedException : ExceptionBase
{
    public override ErrorType Type => ErrorType.Unauthorized;

    public AccountLockedException()
        : base("AUTH_ACCOUNT_LOCKED") { }
}
