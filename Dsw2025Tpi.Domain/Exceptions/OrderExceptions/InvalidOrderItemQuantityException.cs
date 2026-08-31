using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class InvalidOrderItemQuantityException : ExceptionBase
{
    public InvalidOrderItemQuantityException()
        : base("ORDERITEM_INVALID_QUANTITY") { }
}
