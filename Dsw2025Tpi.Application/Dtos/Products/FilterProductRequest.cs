

namespace Dsw2025Tpi.Application.Dtos.Products;

public record FilterProductRequest(
    string? Status,
    string? Search,
    int? PageNumber,
    int? PageSize
);
