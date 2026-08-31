using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;


public sealed class ProductInternalCodeAlreadyExistsDisabledException : ExceptionBase
{
    public string InternalCode { get; }
    public Guid ExistingProductId { get; }

    public ProductInternalCodeAlreadyExistsDisabledException(string internalCode, Guid existingProductId)
        : base("PRODUCT_INTERNAL_CODE_ALREADY_EXISTS_DISABLED", 
            $"Ya existe un producto deshabilitado con el código interno '{internalCode}'. Su ID es: {existingProductId}.")
    {
        InternalCode = internalCode;
        ExistingProductId = existingProductId;
    }
}
