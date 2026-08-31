using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

public sealed class NoProductsAvailableException : ExceptionBase
{
    public NoProductsAvailableException()
        : base("NO_PRODUCTS_AVAILABLE") { }
}
