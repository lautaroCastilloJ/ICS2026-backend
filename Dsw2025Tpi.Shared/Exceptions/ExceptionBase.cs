using Dsw2025Tpi.Shared.Resources;

namespace Dsw2025Tpi.Shared.Exceptions;

public abstract class ExceptionBase : Exception
{
    public string Code { get; }

    protected ExceptionBase(string code)
        : base(GetMessageFromResource(code))
    {
        Code = code;
    }

    protected ExceptionBase(string code, Exception innerException)
        : base(GetMessageFromResource(code), innerException)
    {
        Code = code;
    }

    protected ExceptionBase(string code, string? customMessage)
        : base(customMessage ?? GetMessageFromResource(code))
    {
        Code = code;
    }

    private static string GetMessageFromResource(string code)
    {
        // Convertir dots "PRODUCT.INVALID_PRICE" a underscore si tu resx lo requiere
        // Soporta dos formatos:
        // - "PRODUCT.INVALID_PRICE"
        // - "PRODUCT_INVALID_PRICE"
        string key = code.Replace(".", "_");

        return Messages.ResourceManager.GetString(key)
            ?? $"Unknown error code: {code}";
    }
}

