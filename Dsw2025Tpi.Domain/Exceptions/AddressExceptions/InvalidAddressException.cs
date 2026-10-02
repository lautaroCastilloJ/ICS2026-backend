using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.AddressExceptions;

public sealed class InvalidAddressException : ExceptionBase
{
    public InvalidAddressException(string code)
        : base(code)
    { }
}
