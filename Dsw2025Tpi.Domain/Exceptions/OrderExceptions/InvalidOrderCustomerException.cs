using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class InvalidOrderCustomerException : ExceptionBase
{
    public InvalidOrderCustomerException()
        : base("ORDER_INVALID_CUSTOMER") { }
}
