using FluentValidation;

namespace Dsw2025Tpi.Application.Validators;

/// <summary>
/// Reglas de usuario compartidas por los validadores de registro y de alta
/// de administradores.
/// </summary>
public static class UserRules
{
    // Sincronizado con la politica de Identity en Program.cs (RequiredLength = 8)
    public const int MinPasswordLength = 8;

    // Los administradores tienen una politica mas exigente.
    public const int MinAdminPasswordLength = 12;

    public static IRuleBuilderOptions<T, string> ValidUserName<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .Length(3, 20).WithMessage("El nombre de usuario debe tener entre 3 y 20 caracteres.")
            .Matches(@"^[a-zA-Z0-9_.-]+$").WithMessage("El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos.");

    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> rule, int minLength = MinPasswordLength)
        => rule
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(minLength).WithMessage($"La contraseña debe tener al menos {minLength} caracteres.")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
            .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
            .Matches(@"\d").WithMessage("La contraseña debe contener al menos un número.")
            .Matches(@"[\W_]").WithMessage("La contraseña debe contener al menos un carácter especial.");

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El email no tiene un formato válido.")
            .MaximumLength(150).WithMessage("El email no puede exceder los 150 caracteres.");

    public static IRuleBuilderOptions<T, string> ValidDisplayName<T>(this IRuleBuilder<T, string> rule)
        => rule
            .NotEmpty().WithMessage("El nombre para mostrar es obligatorio.")
            .Length(3, 100).WithMessage("El nombre para mostrar debe tener entre 3 y 100 caracteres.");
}
