using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SkillSnap.Shared.Models;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillsController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SkillsController> _logger;
        private const string SkillsCacheKey = "Skills";

        public SkillsController(SkillSnapContext context, IMemoryCache cache, ILogger<SkillsController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        // GET: api/skills
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkills()
        {
            var sw = Stopwatch.StartNew();
            if (!_cache.TryGetValue(SkillsCacheKey, out List<SkillDto> cachedSkills))
            {
                _logger.LogInformation("Cache miss for {CacheKey}", SkillsCacheKey);
                cachedSkills = await _context.Skills
                    .AsNoTracking()
                    .OrderBy(s => s.Name)
                    .Select(s => new SkillDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Level = s.Level
                    })
                    .ToListAsync();

                _cache.Set(SkillsCacheKey, cachedSkills, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                });
            }
            else
            {
                _logger.LogInformation("Cache hit for {CacheKey}", SkillsCacheKey);
            }

            sw.Stop();
            _logger.LogInformation("GetSkills request duration: {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);

            return Ok(cachedSkills);
        }

        // GET: api/skills/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<SkillDto>> GetSkillById(int id)
        {
            // Project as early as possible and use FirstOrDefaultAsync for efficiency
            var skill = await _context.Skills
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level
                })
                .FirstOrDefaultAsync();

            if (skill == null)
                return NotFound();

            return Ok(skill);
        }

        // Utility method to clear all skill cache entries
        private void ClearSkillsCache()
        {
            // Remove all cache entries that start with the SkillsCacheKey
            if (_cache is MemoryCache memoryCache)
            {
                var field = typeof(MemoryCache).GetField("_entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var entries = field.GetValue(memoryCache) as IDictionary<object, object>;
                    if (entries != null)
                    {
                        var keysToRemove = entries.Keys
                            .Where(k => k is string s && s.StartsWith(SkillsCacheKey))
                            .ToList();
                        foreach (var key in keysToRemove)
                        {
                            _cache.Remove(key);
                        }
                    }
                }
            }
            _cache.Remove(SkillsCacheKey);
        }

        // POST: api/skills
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddSkill([FromBody] Skill newSkill)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Skills.Add(newSkill);
            await _context.SaveChangesAsync();

            ClearSkillsCache();

            return CreatedAtAction(nameof(GetSkillById), new { id = newSkill.Id }, newSkill);
        }

        // PUT: api/skills/5
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSkill(int id, [FromBody] Skill updatedSkill)
        {
            if (id != updatedSkill.Id)
                return BadRequest("ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.Skills.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = updatedSkill.Name;
            existing.Level = updatedSkill.Level;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            ClearSkillsCache();

            return NoContent();
        }

        // DELETE: api/skills/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSkill(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
                return NotFound();

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();

            ClearSkillsCache();

            return NoContent();
        }
    }
}
