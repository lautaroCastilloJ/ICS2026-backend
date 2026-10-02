using Dsw2025Tpi.Shared.Resources;

namespace Dsw2025Tpi.Shared.Exceptions;

public abstract class ExceptionBase : Exception
{
    public string Code { get; }

    /// <summary>
    /// Categoria del error. Por defecto, error de validacion; cada excepcion la
    /// sobrescribe cuando representa otro tipo de falla.
    /// </summary>
    public virtual ErrorType Type => ErrorType.Validation;

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
        => ErrorMessages.Get(code);
}
