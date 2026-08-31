using Dsw2025Tpi.Application.Dtos.Products;
using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

public class FilterProductValidator : AbstractValidator<FilterProductRequest>
{
    public FilterProductValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).When(x => x.PageNumber.HasValue)
            .WithMessage("El número de página debe ser mayor a 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).When(x => x.PageSize.HasValue)
            .WithMessage("El tamaño de página debe ser mayor a 0.")
            .LessThanOrEqualTo(100).WithMessage("El tamaño máximo permitido es 100.");
    }
}