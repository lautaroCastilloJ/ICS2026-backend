using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class InvalidOrderItemPriceException : ExceptionBase
{
    public InvalidOrderItemPriceException()
        : base("ORDERITEM_INVALID_PRICE") { }
}