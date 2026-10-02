using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class OrderInsufficientStockException : ExceptionBase
{
    public override ErrorType Type => ErrorType.BusinessRule;

    public Guid ProductId { get; }
    public string ProductName { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public OrderInsufficientStockException(
        Guid productId,
        string productName,
        int requestedQuantity,
        int availableQuantity)
        : base("ORDER_INSUFFICIENT_STOCK")
    {
        ProductId = productId;
        ProductName = productName;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}
