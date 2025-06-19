using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SkillSnap.Shared.Models; // adjust if needed

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillsController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        private readonly IMemoryCache _cache;

        public SkillsController(SkillSnapContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult GetSkills()
        {
            if (!_cache.TryGetValue("Skills", out List<Skill> cachedSkills))
            {
                cachedSkills = _context.Skills.ToList();
                _cache.Set("Skills", cachedSkills, TimeSpan.FromMinutes(5));
            }
            return Ok(cachedSkills);
        }

        [Authorize] // Require authentication for POST
        [HttpPost]
        public IActionResult AddSkill([FromBody] Skill newSkill)
        {
            _context.Skills.Add(newSkill);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetSkills), new { id = newSkill.Id }, newSkill);
        }
    }
}