using System.Net;
using System.Net.Http.Json;
using Dsw2025Tpi.Application.Dtos.Orders;
using Dsw2025Tpi.Application.Dtos.Products;
using Dsw2025Tpi.IntegrationTests.Infrastructure;
using FluentAssertions;
using static Dsw2025Tpi.IntegrationTests.Infrastructure.ApiTestHelpers;

namespace Dsw2025Tpi.IntegrationTests.E2E;

/// <summary>
/// E2E a nivel de API: encadena peticiones HTTP como lo haria un usuario real,
/// sin tocar la base directamente. Cada paso usa el resultado del anterior.
/// </summary>
[Collection(IntegrationTestsCollection.Name)]
public class PurchaseFlowE2ETests
{
    private readonly CustomWebApplicationFactory _factory;

    public PurchaseFlowE2ETests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task E2E_AdminCreatesProduct_CustomerRegistersAndBuys_StockIsUpdatedInCatalog()
    {
        // 1. El administrador da de alta un producto con stock 10
        var admin = await CreateAdminClientAsync(_factory);
        var createProduct = await admin.PostAsJsonAsync("/api/products",
            NewProductRequest(UniqueSku(), stock: 10, price: 2500m));
        createProduct.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = (await createProduct.Content.ReadFromJsonAsync<ProductResponse>())!;

        // 2. Un cliente se registra e inicia sesion
        var (customer, _) = await CreateCustomerClientAsync(_factory);

        // 3. El cliente compra 4 unidades
        var createOrder = await customer.PostAsJsonAsync("/api/orders", NewOrderRequest(product.Id, 4));
        createOrder.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = (await createOrder.Content.ReadFromJsonAsync<OrderResponse>())!;

        // 4. El cliente consulta su orden
        var getOrder = await customer.GetAsync($"/api/orders/{order.Id}");
        getOrder.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = (await getOrder.Content.ReadFromJsonAsync<OrderResponse>())!;
        fetched.TotalAmount.Should().Be(4 * 2500m);

        // 5. El catalogo publico refleja el stock descontado
        var getProduct = await _factory.CreateClient().GetAsync($"/api/products/{product.Id}");
        getProduct.StatusCode.Should().Be(HttpStatusCode.OK);
        (await getProduct.Content.ReadFromJsonAsync<ProductResponse>())!.StockQuantity.Should().Be(6);
    }
}