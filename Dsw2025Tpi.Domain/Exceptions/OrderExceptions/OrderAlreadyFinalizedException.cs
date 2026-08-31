using Dsw2025Tpi.Domain.Enums;
using Dsw2025Tpi.Shared.Exceptions;


namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class OrderAlreadyFinalizedException : ExceptionBase
{
    public Guid OrderId { get; }
    public OrderStatus CurrentStatus { get; }
    public OrderStatus RequestedStatus { get; }

    public OrderAlreadyFinalizedException(Guid orderId, OrderStatus currentStatus, OrderStatus requestedStatus)
        : base("ORDER_ALREADY_FINALIZED")
    {
        OrderId = orderId;
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }
}