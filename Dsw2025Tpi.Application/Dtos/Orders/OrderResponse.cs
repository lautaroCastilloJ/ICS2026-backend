using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos.Orders;

public record OrderResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    DateTime Date,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    string? Notes,
    string Status,
    decimal TotalAmount,
    List<OrderItemResponse> Items
);
