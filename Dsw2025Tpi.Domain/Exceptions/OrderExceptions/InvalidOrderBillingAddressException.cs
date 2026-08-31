using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class InvalidOrderBillingAddressException : ExceptionBase
{
    public InvalidOrderBillingAddressException()
        : base("ORDER_INVALID_BILLING_ADDRESS") { }
}
