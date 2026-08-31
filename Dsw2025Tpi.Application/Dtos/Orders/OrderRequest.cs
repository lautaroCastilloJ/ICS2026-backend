using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos.Orders;

public sealed record OrderRequest(
    string ShippingAddress,
    string BillingAddress,
    string? Notes,
    IEnumerable<OrderItemRequest> OrderItems
);
