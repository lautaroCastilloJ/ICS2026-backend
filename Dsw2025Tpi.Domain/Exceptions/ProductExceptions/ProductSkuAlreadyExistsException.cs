using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;


public sealed class ProductSkuAlreadyExistsException : ExceptionBase
{
    public string Sku { get; }
    public Guid ExistingProductId { get; }

    public ProductSkuAlreadyExistsException(string sku, Guid existingProductId)
        : base("PRODUCT_SKU_ALREADY_EXISTS", 
            $"Ya existe un producto activo con el SKU '{sku}'.")
    {
        Sku = sku;
        ExistingProductId = existingProductId;
    }
}
