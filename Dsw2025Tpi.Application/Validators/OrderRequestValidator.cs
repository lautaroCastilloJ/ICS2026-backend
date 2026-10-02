using Dsw2025Tpi.Application.Dtos.Orders;
using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

public sealed class OrderRequestValidator : AbstractValidator<OrderRequest>
{
    public OrderRequestValidator()
    {
        RuleFor(o => o.ShippingAddress)
            .NotNull().WithMessage("La dirección de envío es obligatoria.")
            .SetValidator(new AddressDtoValidator());

        RuleFor(o => o.BillingAddress)
            .NotNull().WithMessage("La dirección de facturación es obligatoria.")
            .SetValidator(new AddressDtoValidator());

        RuleFor(o => o.Notes)
            .MaximumLength(500)
            .When(o => o.Notes is not null)
            .WithMessage("Las notas no pueden exceder 500 caracteres.");

        RuleFor(o => o.OrderItems)
            .NotNull().WithMessage("La orden debe incluir al menos un ítem.")
            .Must(items => items != null && items.Any())
            .WithMessage("La orden debe incluir al menos un ítem.");

        RuleForEach(o => o.OrderItems)
            .SetValidator(new OrderItemRequestValidator());
    }
}
