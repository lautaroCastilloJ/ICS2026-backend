using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

public sealed class InsufficientProductStockException : ExceptionBase
{
    public InsufficientProductStockException()
        : base("PRODUCT_INSUFFICIENT_STOCK") { }
}
