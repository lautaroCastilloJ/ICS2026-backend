using Dsw2025Tpi.Domain.Enums;
using Dsw2025Tpi.Shared.Exceptions;
using Dsw2025Tpi.Domain.Exceptions.OrderExceptions;
using Dsw2025Tpi.Domain.ValueObjects;


namespace Dsw2025Tpi.Domain.Entities;

public sealed class Order : EntityBase
{
    public DateTime Date { get; private set; }
    public Address ShippingAddress { get; private set; }
    public Address BillingAddress { get; private set; }
    public string? Notes { get; private set; }
    public OrderStatus Status { get; private set; }
    public Guid CustomerId { get; private set; } // FK

    // Propiedad de navegación
    public Customer? Customer { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.Subtotal); // Derivado: suma de subtotales

    private Order() { } // for EF

    private Order(Guid customerId, Address shippingAddress, Address billingAddress, string notes)
    {
        if (customerId == Guid.Empty)
            throw new InvalidOrderCustomerException();

        CustomerId = customerId;
        Date = DateTime.UtcNow;
        SetShippingAddress(shippingAddress);
        SetBillingAddress(billingAddress);
        SetNotes(notes);
        Status = OrderStatus.Pending;
    }

    public static Order Create(
        Guid customerId,
        Address shippingAddress,
        Address billingAddress,
        string? notes)
    {
        return new Order(customerId, shippingAddress, billingAddress, notes ?? string.Empty);
    }

    // --- comportamiento de la orden ---

    public void AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        EnsureModifiable();
        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.ChangeQuantity(existing.Quantity + quantity);
            return;
        }

        var item = OrderItem.Create(productId, productName, quantity, unitPrice);
        _items.Add(item);
    }


    public void RemoveItem(Guid orderItemId)
    {
        EnsureModifiable();

        var item = _items.FirstOrDefault(i => i.Id == orderItemId);
        if (item is null)
            return;

        _items.Remove(item);
    }

    public void ValidateHasItems()
    {
        if (_items.Count == 0)
            throw new OrderWithoutItemsException();
    }

    // Cambios de estado de la orden

    public void MarkAsProcessing()
    {
        ChangeStatus(OrderStatus.Processing);
    }

    public void MarkAsShipped()
    {
        ChangeStatus(OrderStatus.Shipped);
    }

    public void MarkAsDelivered()
    {
        ChangeStatus(OrderStatus.Delivered);
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOrderStatusTransitionException(Status, OrderStatus.Cancelled);

        ChangeStatus(OrderStatus.Cancelled);
    }

    private void ChangeStatus(OrderStatus newStatus)
    {
        // No se puede cambiar al mismo estado
        if (Status == newStatus)
            throw new OrderStatusAlreadySetException(Status);


        // No se puede cambiar una orden ya finalizada
        if (Status is OrderStatus.Cancelled or OrderStatus.Delivered)
            throw new OrderAlreadyFinalizedException(Id, Status, newStatus); 

        // A partir de cierto estado exigimos que la orden tenga items
        if (newStatus is OrderStatus.Processing or OrderStatus.Shipped or OrderStatus.Delivered)
            ValidateHasItems(); // lanza OrderWithoutItemsException si no hay items

        // Definimos las transiciones permitidas
        var allowedNextStatuses = Status switch
        {
            OrderStatus.Pending => new[] { OrderStatus.Processing, OrderStatus.Cancelled },
            OrderStatus.Processing => new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
            OrderStatus.Shipped => new[] { OrderStatus.Delivered, OrderStatus.Cancelled },
            // Cancelled y Delivered ya fueron manejados arriba como finales
            _ => Array.Empty<OrderStatus>()
        };

        if (!allowedNextStatuses.Contains(newStatus))
            throw new InvalidOrderStatusTransitionException(Status, newStatus);

        Status = newStatus;
    }


    private void EnsureModifiable()
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new InvalidOrderStatusTransitionException(Status, Status);
    }

    // Address ya se valida al crearse: aca solo se exige que este presente.
    private void SetShippingAddress(Address? address)
    {
        ShippingAddress = address ?? throw new InvalidOrderShippingAddressException();
    }

    private void SetBillingAddress(Address? address)
    {
        BillingAddress = address ?? throw new InvalidOrderBillingAddressException();
    }

    private void SetNotes(string notes)
    {
        Notes = notes?.Trim() ?? string.Empty;
    }
}