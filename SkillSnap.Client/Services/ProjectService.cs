using System.Net.Http.Json;
using SkillSnap.Shared.Models;
using System.Net.Http;
using Microsoft.JSInterop;

/// <summary>
/// Service for managing Project CRUD operations with the API.
/// </summary>
public class ProjectService // : IProjectService (recommended for abstraction)
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private List<Project>? _lastProjectsCache;

    public ProjectService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    /// <summary>
    /// Get all projects from the API.
    /// </summary>
    public async Task<List<Project>> GetProjectsAsync()
    {
        try
        {
            _lastProjectsCache = await _http.GetFromJsonAsync<List<Project>>("api/projects").ConfigureAwait(false) ?? new List<Project>();
            return _lastProjectsCache;
        }
        catch (Exception ex)
        {
            // TODO: Add logging here if desired
            return _lastProjectsCache ?? new List<Project>();
        }
    }

    /// <summary>
    /// Get a single project by its ID.
    /// </summary>
    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<Project>($"api/projects/{id}").ConfigureAwait(false);
        }
        catch
        {
            // TODO: Add logging here if desired
            return null;
        }
    }

    /// <summary>
    /// Add a new project (requires authentication).
    /// </summary>
    public async Task AddProjectAsync(Project newProject)
    {
        await AttachTokenAsync().ConfigureAwait(false);
        var response = await _http.PostAsJsonAsync("api/projects", newProject).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        _lastProjectsCache = null;
    }

    /// <summary>
    /// Update an existing project (requires authentication).
    /// </summary>
    public async Task UpdateProjectAsync(Project project)
    {
        await AttachTokenAsync().ConfigureAwait(false);
        var response = await _http.PutAsJsonAsync($"api/projects/{project.Id}", project).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        _lastProjectsCache = null;
    }

    /// <summary>
    /// Delete a project by ID (requires Admin role).
    /// </summary>
    public async Task DeleteProjectAsync(int id)
    {
        await AttachTokenAsync().ConfigureAwait(false);
        var response = await _http.DeleteAsync($"api/projects/{id}").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        _lastProjectsCache = null;
    }

    /// <summary>
    /// Optionally clear the local cache.
    /// </summary>
    public void ClearCache() => _lastProjectsCache = null;

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
