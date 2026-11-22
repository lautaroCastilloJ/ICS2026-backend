using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class OrderInsufficientStockException : ExceptionBase
{
    // 400 (consigna TP4): el pedido supera el stock disponible.
    public override ErrorType Type => ErrorType.Validation;

    public Guid ProductId { get; }
    public string ProductName { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public OrderInsufficientStockException(
        Guid productId,
        string productName,
        int requestedQuantity,
        int availableQuantity)
        : base("ORDER_INSUFFICIENT_STOCK",
            $"Stock insuficiente para '{productName}': se pidieron {requestedQuantity} " +
            $"unidades y hay {availableQuantity} disponibles.")
    {
        ProductId = productId;
        ProductName = productName;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}
