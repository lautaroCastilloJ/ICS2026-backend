using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.OrderExceptions;

public sealed class InvalidOrderStatusException : ExceptionBase
{
    public string Status { get; }

    public InvalidOrderStatusException(string status)
        : base("ORDER_INVALID_STATUS")
    {
        Status = status;
    }
}
