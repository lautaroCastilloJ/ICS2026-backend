using Dsw2025Tpi.Shared.Exceptions;


namespace Dsw2025Tpi.Domain.Exceptions.CustomerExceptions;

public sealed class CustomerInactiveException : ExceptionBase
{
    public Guid CustomerId { get; }

    public CustomerInactiveException(Guid customerId)
        : base("CUSTOMER_INACTIVE")
    {
        CustomerId = customerId;
    }
}

