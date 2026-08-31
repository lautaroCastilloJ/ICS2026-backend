using Dsw2025Tpi.Api.Examples;
using Dsw2025Tpi.Api.Extensions;
using Dsw2025Tpi.Api.Filters;
using Dsw2025Tpi.Application.Dtos.Orders;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Data.Identity;
using Dsw2025Tpi.Domain.Exceptions.OrderExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Claims;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }


    // ----------------------------------------------------------------------
    // 6. Crear una nueva orden: POST /api/orders 
    // Solo rol Cliente puede crear órdenes
    // ----------------------------------------------------------------------
    [HttpPost]
    [Authorize(Roles = AppRoles.Cliente)]
    [ValidateCustomerId]
    [SwaggerRequestExample(typeof(OrderRequest), typeof(OrderRequestExample))]
    public async Task<IActionResult> Create([FromBody] OrderRequest request)
    {
        var customerId = HttpContext.GetCustomerId();

        var created = await _orderService.CreateOrderAsync(customerId, request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created
        );
    }

    // ----------------------------------------------------------------------
    // 7. Obtener orden por ID: GET /api/orders/{id}
    // ----------------------------------------------------------------------
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{AppRoles.Cliente},{AppRoles.Administrador}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _orderService.GetOrderByIdAsync(id);
        return Ok(result);
    }

    // ----------------------------------------------------------------------
    // 8. Obtener mis órdenes (Cliente): GET /api/orders/my-orders
    // Solo puede ver sus propias órdenes, customerId viene del token
    // Búsqueda en: direcciones de envío, facturación y notas
    // ----------------------------------------------------------------------
    [HttpGet("my-orders")]
    [Authorize(Roles = AppRoles.Cliente)]
    [ValidateCustomerId]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize)
    {
        var customerId = HttpContext.GetCustomerId();

        var filter = new FilterOrder(
            Status: status,
            CustomerId: customerId,
            CustomerName: null,  // No tiene sentido filtrar por nombre propio
            Search: search,
            PageNumber: pageNumber,
            PageSize: pageSize
        );

        var pagedResult = await _orderService.GetPagedAsync(filter);

        return Ok(pagedResult);
    }

    // ----------------------------------------------------------------------
    // 9. Obtener todas las órdenes (Administrador): GET /api/orders/admin
    // Puede filtrar por customerId, customerName, status, búsqueda general, con paginación
    // ----------------------------------------------------------------------
    [HttpGet("admin")]
    [Authorize(Roles = AppRoles.Administrador)]
    public async Task<IActionResult> GetAllForAdmin([FromQuery] FilterOrder filter)
    {
        var pagedResult = await _orderService.GetPagedAsync(filter);

        return Ok(pagedResult);
    }

    // ----------------------------------------------------------------------
    // 10. Actualizar estado de orden (Administrador): PUT /api/orders/{id}/status
    // Solo administradores pueden cambiar el estado de una orden
    // ----------------------------------------------------------------------
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = AppRoles.Administrador)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        var updated = await _orderService.UpdateOrderStatusAsync(id, request.NewStatus);

        return Ok(updated);
    }
}