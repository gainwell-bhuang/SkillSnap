using Microsoft.AspNetCore.Mvc;
using SkillSnap.Shared.Models; // adjust if needed

namespace SkillSnap.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly SkillSnapContext _context;

        public SkillsController(SkillSnapContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetSkills()
        {
            return Ok(_context.Skills);
        }

        [HttpPost]
        public IActionResult AddSkill([FromBody] Skill newSkill)
        {
            _context.Skills.Add(newSkill);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetSkills), new { id = newSkill.Id }, newSkill);
        }
    }
}