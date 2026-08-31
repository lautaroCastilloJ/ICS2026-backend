using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.ProductExceptions;

public sealed class InvalidProductException : ExceptionBase
{
    public InvalidProductException(string code)
        : base(code)
    { }
}