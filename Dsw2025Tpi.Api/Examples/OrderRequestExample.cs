using Dsw2025Tpi.Application.Dtos.Orders;
using Swashbuckle.AspNetCore.Filters;

namespace Dsw2025Tpi.Api.Examples;

public class OrderRequestExample : IExamplesProvider<OrderRequest>
{
    public OrderRequest GetExamples()
    {
        return new OrderRequest(
            ShippingAddress: "Calle: , Altura: , Ciudad: San Miguel de Tucumán, Provincia: Tucumán",
            BillingAddress: "Calle: , Altura: , Ciudad: San Miguel de Tucumán, Provincia: Tucumán",
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
