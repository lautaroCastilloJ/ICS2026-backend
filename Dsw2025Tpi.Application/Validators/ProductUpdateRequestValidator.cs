using Dsw2025Tpi.Application.Dtos.Products;
using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

public class ProductUpdateRequestValidator : AbstractValidator<ProductUpdateRequest>
{
    public ProductUpdateRequestValidator()
    {
        RuleFor(p => p.Sku)
            .NotEmpty().WithMessage("El SKU es obligatorio.")
            .Length(3, 50).WithMessage("El SKU debe tener entre 3 y 50 caracteres.")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("El SKU solo puede contener letras mayúsculas, números y guiones.");

        RuleFor(p => p.InternalCode)
            .NotEmpty().WithMessage("El código interno es obligatorio.")
            .Length(1, 50).WithMessage("El código interno no puede exceder 50 caracteres.");

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Length(3, 100).WithMessage("El nombre debe tener entre 3 y 100 caracteres.");

        RuleFor(p => p.Description)
            .MaximumLength(250).When(p => p.Description != null)
            .WithMessage("La descripción no puede exceder 250 caracteres.");

        RuleFor(p => p.CurrentUnitPrice)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");

        RuleFor(p => p.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo.");

        // ImageUrl Validation (Optional field)
        RuleFor(p => p.ImageUrl)
            .MaximumLength(500).When(p => !string.IsNullOrWhiteSpace(p.ImageUrl))
            .WithMessage("La URL de la imagen no puede exceder 500 caracteres.")
            .Must(BeAValidUrl).When(p => !string.IsNullOrWhiteSpace(p.ImageUrl))
            .WithMessage("La URL de la imagen debe ser una URL válida.");
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
