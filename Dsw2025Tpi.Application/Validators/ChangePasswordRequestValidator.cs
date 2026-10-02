using Dsw2025Tpi.Application.Dtos.Users;
using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("La contraseña actual es obligatoria.");

        // La longitud minima de administradores (12) se controla en el servicio,
        // porque depende del rol del usuario y no solo de la request.
        RuleFor(x => x.NewPassword)
            .ValidPassword()
            .NotEqual(x => x.CurrentPassword).WithMessage("La nueva contraseña debe ser distinta de la actual.");
    }
}
