using Dsw2025Tpi.Application.Dtos.Orders;
using Dsw2025Tpi.Domain.ValueObjects;
using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

/// <summary>
/// Validacion de entrada de una direccion. Reutiliza los limites del Value
/// Object para que la API y el dominio no puedan desincronizarse.
/// </summary>
public sealed class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(a => a.Street)
            .NotEmpty().WithMessage("La calle es obligatoria.")
            .MaximumLength(Address.StreetMaxLength)
            .WithMessage($"La calle no puede exceder {Address.StreetMaxLength} caracteres.");

        RuleFor(a => a.Number)
            .NotEmpty().WithMessage("La altura es obligatoria (use \"S/N\" si no tiene).")
            .MaximumLength(Address.NumberMaxLength)
            .WithMessage($"La altura no puede exceder {Address.NumberMaxLength} caracteres.");

        RuleFor(a => a.City)
            .NotEmpty().WithMessage("La ciudad es obligatoria.")
            .MaximumLength(Address.CityMaxLength)
            .WithMessage($"La ciudad no puede exceder {Address.CityMaxLength} caracteres.");

        RuleFor(a => a.Province)
            .NotEmpty().WithMessage("La provincia es obligatoria.")
            .MaximumLength(Address.ProvinceMaxLength)
            .WithMessage($"La provincia no puede exceder {Address.ProvinceMaxLength} caracteres.");

        RuleFor(a => a.PostalCode)
            .NotEmpty().WithMessage("El código postal es obligatorio.")
            .Matches(@"^(\d{4}|[A-Za-z]\d{4}[A-Za-z]{3})$")
            .WithMessage("El código postal debe tener 4 dígitos (ej. 4000) o formato CPA (ej. T4000ABC).");
    }
}
