using System.Net.Http.Json;
using SkillSnap.Shared.Models;
using System.Net.Http;
using Microsoft.JSInterop;

public class ProjectService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public ProjectService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        return await _http.GetFromJsonAsync<List<Project>>("api/projects");
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<Project>($"api/projects/{id}");
    }

    public async Task AddProjectAsync(Project newProject)
    {
        await AttachTokenAsync();
        var response = await _http.PostAsJsonAsync("api/projects", newProject);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateProjectAsync(Project project)
    {
        await AttachTokenAsync();
        var response = await _http.PutAsJsonAsync($"api/projects/{project.Id}", project);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteProjectAsync(int id)
    {
        await AttachTokenAsync();
        var response = await _http.DeleteAsync($"api/projects/{id}");
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
