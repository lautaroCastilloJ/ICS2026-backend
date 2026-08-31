using Dsw2025Tpi.Shared.Exceptions;


namespace Dsw2025Tpi.Domain.Exceptions.CustomerExceptions;

public sealed class InvalidCustomerPhoneException : ExceptionBase
{
    public string Phone { get; }

    public InvalidCustomerPhoneException(string phone)
        : base("CUSTOMER_INVALID_PHONE")
    {
        Phone = phone;
    }
}
