using System.Net.Http.Json;
using SkillSnap.Shared.Models;
using System.Net.Http;
using Microsoft.JSInterop;

/// <summary>
/// Service for managing PortfolioUser CRUD operations with the API.
/// </summary>
public class PortfolioUserService // : IPortfolioUserService (recommended for abstraction)
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public PortfolioUserService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    /// <summary>
    /// Get all portfolio users from the API.
    /// </summary>
    public async Task<List<PortfolioUser>> GetPortfolioUsersAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<PortfolioUser>>("api/portfoliousers").ConfigureAwait(false) ?? new List<PortfolioUser>();
        }
        catch
        {
            // TODO: Add logging here if desired
            return new List<PortfolioUser>();
        }
    }

    /// <summary>
    /// Get a single portfolio user by its ID.
    /// </summary>
    public async Task<PortfolioUser?> GetPortfolioUserByIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<PortfolioUser>($"api/portfoliousers/{id}").ConfigureAwait(false);
        }
        catch
        {
            // TODO: Add logging here if desired
            return null;
        }
    }

    /// <summary>
    /// Add a new portfolio user (requires authentication).
    /// </summary>
    public async Task AddPortfolioUserAsync(PortfolioUser newUser)
    {
        await AttachTokenAsync().ConfigureAwait(false);
        var response = await _http.PostAsJsonAsync("api/portfoliousers", newUser).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Update an existing portfolio user (requires authentication).
    /// </summary>
    public async Task UpdatePortfolioUserAsync(PortfolioUser user)
    {
        await AttachTokenAsync().ConfigureAwait(false);
        var response = await _http.PutAsJsonAsync($"api/portfoliousers/{user.Id}", user).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Delete a portfolio user by ID (requires Admin role).
    /// </summary>
    public async Task DeletePortfolioUserAsync(int id)
    {
        await AttachTokenAsync().ConfigureAwait(false);
        var response = await _http.DeleteAsync($"api/portfoliousers/{id}").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Helper method to attach the JWT token to the Authorization header.
    /// </summary>
    private async Task AttachTokenAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken").ConfigureAwait(false);
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
