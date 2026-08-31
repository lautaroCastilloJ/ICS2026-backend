using Dsw2025Tpi.Application.Dtos.Users;
using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        // UserName
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .Length(3, 20).WithMessage("El nombre de usuario debe tener entre 3 y 20 caracteres.")
            .Matches(@"^[a-zA-Z0-9_.-]+$").WithMessage("El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos.");

        // Password - Sincronizado con Program.cs (RequiredLength = 8)
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
            .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
            .Matches(@"\d").WithMessage("La contraseña debe contener al menos un número.")
            .Matches(@"[\W_]").WithMessage("La contraseña debe contener al menos un carácter especial.");

        // Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El email no puede exceder los 150 caracteres.");

        // DisplayName -> lo usarás como Name del Customer
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("El nombre para mostrar es obligatorio.")
            .Length(3, 100).WithMessage("El nombre para mostrar debe tener entre 3 y 100 caracteres.");

        // Role (opcional)
        RuleFor(x => x.Role)
            .MaximumLength(50).WithMessage("El rol no puede exceder los 50 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Role));

        // Validar roles permitidos
        RuleFor(x => x.Role)
             .Must(role => new[] { "Administrador", "Cliente" }.Contains(role))
             .When(x => !string.IsNullOrWhiteSpace(x.Role))
             .WithMessage("El rol especificado no es válido. Roles válidos: Administrador, Cliente");
    }
}
