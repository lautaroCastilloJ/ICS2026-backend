using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;


public sealed class ProductSkuAlreadyExistsDisabledException : ExceptionBase
{
    public override ErrorType Type => ErrorType.Conflict;

    public string Sku { get; }
    public Guid ExistingProductId { get; }

    public ProductSkuAlreadyExistsDisabledException(string sku, Guid existingProductId)
        : base("PRODUCT_SKU_ALREADY_EXISTS_DISABLED", 
            $"Ya existe un producto deshabilitado con el SKU '{sku}'. Su ID es: {existingProductId}.")
    {
        Sku = sku;
        ExistingProductId = existingProductId;
    }
}
