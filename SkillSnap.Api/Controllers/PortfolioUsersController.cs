using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioUsersController : ControllerBase
    {
        private readonly SkillSnapContext _context;

        public PortfolioUsersController(SkillSnapContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioUser>>> GetPortfolioUsers()
        {
            return Ok(await _context.PortfolioUsers.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PortfolioUser>> GetPortfolioUserById(int id)
        {
            var user = await _context.PortfolioUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPortfolioUser([FromBody] PortfolioUser newUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.PortfolioUsers.Add(newUser);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPortfolioUserById), new { id = newUser.Id }, newUser);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePortfolioUser(int id, [FromBody] PortfolioUser updatedUser)
        {
            if (id != updatedUser.Id)
                return BadRequest("ID mismatch.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.PortfolioUsers.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = updatedUser.Name;
            existing.Bio = updatedUser.Bio;
            existing.ProfileImageUrl = updatedUser.ProfileImageUrl;
            // Update other properties as needed

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePortfolioUser(int id)
        {
            var user = await _context.PortfolioUsers.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.PortfolioUsers.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
