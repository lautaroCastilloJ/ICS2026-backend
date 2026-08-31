using Dsw2025Tpi.Application.Dtos.Orders;
using Dsw2025Tpi.Domain.Enums;
using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

public sealed class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    private static readonly string[] AllowedStatuses =
        Enum.GetNames(typeof(OrderStatus));

    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("El nuevo estado es obligatorio.")
            .Must(BeValidStatus)
            .WithMessage($"El estado especificado no es válido. Estados válidos: {string.Join(", ", AllowedStatuses)}");
    }

    private static bool BeValidStatus(string newStatus)
        => AllowedStatuses.Any(s =>
            string.Equals(s, newStatus, StringComparison.OrdinalIgnoreCase));
}
