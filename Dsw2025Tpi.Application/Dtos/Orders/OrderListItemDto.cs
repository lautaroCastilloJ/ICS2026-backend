namespace Dsw2025Tpi.Application.Dtos.Orders;

public sealed record OrderListItemDto(
    Guid Id,
    DateTime Date,
    string Status,
    Guid CustomerId,
    string CustomerName,
    decimal TotalAmount);
