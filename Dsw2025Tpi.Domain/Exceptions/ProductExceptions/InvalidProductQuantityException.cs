using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

public sealed class InvalidProductQuantityException : ExceptionBase
{
    public InvalidProductQuantityException()
        : base("PRODUCT_INVALID_QUANTITY") { }
}
