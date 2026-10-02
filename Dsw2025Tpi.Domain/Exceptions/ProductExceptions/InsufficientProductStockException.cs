using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

public sealed class InsufficientProductStockException : ExceptionBase
{
    public override ErrorType Type => ErrorType.BusinessRule;

    public InsufficientProductStockException()
        : base("PRODUCT_INSUFFICIENT_STOCK") { }
}
