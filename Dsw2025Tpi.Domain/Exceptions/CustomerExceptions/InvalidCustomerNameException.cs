using Dsw2025Tpi.Shared.Exceptions;


namespace Dsw2025Tpi.Domain.Exceptions.CustomerExceptions;

public sealed class InvalidCustomerNameException : ExceptionBase
{
    public string NameValue { get; }

    public InvalidCustomerNameException(string name)
        : base("CUSTOMER_INVALID_NAME")
    {
        NameValue = name;
    }
}

