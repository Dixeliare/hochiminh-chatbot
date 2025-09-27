using Data;

namespace Services.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(string username, string email, string password, string? fullName = null);
    Task<string> LoginAsync(string username, string password);
    Task<user?> GetUserByUsernameAsync(string username);
    Task<user?> GetUserByEmailAsync(string email);
    Task<bool> ValidateTokenAsync(string token);
    string GenerateJwtToken(user user);
}