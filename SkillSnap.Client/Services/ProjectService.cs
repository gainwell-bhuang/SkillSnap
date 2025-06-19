using System.Net.Http.Json;
using SkillSnap.Shared.Models; // adjust namespace if needed
public class ProjectService
{
    private readonly HttpClient _http;
    public ProjectService(HttpClient http)
    {
        _http = http;
    }
    public async Task<List<Project>> GetProjectsAsync()
    {
        return await _http.GetFromJsonAsync<List<Project>>("api/projects");
    }
    public async Task AddProjectAsync(Project newProject)
    {
        var response = await _http.PostAsJsonAsync("api/projects", newProject);
        response.EnsureSuccessStatusCode();
    }
}
