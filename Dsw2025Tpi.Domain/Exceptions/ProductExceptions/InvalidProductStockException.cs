using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

public sealed class InvalidProductStockException : ExceptionBase
{
    public InvalidProductStockException()
        : base("PRODUCT_INVALID_STOCK") { }
}
