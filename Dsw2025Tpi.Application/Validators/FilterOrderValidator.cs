using Dsw2025Tpi.Domain.Enums;
using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

public sealed class FilterOrderValidator : AbstractValidator<Dsw2025Tpi.Application.Dtos.Orders.FilterOrder>
{
    private static readonly string[] AllowedStatuses = Enum.GetNames(typeof(OrderStatus));

    public FilterOrderValidator()
    {
        RuleFor(x => x.Status)
            .Must(BeValidStatus)
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage($"El estado especificado no es válido. Estados válidos: {string.Join(", ", AllowedStatuses)}");

        RuleFor(f => f.PageNumber)
            .GreaterThan(0).When(f => f.PageNumber.HasValue)
            .WithMessage("El número de página debe ser mayor que 0.");

        RuleFor(f => f.PageSize)
            .GreaterThan(0).When(f => f.PageSize.HasValue)
            .WithMessage("El tamaño de página debe ser mayor que 0.")
            .LessThanOrEqualTo(100).When(f => f.PageSize.HasValue)
            .WithMessage("El tamaño máximo de página permitido es 100.");

        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerName))
            .WithMessage("El nombre del cliente no puede exceder 100 caracteres.");

        RuleFor(x => x.Search)
            .MaximumLength(250)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithMessage("El término de búsqueda no puede exceder 250 caracteres.");
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return true;

        return AllowedStatuses.Any(s =>
            string.Equals(s, status, StringComparison.OrdinalIgnoreCase));
    }
}
