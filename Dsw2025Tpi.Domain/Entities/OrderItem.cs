using Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

namespace Dsw2025Tpi.Domain.Entities;

public sealed class OrderItem : EntityBase
{
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Subtotal => Quantity * UnitPrice;   // derivado

    public Guid OrderId { get; private set; }          // FK
    public Guid ProductId { get; private set; }        // FK
    public string ProductName { get; private set; } = null!; // snapshot del nombre

    public Order? Order { get; private set; }          // navegación

    private OrderItem() { } // for EF

    private OrderItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        SetProduct(productId);
        SetProductName(productName);
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
    }

    public static OrderItem Create(Guid productId, string productName, int quantity, decimal unitPrice)
        => new(productId, productName, quantity, unitPrice);

    public void ChangeQuantity(int newQuantity)
    {
        SetQuantity(newQuantity);
    }

    // Si querés permitir cambiar el nombre después, dejalo public;
    // si no, hacelo private para que solo se pueda setear en el constructor.
    private void SetProductName(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new InvalidOrderItemProductNameException(); // excepción específica

        ProductName = productName.Trim();
    }

    private void SetProduct(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new InvalidOrderItemProductException();     // aquí sí aplica esta
        ProductId = productId;
    }

    private void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidOrderItemQuantityException();

        Quantity = quantity;
    }

    private void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
            throw new InvalidOrderItemPriceException();

        UnitPrice = unitPrice;
    }
}
