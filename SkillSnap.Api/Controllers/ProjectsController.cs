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
        private static readonly string ProjectListCacheKey = "Projects";
        private const int DefaultPageSize = 20;

        public ProjectsController(SkillSnapContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/projects?page=1&pageSize=10
        [HttpGet]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects(int page = 1, int pageSize = DefaultPageSize)
        {
            string cacheKey = $"{ProjectListCacheKey}_Page{page}_Size{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<ProjectDto> projects))
            {
                // Project as early as possible for better SQL and memory efficiency
                projects = await _context.Projects
                    .AsNoTracking()
                    .OrderByDescending(p => p.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProjectDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        CreatedDate = p.CreatedDate
                    })
                    .ToListAsync();

                _cache.Set(cacheKey, projects, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                });
            }

            return Ok(projects);
        }

        // GET: api/projects/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProjectDto>> GetProjectById(int id)
        {
            // Project as early as possible and use FirstOrDefaultAsync for efficiency
            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    CreatedDate = p.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (project == null)
                return NotFound();

            return Ok(project);
        }

        // POST: api/projects
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddProject([FromBody] Project newProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            newProject.CreatedDate = DateTime.UtcNow;
            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();

            // Invalidate all cached pages (simplest form)
            _cache.Remove(ProjectListCacheKey);

            return CreatedAtAction(nameof(GetProjectById), new { id = newProject.Id }, newProject);
        }

        // PUT: api/projects/5
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] Project updatedProject)
        {
            if (id != updatedProject.Id)
                return BadRequest("ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Projects.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Title = updatedProject.Title;
            existing.Description = updatedProject.Description;
            existing.UpdatedDate = DateTime.UtcNow;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _cache.Remove(ProjectListCacheKey);
            return NoContent();
        }

        // DELETE: api/projects/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            _cache.Remove(ProjectListCacheKey);
            return NoContent();
        }
    }
}
