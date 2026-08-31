using Dsw2025Tpi.Application.Dtos.Products;
using Dsw2025Tpi.Application.Pagination;

namespace Dsw2025Tpi.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse?> GetByIdAsync(Guid id);
    Task<PagedResult<ProductResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10, string? search = null);
    Task<ProductResponse> CreateAsync(ProductRequest request);
    Task<ProductResponse?> UpdateAsync(Guid productId, ProductUpdateRequest request);
    Task DisableAsync(Guid productId);
    Task EnableAsync(Guid productId);
    Task<PagedResult<ProductListItemDto>> GetPagedAsync(FilterProductRequest filter, CancellationToken cancellationToken = default);
}
