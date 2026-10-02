using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;


public sealed class ProductInternalCodeAlreadyExistsException : ExceptionBase
{
    public override ErrorType Type => ErrorType.Conflict;

    public string InternalCode { get; }
    public Guid ExistingProductId { get; }

    public ProductInternalCodeAlreadyExistsException(string internalCode, Guid existingProductId)
        : base("PRODUCT_INTERNAL_CODE_ALREADY_EXISTS", 
            $"Ya existe un producto activo con el código interno '{internalCode}'.")
    {
        InternalCode = internalCode;
        ExistingProductId = existingProductId;
    }
}
