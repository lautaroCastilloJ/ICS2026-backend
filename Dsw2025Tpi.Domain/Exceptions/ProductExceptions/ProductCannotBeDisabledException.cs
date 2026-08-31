using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

public sealed class ProductCannotBeDisabledException : ExceptionBase
{
    public ProductCannotBeDisabledException()
        : base("PRODUCT_CANNOT_BE_DISABLED") { }
}
