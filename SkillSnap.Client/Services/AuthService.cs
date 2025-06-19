using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.JSInterop;

namespace SkillSnap.Client.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly CustomAuthenticationStateProvider _authProvider;

    public event Action? AuthStateChanged;

    public AuthService(HttpClient http, IJSRuntime js, CustomAuthenticationStateProvider authProvider)
    {
        _http = http;
        _js = js;
        _authProvider = authProvider;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResult>();
            if (result?.token != null)
            {
                await _js.InvokeVoidAsync("localStorage.setItem", "authToken", result.token);
                NotifyAuthChanged();
                return true;
            }
        }
        return false;
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
        NotifyAuthChanged();
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrWhiteSpace(token) && !IsTokenExpired(token);
    }

    public async Task<string?> GetCurrentUserAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return null;

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Try to get email claim, fallback to sub
        return jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email")?.Value
            ?? jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
    }

    public async Task<IEnumerable<Claim>> GetClaimsAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) return Enumerable.Empty<Claim>();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims;
    }

    private bool IsTokenExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.ValidTo < DateTime.UtcNow;
    }

    private void NotifyAuthChanged()
    {
        _authProvider.NotifyUserAuthenticationChanged();
        AuthStateChanged?.Invoke();
    }

    private class LoginResult
    {
#if NET7_0_OR_GREATER
        public required string token { get; set; }  // C# 11 required keyword
#else
        public string token { get; set; } = string.Empty; // Fallback for earlier versions
#endif
    }
}
