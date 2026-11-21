using System.Net;
using System.Net.Http.Json;
using Dsw2025Tpi.Api.Errors;
using Dsw2025Tpi.Application.Dtos.Products;
using Dsw2025Tpi.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static Dsw2025Tpi.IntegrationTests.Infrastructure.ApiTestHelpers;

namespace Dsw2025Tpi.IntegrationTests.Products;

[Collection(IntegrationTestsCollection.Name)]
public class ProductsEndpointsTests
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductsEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AddProduct_WhenUnauthenticatedOrNotAdmin_ReturnsUnauthorizedOrForbidden()
    {
        // Arrange
        var sku = UniqueSku();
        var anonymous = _factory.CreateClient();
        var (customer, _) = await CreateCustomerClientAsync(_factory);

        // Act
        var withoutToken = await anonymous.PostAsJsonAsync("/api/products", NewProductRequest(sku));
        var asCustomer = await customer.PostAsJsonAsync("/api/products", NewProductRequest(sku));

        // Assert: 401 sin token, 403 con un rol distinto de Administrador
        withoutToken.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        asCustomer.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // ... y no se inserto nada (CA 2)
        using var db = _factory.CreateDbContext();
        (await db.Products.AnyAsync(p => p.Sku == sku)).Should().BeFalse();
    }

    [Fact]
    public async Task AddProduct_WhenAuthenticatedAsAdmin_PersistsInSqliteAndReturnsCreated()
    {
        // Arrange
        var sku = UniqueSku();
        var admin = await CreateAdminClientAsync(_factory);

        // Act
        var response = await admin.PostAsJsonAsync("/api/products",
            NewProductRequest(sku, stock: 10, price: 1500m));

        // Assert: respuesta HTTP
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        // Assert: la fila existe en SQLite con los valores enviados
        using var db = _factory.CreateDbContext();
        var saved = await db.Products.SingleAsync(p => p.Sku == sku);
        saved.StockQuantity.Should().Be(10);
        saved.CurrentUnitPrice.Should().Be(1500m);
        saved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AddProduct_WhenSkuAlreadyExistsInDatabase_ReturnsConflictStatusCode()
    {
        // Arrange: el producto ya esta en la base
        var sku = UniqueSku();
        await SeedProductAsync(_factory, sku);
        var admin = await CreateAdminClientAsync(_factory);

        // Act
        var response = await admin.PostAsJsonAsync("/api/products", NewProductRequest(sku));

        // Assert: 409 con el contrato de error de la API (no un 500)
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Code.Should().Be("PRODUCT_SKU_ALREADY_EXISTS");
    }

    [Fact]
    public async Task GetProducts_PublicEndpoint_ReturnsOkWithPaginatedJsonStructure()
    {
        // Arrange: al menos un producto activo (con la base vacia la API responde 404)
        await SeedProductAsync(_factory, UniqueSku());
        var anonymous = _factory.CreateClient();

        // Act
        var response = await anonymous.GetAsync("/api/products?pageNumber=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>();
        page!.PageNumber.Should().Be(1);
        page.PageSize.Should().Be(5);
        page.TotalCount.Should().BeGreaterThan(0);
        page.Items.Should().NotBeEmpty().And.HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetProducts_WithQueryStringFilters_ReturnsFilteredResultsFromSqlite()
    {
        // Arrange: 3 productos; solo 2 contienen el termino buscado
        var term = $"Q{Guid.NewGuid():N}"[..10];
        var mouse = await SeedProductAsync(_factory, UniqueSku(), name: $"Mouse {term}");
        var pad = await SeedProductAsync(_factory, UniqueSku(), name: $"Pad {term}");
        await SeedProductAsync(_factory, UniqueSku(), name: "Teclado sin coincidencia");
        var anonymous = _factory.CreateClient();

        // Act: el termino viaja por query string (model binding real)
        var response = await anonymous.GetAsync($"/api/products?search={term.ToLower()}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>();
        page!.Items.Select(p => p.Id).Should().BeEquivalentTo(new[] { mouse.Id, pad.Id });
    }

    [Fact]
    public async Task GetAuthProducts_AdminEndpoint_WhenCalledByAdmin_IncludesInactiveProducts()
    {
        // Arrange: un producto INACTIVO con un nombre unico
        var term = $"Q{Guid.NewGuid():N}"[..10];
        var inactive = await SeedProductAsync(_factory, UniqueSku(), active: false, name: $"Inactivo {term}");
        var admin = await CreateAdminClientAsync(_factory);

        // Act
        var adminResponse = await admin.GetAsync($"/api/products/admin?search={term}");
        var publicResponse = await _factory.CreateClient().GetAsync($"/api/products?search={term}");

        // Assert: el endpoint admin lo incluye
        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await adminResponse.Content.ReadFromJsonAsync<PagedResponse<ProductListItemDto>>();
        page!.Items.Should().ContainSingle(p => p.Id == inactive.Id && !p.IsActive);

        // ... y el publico no (sin productos activos que coincidan responde 404)
        publicResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}