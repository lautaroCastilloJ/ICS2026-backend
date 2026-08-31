using Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

namespace Dsw2025Tpi.Domain.Entities;

public sealed class Product : EntityBase
{
    public string Sku { get; private set; }
    public string InternalCode { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal CurrentUnitPrice { get; private set; }
    public int StockQuantity { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { } // EF Core

    private Product(
        string sku,
        string internalCode,
        string name,
        string description,
        decimal price,
        int stock,
        string? imageUrl = null)
    {
        SetSku(sku);
        SetInternalCode(internalCode);
        SetName(name);
        SetDescription(description);
        SetPrice(price);
        SetStock(stock);
        SetImageUrl(imageUrl);
        IsActive = true;
    }

    public static Product Create(
        string sku,
        string internalCode,
        string name,
        string description,
        decimal price,
        int stock,
        string? imageUrl = null)
        => new Product(sku, internalCode, name, description, price, stock, imageUrl);

    // --------- Update method ---------

    public void UpdateDetails(string sku, string internalCode, string name, string description, decimal price, int stock, string? imageUrl = null)
    {
        SetSku(sku);
        SetInternalCode(internalCode);
        SetName(name);
        SetDescription(description);
        SetPrice(price);
        SetStock(stock);
        SetImageUrl(imageUrl);
    }

    // --------- ---------

    public void UpdatePrice(decimal newPrice)
    {
        EnsureActive();

        if (newPrice <= 0)
            throw new InvalidProductPriceException();

        CurrentUnitPrice = newPrice;
    }

    public void AddStock(int quantity)
    {
        EnsureActive();

        if (quantity <= 0)
            throw new InvalidProductQuantityException();

        StockQuantity += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        EnsureActive();

        if (quantity <= 0)
            throw new InvalidProductQuantityException();

        if (StockQuantity < quantity)
            throw new InsufficientProductStockException();

        StockQuantity -= quantity;
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }


    public bool HasSufficientStock(int requestedQty) =>
        IsActive && requestedQty > 0 && StockQuantity >= requestedQty;

    // --------- Private invariant methods ---------

    private void EnsureActive()
    {
        if (!IsActive)
            throw new ProductInactiveException();
    }

    private void SetSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new InvalidProductException("PRODUCT_INVALID_SKU");

        Sku = sku.Trim();
    }

    private void SetInternalCode(string internalCode)
    {
        if (string.IsNullOrWhiteSpace(internalCode))
            throw new InvalidProductException("PRODUCT_INVALID_INTERNAL_CODE");

        InternalCode = internalCode.Trim();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidProductException("PRODUCT_INVALID_NAME");

        Name = name.Trim();
    }

    private void SetDescription(string desc)
        => Description = desc?.Trim() ?? string.Empty;

    private void SetPrice(decimal price)
    {
        if (price <= 0)
            throw new InvalidProductPriceException();

        CurrentUnitPrice = price;
    }

    private void SetStock(int stock)
    {
        if (stock < 0)
            throw new InvalidProductStockException();

        StockQuantity = stock;
    }

    private void SetImageUrl(string? imageUrl)
        => ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
}

