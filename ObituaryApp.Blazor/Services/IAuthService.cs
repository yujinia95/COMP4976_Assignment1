using ObituaryApp.Blazor.Models;

namespace ObituaryApp.Blazor.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    Task<string?> GetUserRoleAsync();
    event Action OnAuthStateChanged;
}