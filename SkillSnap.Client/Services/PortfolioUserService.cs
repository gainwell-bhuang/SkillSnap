using System.Net.Http.Json;
using SkillSnap.Shared.Models;
using System.Net.Http;
using Microsoft.JSInterop;

public class PortfolioUserService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public PortfolioUserService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<List<PortfolioUser>> GetPortfolioUsersAsync()
    {
        return await _http.GetFromJsonAsync<List<PortfolioUser>>("api/portfoliousers");
    }

    public async Task<PortfolioUser?> GetPortfolioUserByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<PortfolioUser>($"api/portfoliousers/{id}");
    }

    public async Task AddPortfolioUserAsync(PortfolioUser newUser)
    {
        await AttachTokenAsync();
        var response = await _http.PostAsJsonAsync("api/portfoliousers", newUser);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdatePortfolioUserAsync(PortfolioUser user)
    {
        await AttachTokenAsync();
        var response = await _http.PutAsJsonAsync($"api/portfoliousers/{user.Id}", user);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeletePortfolioUserAsync(int id)
    {
        await AttachTokenAsync();
        var response = await _http.DeleteAsync($"api/portfoliousers/{id}");
        response.EnsureSuccessStatusCode();
    }

    private async Task AttachTokenAsync()
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
        {
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}
