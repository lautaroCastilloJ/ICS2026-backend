using Dsw2025Tpi.Application.Dtos.Orders;
using Dsw2025Tpi.Application.Pagination;


namespace Dsw2025Tpi.Application.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(Guid customerId, OrderRequest request);
    // null se reserva para una consulta autorizada de un administrador.
    Task<OrderResponse> GetOrderByIdAsync(Guid id, Guid? requestingCustomerId);
    Task<OrderResponse> UpdateOrderStatusAsync(Guid id, string newStatus);
    Task<PagedResult<OrderListItemDto>> GetPagedAsync(
    FilterOrder filter,
    CancellationToken cancellationToken = default);

}
