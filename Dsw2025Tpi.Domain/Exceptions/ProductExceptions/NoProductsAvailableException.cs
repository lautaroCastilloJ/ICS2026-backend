using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

public sealed class NoProductsAvailableException : ExceptionBase
{
    public override ErrorType Type => ErrorType.NotFound;

    public NoProductsAvailableException()
        : base("NO_PRODUCTS_AVAILABLE") { }
}
