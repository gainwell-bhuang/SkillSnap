using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSnap.Shared.Models;

namespace SkillSnap.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        public ProjectsController(SkillSnapContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetProjects()
        {
            return Ok(_context.Projects);
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