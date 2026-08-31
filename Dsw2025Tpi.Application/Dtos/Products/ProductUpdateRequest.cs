namespace Dsw2025Tpi.Application.Dtos.Products;

public record ProductUpdateRequest(
    string Sku,
    string InternalCode,
    string Name,
    string? Description,
    decimal CurrentUnitPrice,
    int StockQuantity,
    string? ImageUrl
);