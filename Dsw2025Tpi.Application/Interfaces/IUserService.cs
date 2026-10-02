using Dsw2025Tpi.Application.Dtos.Users;

namespace Dsw2025Tpi.Application.Interfaces;

public interface IUserService
{
    /// <summary>Registro publico: crea siempre un usuario Cliente con su Customer.</summary>
    Task<string> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>Crea un administrador. Debe invocarse solo desde un endpoint protegido.</summary>
    Task<string> CreateAdminAsync(CreateAdminRequest request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>Cambia la contraseña del usuario indicado (el autenticado), verificando la actual.</summary>
    Task ChangePasswordAsync(string userName, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<(string Username, string Role, Guid? CustomerId)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
