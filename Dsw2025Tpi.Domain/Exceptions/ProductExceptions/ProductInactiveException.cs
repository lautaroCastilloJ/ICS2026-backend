using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

public sealed class ProductInactiveException : ExceptionBase
{
    public ProductInactiveException()
        : base("PRODUCT_INACTIVE") { }
}
