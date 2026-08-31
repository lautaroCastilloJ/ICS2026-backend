using Dsw2025Tpi.Application.Dtos.Users;

namespace Dsw2025Tpi.Application.Interfaces;

public interface IUserService
{
    Task<string> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<(string Username, string Role, Guid? CustomerId)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
