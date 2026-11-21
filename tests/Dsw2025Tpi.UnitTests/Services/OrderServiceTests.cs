using System.Linq.Expressions;
using AutoMapper;
using Dsw2025Tpi.Application.Dtos.Orders;
using Dsw2025Tpi.Application.Mappings;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Exceptions.CustomerExceptions;
using Dsw2025Tpi.Domain.Exceptions.OrderExceptions;
using Dsw2025Tpi.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Dsw2025Tpi.UnitTests.Services;

public class OrderServiceTests
{
    private readonly Mock<IRepository<Order>> _orderRepository = new();
    private readonly Mock<IRepository<Product>> _productRepository = new();
    private readonly Mock<IRepository<Customer>> _customerRepository = new();
    private readonly OrderService _sut; // System Under Test

    private readonly Customer _customer = Customer.Create("cliente@test.com", "Cliente Test", null);

    private static readonly AddressDto Address =
        new("Av. Siempre Viva", "742", "San Miguel de Tucuman", "Tucuman", "4000");

    public OrderServiceTests()
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfiles>()).CreateMapper();
        _sut = new OrderService(_orderRepository.Object, _productRepository.Object, _customerRepository.Object,
            mapper);
    }

    private static OrderRequest Request(params OrderItemRequest[] items) => new(Address, Address, null, items);

    // Configura los mocks para que "existan" el cliente y los productos dados.
    private void Given(Customer? customer, params Product[] products)
    {
        _customerRepository
            .Setup(r => r.GetById(It.IsAny<Guid>(), It.IsAny<string[]>()))
            .ReturnsAsync(customer);
        _productRepository
            .Setup(r => r.GetFiltered(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
            .ReturnsAsync(products);
        _orderRepository
            .Setup(r => r.Add(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);
    }

    [Fact]
    public async Task CreateOrder_WithValidItemsAndStock_CalculatesTotalAndDecrementsProductStock()
    {
        // Arrange: cliente existente y dos productos con stock suficiente
        var mouse = Product.Create("MOUSE-001", "INT-1", "Mouse", "", 1500m, 10);
        var pad = Product.Create("PAD-001", "INT-2", "Pad", "", 2000m, 4);
        Given(_customer, mouse, pad);

        // Act: 3 mouses + 1 pad
        var result = await _sut.CreateOrderAsync(_customer.Id,
            Request(new OrderItemRequest(mouse.Id, 3), new OrderItemRequest(pad.Id, 1)));

        // Assert: total = suma exacta de subtotales
        result.TotalAmount.Should().Be(3 * 1500m + 1 * 2000m);
        // el stock se desconto en las entidades
        mouse.StockQuantity.Should().Be(7);
        pad.StockQuantity.Should().Be(3);
        // la orden se guardo UNA vez; ese unico SaveChanges confirma tambien el stock (Unit of Work)
        _orderRepository.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
        _productRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_WhenProductStockIsInsufficient_ThrowsInsufficientStockException()
    {
        // Arrange: stock 2, se piden 5
        var mouse = Product.Create("MOUSE-001", "INT-1", "Mouse", "", 1500m, 2);
        Given(_customer, mouse);

        // Act
        var act = () => _sut.CreateOrderAsync(_customer.Id, Request(new OrderItemRequest(mouse.Id, 5)));

        // Assert
        await act.Should().ThrowAsync<OrderInsufficientStockException>();
        mouse.StockQuantity.Should().Be(2);                                    // producto intacto
        _orderRepository.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);  // nada persistido
        _productRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrder_WhenCustomerDoesNotExist_ThrowsCustomerNotFoundException()
    {
        // Arrange: el repositorio de clientes devuelve null
        Given(customer: null);

        // Act
        var act = () => _sut.CreateOrderAsync(Guid.NewGuid(), Request(new OrderItemRequest(Guid.NewGuid(), 1)));

        // Assert: falla de inmediato, sin tocar productos ni ordenes
        await act.Should().ThrowAsync<CustomerNotFoundException>();
        _productRepository.Verify(r => r.GetFiltered(It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<string[]>()), Times.Never);
        _productRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _orderRepository.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }
}
