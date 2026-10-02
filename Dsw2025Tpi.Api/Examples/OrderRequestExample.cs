using Dsw2025Tpi.Application.Dtos.Orders;
using Swashbuckle.AspNetCore.Filters;

namespace Dsw2025Tpi.Api.Examples;

public class OrderRequestExample : IExamplesProvider<OrderRequest>
{
    public OrderRequest GetExamples()
    {
        return new OrderRequest(
            ShippingAddress: new AddressDto(
                Street: "Av. Mate de Luna",
                Number: "1850",
                City: "San Miguel de Tucumán",
                Province: "Tucumán",
                PostalCode: "4000"
            ),
            BillingAddress: new AddressDto(
                Street: "San Martín",
                Number: "450",
                City: "San Miguel de Tucumán",
                Province: "Tucumán",
                PostalCode: "T4000ABC"
            ),
            Notes: "Entregar después de las 18:00",
            OrderItems: new List<OrderItemRequest>
            {
                new OrderItemRequest(
                    ProductId: Guid.Parse("660e8400-e29b-41d4-a716-446655440000"),
                    Quantity: 2
                )
            }
        );
    }
}
