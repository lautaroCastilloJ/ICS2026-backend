using System.Net.Http.Headers;
using System.Net.Http.Json;
using Dsw2025Tpi.Application.Dtos.Orders;
using Dsw2025Tpi.Application.Dtos.Products;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.IntegrationTests.Infrastructure;

/// <summary>Respuesta de POST /api/auth/login: { token, role, customerId }.</summary>
public sealed record LoginResult(string Token, string Role, Guid? CustomerId);

/// <summary>
/// Forma JSON de PagedResult. PagedResult tiene constructor privado y no se
/// puede deserializar directamente, por eso la prueba define su propio contrato.
/// </summary>
public sealed record PagedResponse<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize);

/// <summary>Datos y clientes HTTP reutilizables por todas las pruebas de integracion.</summary>
public static class ApiTestHelpers
{
    public const string CustomerPassword = "Cliente123!";

    public static readonly AddressDto Address =
        new("Av. Siempre Viva", "742", "San Miguel de Tucuman", "Tucuman", "4000");

    // La base SQLite se comparte entre TODAS las pruebas de la coleccion: cada
    // prueba genera datos unicos para no depender de lo que dejaron las demas.
    // El SKU solo admite mayusculas, numeros y guiones (ProductRequestValidator).
    public static string UniqueSku() => $"IT-{Guid.NewGuid():N}"[..15].ToUpperInvariant();

    public static ProductRequest NewProductRequest(string sku, int stock = 10, decimal price = 1500m) =>
        new(sku, $"INT-{sku}", $"Producto {sku}", "Producto de prueba", price, stock, null);

    public static OrderRequest NewOrderRequest(Guid productId, int quantity) =>
        new(Address, Address, null, new[] { new OrderItemRequest(productId, quantity) });

    public static async Task<LoginResult> LoginAsync(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginResult>())!;
    }

    /// <summary>Cliente HTTP autenticado como el admin que siembra IdentitySeeder.</summary>
    public static async Task<HttpClient> CreateAdminClientAsync(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var login = await LoginAsync(client,
            CustomWebApplicationFactory.AdminUserName, CustomWebApplicationFactory.AdminPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        return client;
    }

    /// <summary>Registra un cliente nuevo por la API, inicia sesion y devuelve el HttpClient con su JWT.</summary>
    public static async Task<(HttpClient Client, Guid CustomerId)>
        CreateCustomerClientAsync(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        var userName = $"cli{Guid.NewGuid():N}"[..15];

        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            userName,
            password = CustomerPassword,
            email = $"{userName}@test.local",
            displayName = "Cliente de Prueba",
            phoneNumber = (string?)null
        });
        register.EnsureSuccessStatusCode();

        var login = await LoginAsync(client, userName, CustomerPassword);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);

        return (client, login.CustomerId!.Value);
    }

    /// <summary>Inserta un producto directamente en SQLite (sin pasar por la API).</summary>
    public static async Task<Product> SeedProductAsync(
        CustomWebApplicationFactory factory, string sku, int stock = 10, bool active = true, string? name = null)
    {
        using var db = factory.CreateDbContext();
        var product = Product.Create(sku, $"INT-{sku}", name ?? $"Producto {sku}", "", 1500m, stock);
        if (!active)
            product.Deactivate();
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }
}