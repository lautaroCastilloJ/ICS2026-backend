using System.Net;
using System.Net.Http.Json;
using Dsw2025Tpi.Api.Errors;
using Dsw2025Tpi.Application.Dtos.Orders;
using Dsw2025Tpi.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static Dsw2025Tpi.IntegrationTests.Infrastructure.ApiTestHelpers;

namespace Dsw2025Tpi.IntegrationTests.Orders;

[Collection(IntegrationTestsCollection.Name)]
public class OrdersEndpointsTests
{
    private readonly CustomWebApplicationFactory _factory;

    public OrdersEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateOrder_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var product = await SeedProductAsync(_factory, UniqueSku());
        var anonymous = _factory.CreateClient();

        // Act
        var response = await anonymous.PostAsJsonAsync("/api/orders", NewOrderRequest(product.Id, 1));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WhenAuthenticatedWithValidData_ReturnsCreatedAndPersistsOrderAndStockInSqlite()
    {
        // Arrange: producto con stock 10 y un cliente autenticado
        var product = await SeedProductAsync(_factory, UniqueSku(), stock: 10);
        var (customer, customerId) = await CreateCustomerClientAsync(_factory);

        // Act: compra de 3 unidades
        var response = await customer.PostAsJsonAsync("/api/orders", NewOrderRequest(product.Id, 3));

        // Assert: respuesta HTTP
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var created = await response.Content.ReadFromJsonAsync<OrderResponse>();

        // Assert: estado real de la base (un contexto NUEVO, sin cache del change tracker)
        using var db = _factory.CreateDbContext();
        var order = await db.Orders.Include(o => o.Items).SingleAsync(o => o.Id == created!.Id);
        order.CustomerId.Should().Be(customerId);
        order.Items.Should().ContainSingle(i => i.ProductId == product.Id && i.Quantity == 3);
        (await db.Products.SingleAsync(p => p.Id == product.Id)).StockQuantity.Should().Be(7);
    }

    [Fact]
    public async Task CreateOrder_WhenStockIsInsufficient_ReturnsBadRequestWithErrorMessage()
    {
        // Arrange: stock 2, se piden 5
        var product = await SeedProductAsync(_factory, UniqueSku(), stock: 2);
        var (customer, customerId) = await CreateCustomerClientAsync(_factory);

        // Act
        var response = await customer.PostAsJsonAsync("/api/orders", NewOrderRequest(product.Id, 5));

        // Assert: 400 con un JSON que explica la falta de stock
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Code.Should().Be("ORDER_INSUFFICIENT_STOCK");
        error.Message.Should().Contain(product.Name).And.Contain("5").And.Contain("2");

        // Assert: no se persistio nada (CA 4)
        using var db = _factory.CreateDbContext();
        (await db.Orders.AnyAsync(o => o.CustomerId == customerId)).Should().BeFalse();
        (await db.Products.SingleAsync(p => p.Id == product.Id)).StockQuantity.Should().Be(2);
    }
}