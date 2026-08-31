using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class InvalidOrderItemProductException : ExceptionBase
{
    public InvalidOrderItemProductException()
        : base("ORDERITEM_INVALID_PRODUCT") { }
}
