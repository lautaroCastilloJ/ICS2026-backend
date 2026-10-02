using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

public sealed class ProductCannotBeDisabledException : ExceptionBase
{
    public override ErrorType Type => ErrorType.BusinessRule;

    public ProductCannotBeDisabledException()
        : base("PRODUCT_CANNOT_BE_DISABLED") { }
}
