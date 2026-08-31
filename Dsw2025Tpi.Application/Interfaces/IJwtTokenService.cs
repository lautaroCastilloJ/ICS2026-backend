namespace Dsw2025Tpi.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(string username, string role, Guid? customerId = null);
}
