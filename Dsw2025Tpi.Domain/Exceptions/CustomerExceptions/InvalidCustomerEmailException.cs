using Dsw2025Tpi.Shared.Exceptions;


namespace Dsw2025Tpi.Domain.Exceptions.CustomerExceptions;

public sealed class InvalidCustomerEmailException : ExceptionBase
{
    public string Email { get; }

    public InvalidCustomerEmailException(string email)
        : base("CUSTOMER_INVALID_EMAIL")
    {
        Email = email;
    }
}

