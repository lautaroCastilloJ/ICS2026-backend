using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos.Products;

public record ProductResponse(
    Guid Id,
    string Sku,
    string InternalCode,
    string Name,
    string? Description,
    decimal CurrentUnitPrice,
    int StockQuantity,
    string? ImageUrl,
    bool IsActive
);
