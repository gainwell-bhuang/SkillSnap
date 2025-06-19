using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SkillSnap.Shared.Models;

namespace SkillSnap.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        private readonly IMemoryCache _cache;

        public ProjectsController(SkillSnapContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult GetProjects()
        {
            if (!_cache.TryGetValue("Projects", out List<Project> cachedProjects))
            {
                cachedProjects = _context.Projects
                    .AsNoTracking() // Use AsNoTracking for better performance
                    .ToList();
                _cache.Set("Projects", cachedProjects, TimeSpan.FromMinutes(5));
            }
            return Ok(cachedProjects);
        }

        [Authorize] // Require authentication for POST
        [HttpPost]
        public IActionResult AddProject([FromBody] Project newProject)
        {
            _context.Projects.Add(newProject);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetProjects), new { id = newProject.Id }, newProject);
        }

        // For admin-only:
        // [Authorize(Roles = "Admin")]
        // public IActionResult DeleteProject(int id) { ... }
    }
}