using Dsw2025Tpi.Shared.Exceptions;

public sealed class ProductNotFoundException : ExceptionBase
{
    public override ErrorType Type => ErrorType.NotFound;

    public ProductNotFoundException(Guid productId)
        : base("PRODUCT_NOT_FOUND")
    {
        ProductId = productId;
    }

    public Guid ProductId { get; }
}
