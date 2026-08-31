using Dsw2025Tpi.Domain.Exceptions.CustomerExceptions;


namespace Dsw2025Tpi.Domain.Entities;

public sealed class Customer : EntityBase
{
    public string Email { get; private set; }
    public string Name { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }

    private Customer() { } // Ctor para EF Core

    private Customer(string email, string name, string? phoneNumber)
    {
        SetEmail(email);
        SetName(name);
        SetPhoneNumber(phoneNumber);
        IsActive = true;
    }

    public static Customer Create(string email, string name, string? phoneNumber)
        => new Customer(email, name, phoneNumber);

    
    // ----------------- Comportamiento de dominio -----------------

    public void UpdateContactInfo(string email, string name, string? phoneNumber)
    {
        EnsureActive();

        SetEmail(email);
        SetName(name);
        SetPhoneNumber(phoneNumber);
    }

    public void ChangeEmail(string email)
    {
        EnsureActive();
        SetEmail(email);
    }

    public void ChangeName(string name)
    {
        EnsureActive();
        SetName(name);
    }

    public void ChangePhoneNumber(string? phoneNumber)
    {
        EnsureActive();
        SetPhoneNumber(phoneNumber);
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new CustomerAlreadyInactiveException(Id);

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
    }

    // ----------------- Invariantes internas -----------------

    private void EnsureActive()
    {
        if (!IsActive)
            throw new CustomerInactiveException(Id);
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidCustomerEmailException(email);

        var trimmed = email.Trim();

        // Validación simple de dominio, el resto con FluentValidation en la capa de Aplicación con el RequestValidator
        if (!trimmed.Contains("@"))
            throw new InvalidCustomerEmailException(trimmed);

        Email = trimmed;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCustomerNameException(name);

        Name = name.Trim();
    }

    private void SetPhoneNumber(string? phoneNumber)
    {
        // PhoneNumber es opcional, permite null
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            PhoneNumber = phoneNumber;
            return;
        }

        PhoneNumber = phoneNumber.Trim();
    }
}
