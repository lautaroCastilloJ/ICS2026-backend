namespace Dsw2025Tpi.Shared.Resources;

/// <summary>
/// Acceso al catalogo de mensajes de error (Messages.resx) a partir de un codigo.
/// </summary>
public static class ErrorMessages
{
    // Codigos genericos que no corresponden a una excepcion concreta.
    public const string UnexpectedError = "UNEXPECTED_ERROR";
    public const string ValidationError = "VALIDATION_ERROR";

    public static string Get(string code)
    {
        // Soporta dos formatos:
        // - "PRODUCT.INVALID_PRICE"
        // - "PRODUCT_INVALID_PRICE"
        string key = code.Replace(".", "_");

        return Messages.ResourceManager.GetString(key)
            ?? $"Unknown error code: {code}";
    }
}
