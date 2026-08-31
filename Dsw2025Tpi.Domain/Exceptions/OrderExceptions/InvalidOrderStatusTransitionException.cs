using Dsw2025Tpi.Domain.Enums;
using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class InvalidOrderStatusTransitionException : ExceptionBase
{
    public OrderStatus FromStatus { get; }
    public OrderStatus ToStatus { get; }

    public InvalidOrderStatusTransitionException(OrderStatus from, OrderStatus to)
        : base("ORDER_INVALID_STATUS_TRANSITION")
    {
        FromStatus = from;
        ToStatus = to;
    }
}