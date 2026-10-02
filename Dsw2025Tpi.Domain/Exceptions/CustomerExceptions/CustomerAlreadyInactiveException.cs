using Dsw2025Tpi.Shared.Exceptions;


namespace Dsw2025Tpi.Domain.Exceptions.CustomerExceptions;

public sealed class CustomerAlreadyInactiveException : ExceptionBase
{
    public override ErrorType Type => ErrorType.Conflict;

    public Guid CustomerId { get; }

    public CustomerAlreadyInactiveException(Guid id)
        : base("CUSTOMER_ALREADY_INACTIVE")
    {
        CustomerId = id;
    }
}

