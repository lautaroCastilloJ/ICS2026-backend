using System.Linq.Expressions;
using AutoMapper;
using Dsw2025Tpi.Application.Dtos.Products;
using Dsw2025Tpi.Application.Mappings;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Exceptions.ProductExceptions;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Shared.Exceptions;
using FluentAssertions;
using Moq;

namespace Dsw2025Tpi.UnitTests.Services;

public class ProductServiceTests
{
    // Dependencias del servicio: el repositorio es un Mock (no hay base de
    // datos) y el mapper es el REAL, con los perfiles de produccion.
    private readonly Mock<IRepository<Product>> _productRepository = new();
    private readonly IMapper _mapper =
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>()).CreateMapper();
    private readonly ProductService _sut; // System Under Test

    public ProductServiceTests()
    {
        _sut = new ProductService(_productRepository.Object, _mapper);
    }

    private static ProductRequest Request(decimal price = 1500m, int stock = 10, string sku = "MOUSE-001") =>
        new(sku, "INT-001", "Mouse Gamer", "Mouse RGB", price, stock, null);

    [Fact]
    public async Task AddProduct_WithValidData_ReturnsProductAndCallsRepositoryAdd()
    {
        // Arrange: no existe ningun producto con ese SKU o codigo interno
        _productRepository
            .Setup(r => r.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
            .ReturnsAsync((Product?)null);
        _productRepository
            .Setup(r => r.Add(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        // Act
        var result = await _sut.CreateAsync(Request());

        // Assert
        result.Sku.Should().Be("MOUSE-001");
        result.StockQuantity.Should().Be(10);
        _productRepository.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task AddProduct_WithDuplicateSku_ThrowsDuplicatedEntityException()
    {
        // Arrange: el repositorio "encuentra" un producto activo con el mismo SKU
        var existing = Product.Create("MOUSE-001", "OTRO-COD", "Mouse viejo", "", 900m, 3);
        _productRepository
            .Setup(r => r.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
            .ReturnsAsync(existing);

        // Act
        var act = () => _sut.CreateAsync(Request());

        // Assert
        await act.Should().ThrowAsync<ProductSkuAlreadyExistsException>();
        _productRepository.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }

    [Theory]
    [InlineData(1500, 0)]   // stock en cero
    [InlineData(1500, -1)]  // stock negativo
    [InlineData(-10, 5)]    // precio negativo
    public async Task AddProduct_WithInvalidStockOrPrice_ThrowsInvalidDataException(decimal price, int stock)
    {
        // Arrange: no hace falta configurar el mock, no deberia usarse

        // Act
        var act = () => _sut.CreateAsync(Request(price, stock));

        // Assert: falla con un error de validacion (400) ...
        var error = await act.Should().ThrowAsync<ExceptionBase>();
        error.Which.Code.Should().BeOneOf("PRODUCT_INVALID_PRICE", "PRODUCT_INVALID_STOCK");

        // ... y el repositorio NUNCA fue consultado ni modificado (CA 3)
        _productRepository.Verify(r => r.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()),
            Times.Never);
        _productRepository.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task GetProducts_WhenIncludeInactiveIsFalse_ReturnsOnlyActiveProducts()
    {
        // Arrange: lista mixta de activos e inactivos
        var active = Product.Create("ACT-001", "A1", "Activo 1", "", 100m, 5);
        var active2 = Product.Create("ACT-002", "A2", "Activo 2", "", 100m, 5);
        var inactive = Product.Create("INA-001", "I1", "Inactivo", "", 100m, 5);
        inactive.Deactivate();
        _productRepository
            .Setup(r => r.GetAllQueryable(It.IsAny<string[]>()))
            .Returns(new[] { active, inactive, active2 }.AsQueryable());

        // Act: el catalogo publico (GetAllAsync) nunca incluye inactivos
        var result = await _sut.GetAllAsync(1, 10);

        // Assert
        result.Items.Should().OnlyContain(p => p.IsActive);
        result.Items.Select(p => p.Sku).Should().BeEquivalentTo(new[] { "ACT-001", "ACT-002" });
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetProducts_WithPaginationFilter_AppliesSkipAndTakeCorrectly()
    {
        // Arrange: 15 productos activos SKU-01 .. SKU-15
        var products = Enumerable.Range(1, 15)
            .Select(i => Product.Create($"SKU-{i:00}", $"INT-{i:00}", $"Producto {i:00}", "", 100m, 5))
            .ToList();
        _productRepository
            .Setup(r => r.GetAllQueryable(It.IsAny<string[]>()))
            .Returns(products.AsQueryable());

        // Act: segunda pagina de 5
        var result = await _sut.GetAllAsync(pageNumber: 2, pageSize: 5);

        // Assert
        result.TotalCount.Should().Be(15);
        result.Items.Should().HaveCount(5);
        result.Items.Select(p => p.Sku).Should().Equal("SKU-06", "SKU-07", "SKU-08", "SKU-09", "SKU-10");
    }

    [Fact]
    public async Task GetProducts_WithSearchTerm_FiltersByNameSkuOrInternalCode()
    {
        // Arrange
        var products = new List<Product>
        {
            Product.Create("PER-001", "GAM-01", "Mouse Gamer", "", 100m, 5),
            Product.Create("PER-002", "MOUSE-PAD", "Pad XL", "", 100m, 5),  // coincide por codigo interno
            Product.Create("PER-003", "TEC-01", "Teclado Mecanico", "", 100m, 5),
            Product.Create("PER-004", "MON-01", "Monitor", "", 100m, 5),
        };
        _productRepository
            .Setup(r => r.GetAllQueryable(It.IsAny<string[]>()))
            .Returns(products.AsQueryable());

        // Act: busqueda en minusculas
        var result = await _sut.GetAllAsync(1, 10, "mouse");

        // Assert
        result.Items.Select(p => p.Sku).Should().BeEquivalentTo(new[] { "PER-001", "PER-002" });
    }
}
