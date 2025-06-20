using System.Net.Http.Json;
using SkillSnap.Shared.Models;
using System.Net.Http;
using Microsoft.JSInterop;

public class SkillService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public SkillService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<List<Skill>> GetSkillsAsync()
    {
        return await _http.GetFromJsonAsync<List<Skill>>("api/skills") ?? new List<Skill>();
    }

    public async Task<Skill?> GetSkillByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Skill>($"api/skills/{id}");
    }

    public async Task AddSkillAsync(Skill newSkill)
    {
        await AttachTokenAsync();
        var response = await _http.PostAsJsonAsync("api/skills", newSkill);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateSkillAsync(Skill skill)
    {
        await AttachTokenAsync();
        var response = await _http.PutAsJsonAsync($"api/skills/{skill.Id}", skill);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteSkillAsync(int id)
    {
        await AttachTokenAsync();
        var response = await _http.DeleteAsync($"api/skills/{id}");
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
