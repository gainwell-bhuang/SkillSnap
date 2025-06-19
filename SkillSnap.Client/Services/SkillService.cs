using System.Net.Http.Json;
using SkillSnap.Shared.Models; // adjust namespace if needed
public class SkillService
{
    private readonly HttpClient _http;
    public SkillService(HttpClient http)
    {
        _http = http;
    }
    public async Task<List<Skill>> GetSkillsAsync()
    {
        return await _http.GetFromJsonAsync<List<Skill>>("api/skills");
    }
    public async Task AddSkillAsync(Skill newSkill)
    {
        var response = await _http.PostAsJsonAsync("api/skills", newSkill);
        response.EnsureSuccessStatusCode();
    }
}
