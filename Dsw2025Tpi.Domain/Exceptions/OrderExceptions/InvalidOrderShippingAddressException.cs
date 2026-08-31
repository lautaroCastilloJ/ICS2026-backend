using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class InvalidOrderShippingAddressException : ExceptionBase
{
    public InvalidOrderShippingAddressException()
        : base("ORDER_INVALID_SHIPPING_ADDRESS") { }
}
