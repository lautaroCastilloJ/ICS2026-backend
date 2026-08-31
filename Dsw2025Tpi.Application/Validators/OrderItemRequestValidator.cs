using Dsw2025Tpi.Application.Dtos.Orders;
using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

public sealed class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(i => i.ProductId)
            .NotEmpty().WithMessage("El identificador del producto es obligatorio.");

        RuleFor(i => i.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.")
            .LessThanOrEqualTo(999_999).WithMessage("La cantidad no puede exceder 999.999 unidades.");
    }
}
