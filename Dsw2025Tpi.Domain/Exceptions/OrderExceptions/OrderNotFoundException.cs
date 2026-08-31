using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class OrderNotFoundException : ExceptionBase
{
    public Guid OrderId { get; }

    public OrderNotFoundException(Guid orderId)
        : base("ORDER_NOT_FOUND")
    {
        OrderId = orderId;
    }
}
