using Dsw2025Tpi.Shared.Exceptions;

namespace Dsw2025Tpi.Domain.Exceptions.CustomerExceptions;

/// <summary>
/// El cliente indicado no existe. La API la traduce a 404 (ErrorType.NotFound).
/// </summary>
public sealed class CustomerNotFoundException : ExceptionBase
{
    public override ErrorType Type => ErrorType.NotFound;

    public Guid CustomerId { get; }

    public CustomerNotFoundException(Guid customerId)
        : base("CUSTOMER_NOT_FOUND")
    {
        CustomerId = customerId;
    }
}
