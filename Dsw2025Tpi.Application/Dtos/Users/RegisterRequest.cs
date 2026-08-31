

namespace Dsw2025Tpi.Application.Dtos.Users;

public record RegisterRequest(
    string UserName, 
    string Password, 
    string Email, 
    string DisplayName,
    string? PhoneNumber,
    string? Role
);
