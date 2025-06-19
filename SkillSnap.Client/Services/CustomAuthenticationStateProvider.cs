using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace SkillSnap.Client.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;

    public CustomAuthenticationStateProvider(IJSRuntime js)
    {
        _js = js;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
        var identity = new ClaimsIdentity();

        if (!string.IsNullOrWhiteSpace(token) && !IsTokenExpired(token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            identity = new ClaimsIdentity(jwt.Claims, "jwt");
        }

        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public void NotifyUserAuthenticationChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private bool IsTokenExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.ValidTo < DateTime.UtcNow;
    }
}
