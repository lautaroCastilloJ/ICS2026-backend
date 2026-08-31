using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class OrderWithoutItemsException : ExceptionBase
{
    public OrderWithoutItemsException()
        : base("ORDER_WITHOUT_ITEMS") { }
}
