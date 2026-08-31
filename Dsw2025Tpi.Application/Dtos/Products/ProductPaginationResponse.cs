using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos.Products;

public record ProductPaginationResponse(
    IEnumerable<ProductResponse> ProductItems,
    int Total
);

// Response DTO para paginación de productos