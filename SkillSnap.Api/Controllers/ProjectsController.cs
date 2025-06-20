using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SkillSnap.Shared.Models;
using System.Diagnostics;

namespace SkillSnap.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProjectsController> _logger;
        private static readonly string ProjectListCacheKey = "Projects";
        private const int DefaultPageSize = 20;

        public ProjectsController(SkillSnapContext context, IMemoryCache cache, ILogger<ProjectsController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        // GET: api/projects?page=1&pageSize=10
        [HttpGet]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects(int page = 1, int pageSize = DefaultPageSize)
        {
            var sw = Stopwatch.StartNew();
            string cacheKey = $"{ProjectListCacheKey}_Page{page}_Size{pageSize}";

            if (!_cache.TryGetValue(cacheKey, out List<ProjectDto> projects))
            {
                _logger.LogInformation("Cache miss for {CacheKey}", cacheKey);
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
            else
            {
                _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
            }

            sw.Stop();
            _logger.LogInformation("GetProjects request duration: {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);

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
