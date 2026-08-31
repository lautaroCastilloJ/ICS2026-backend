using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class InvalidOrderItemProductNameException : ExceptionBase
{
    public InvalidOrderItemProductNameException()
        : base("ORDERITEM_INVALID_PRODUCT_NAME")
    {
    }
}
