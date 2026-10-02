using Dsw2025Tpi.Api.Errors;
using Dsw2025Tpi.Application.Dtos.Orders;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Identity;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Dsw2025Tpi.Tests;

public sealed class OrdersSafetyTests : IClassFixture<OrdersDatabaseFixture>
{
    private readonly OrdersDatabaseFixture _fixture;
    public OrdersSafetyTests(OrdersDatabaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Owner_can_read_order_but_another_customer_gets_the_same_404_as_missing_order()
    {
        var (owner, products) = await SeedAsync(4);
        await using var db = _fixture.CreateContext();
        var order = await _fixture.CreateService(db).CreateOrderAsync(owner, Request(products[0], 1));
        using var own = await GetOrder(order.Id, AppRoles.Cliente, owner.ToString());
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Equal(order.Id, (await own.Content.ReadFromJsonAsync<OrderResponse>())!.Id);

        var stranger = Guid.NewGuid().ToString();
        using var foreign = await GetOrder(order.Id, AppRoles.Cliente, stranger);
        using var absent = await GetOrder(Guid.NewGuid(), AppRoles.Cliente, stranger);
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, absent.StatusCode);
        var foreignError = await foreign.Content.ReadFromJsonAsync<ErrorResponse>();
        var absentError = await absent.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("ORDER_NOT_FOUND", foreignError!.Code);
        Assert.Equal(absentError!.Message, foreignError.Message);
        Assert.DoesNotContain("shippingAddress", await foreign.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Administrator_without_customer_claim_can_read_any_order()
    {
        var (owner, products) = await SeedAsync(4);
        await using var db = _fixture.CreateContext();
        var order = await _fixture.CreateService(db).CreateOrderAsync(owner, Request(products[0], 1));
        using var response = await GetOrder(order.Id, AppRoles.Administrador);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized)]
    [InlineData("OtroRol", null, HttpStatusCode.Forbidden)]
    [InlineData(AppRoles.Cliente, null, HttpStatusCode.Forbidden)]
    [InlineData(AppRoles.Cliente, "invalido", HttpStatusCode.Unauthorized)]
    [InlineData(AppRoles.Cliente, "00000000-0000-0000-0000-000000000000", HttpStatusCode.Unauthorized)]
    public async Task Missing_or_invalid_identity_never_becomes_an_administrator_lookup(string? role, string? customer, HttpStatusCode status)
    {
        using var response = await GetOrder(Guid.NewGuid(), role, customer);
        Assert.Equal(status, response.StatusCode);
    }

    [Fact]
    public async Task Creating_order_persists_all_stock_decrements_and_the_order_together()
    {
        var (customer, products) = await SeedAsync(5, 7);
        await using (var db = _fixture.CreateContext())
        {
            await _fixture.CreateService(db).CreateOrderAsync(customer,
                Request(new OrderItemRequest(products[0], 2), new OrderItemRequest(products[1], 3)));
        }
        await using var verify = _fixture.CreateContext();
        Assert.Equal(3, (await verify.Products.FindAsync(products[0]))!.StockQuantity);
        Assert.Equal(4, (await verify.Products.FindAsync(products[1]))!.StockQuantity);
        var order = await verify.Orders.Include(o => o.Items).SingleAsync(o => o.CustomerId == customer);
        Assert.Equal(2, order.Items.Count);
        Assert.Equal(5000m, order.TotalAmount);
    }

    [Fact]
    public async Task Two_buyers_of_the_last_unit_produce_one_order_and_one_conflict()
    {
        var (customer, products) = await SeedAsync(1);
        var gate = new ConcurrentSaveGate();
        await using var first = _fixture.CreateContext(gate);
        await using var second = _fixture.CreateContext(gate);
        var attempts = await Task.WhenAll(
            Record.ExceptionAsync(() => _fixture.CreateService(first).CreateOrderAsync(customer, Request(products[0], 1))),
            Record.ExceptionAsync(() => _fixture.CreateService(second).CreateOrderAsync(customer, Request(products[0], 1))));
        Assert.Single(attempts, e => e is null);
        Assert.Single(attempts, e => e is ConcurrentUpdateException);
        await using var verify = _fixture.CreateContext();
        Assert.Equal(0, (await verify.Products.FindAsync(products[0]))!.StockQuantity);
        Assert.Equal(1, await verify.Orders.CountAsync(o => o.CustomerId == customer));
        Assert.Equal(1, await verify.OrderItems.CountAsync(i => i.ProductId == products[0]));
    }

    [Fact]
    public async Task Conflict_rolls_back_the_whole_order_including_other_products()
    {
        var (customer, products) = await SeedAsync(1, 5);
        await using var stale = _fixture.CreateContext();
        // Forzar una lectura anterior a la compra ganadora.
        await stale.Products.Where(p => products.Contains(p.Id)).LoadAsync();
        await using (var winner = _fixture.CreateContext())
            await _fixture.CreateService(winner).CreateOrderAsync(customer, Request(products[0], 1));

        await Assert.ThrowsAsync<ConcurrentUpdateException>(() => _fixture.CreateService(stale)
            .CreateOrderAsync(customer, Request(new OrderItemRequest(products[1], 2), new OrderItemRequest(products[0], 1))));
        Assert.Empty(stale.ChangeTracker.Entries());
        await using var verify = _fixture.CreateContext();
        Assert.Equal(0, (await verify.Products.FindAsync(products[0]))!.StockQuantity);
        Assert.Equal(5, (await verify.Products.FindAsync(products[1]))!.StockQuantity);
        Assert.Equal(1, await verify.Orders.CountAsync(o => o.CustomerId == customer));
        Assert.False(await verify.OrderItems.AnyAsync(i => i.ProductId == products[1]));
    }

    [Fact]
    public async Task Database_failure_when_inserting_order_does_not_commit_stock()
    {
        var (customer, products) = await SeedAsync(5, 7);
        // Provocar un fallo real de FK al guardar, despues de las validaciones
        // y descuentos en memoria, sin alterar el esquema de la base.
        var request = Request(new OrderItemRequest(products[0], 2), new OrderItemRequest(products[1], 3));
        await using var db = _fixture.CreateContext(new InvalidOrderCustomerInterceptor());
        await Assert.ThrowsAsync<DbUpdateException>(() => _fixture.CreateService(db).CreateOrderAsync(customer, request));
        await using var verify = _fixture.CreateContext();
        Assert.Equal(5, (await verify.Products.FindAsync(products[0]))!.StockQuantity);
        Assert.Equal(7, (await verify.Products.FindAsync(products[1]))!.StockQuantity);
        Assert.False(await verify.Orders.AnyAsync(o => o.CustomerId == customer));
        Assert.False(await verify.OrderItems.AnyAsync(i => products.Contains(i.ProductId)));
    }

    [Fact]
    public async Task Concurrency_error_is_returned_as_409_with_a_public_message()
    {
        using var response = await _fixture.Client.GetAsync("/test-conflict");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("RESOURCE_CONCURRENTLY_MODIFIED", error!.Code);
        Assert.DoesNotContain("Unknown error", error.Message);
        Assert.Null(error.Details);
    }

    private async Task<HttpResponseMessage> GetOrder(Guid id, string? role, string? customer = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/orders/{id}");
        if (role is not null) request.Headers.Add("X-Test-Role", role);
        if (customer is not null) request.Headers.Add("X-Test-Customer", customer);
        return await _fixture.Client.SendAsync(request);
    }

    private async Task<(Guid Customer, Guid[] Products)> SeedAsync(params int[] stocks)
    {
        await using var db = _fixture.CreateContext();
        var customer = Customer.Create($"{Guid.NewGuid():N}@example.com", "Cliente de prueba", null);
        var products = stocks.Select(stock => Product.Create($"TEST-{Guid.NewGuid():N}".ToUpperInvariant(),
            Guid.NewGuid().ToString("N"), "Producto de prueba", "", 1000m, stock)).ToArray();
        db.Customers.Add(customer);
        db.Products.AddRange(products);
        await db.SaveChangesAsync();
        return (customer.Id, products.Select(p => p.Id).ToArray());
    }

    private static OrderRequest Request(Guid product, int quantity) => Request(new OrderItemRequest(product, quantity));
    private static OrderRequest Request(params OrderItemRequest[] items)
    {
        var address = new AddressDto("Calle de prueba", "123", "Tucuman", "Tucuman", "4000");
        return new OrderRequest(address, address, "Prueba automatizada", items);
    }

    private sealed class ConcurrentSaveGate : SaveChangesInterceptor
    {
        private int _arrived;
        private readonly TaskCompletionSource _bothReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _arrived) == 2) _bothReady.TrySetResult();
            await _bothReady.Task.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
            return result;
        }
    }

    private sealed class InvalidOrderCustomerInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var order = eventData.Context!.ChangeTracker.Entries<Order>().Single(e => e.State == EntityState.Added);
            order.Property(o => o.CustomerId).CurrentValue = Guid.NewGuid();
            return ValueTask.FromResult(result);
        }
    }
}
