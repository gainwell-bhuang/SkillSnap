using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioUsersController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PortfolioUsersController> _logger;
        private static readonly string PortfolioUserCacheKey = "PortfolioUsers";

        public PortfolioUsersController(SkillSnapContext context, IMemoryCache cache, ILogger<PortfolioUsersController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PortfolioUserDto>>> GetPortfolioUsers()
        {
            if (!_cache.TryGetValue(PortfolioUserCacheKey, out List<PortfolioUserDto> users))
            {
                _logger.LogInformation("Cache miss for {CacheKey}", PortfolioUserCacheKey);
                users = await _context.PortfolioUsers
                    .AsNoTracking()
                    .Select(u => new PortfolioUserDto
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Bio = u.Bio,
                        ProfileImageUrl = u.ProfileImageUrl
                    })
                    .ToListAsync();

                _cache.Set(PortfolioUserCacheKey, users, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                });
            }
            else
            {
                _logger.LogInformation("Cache hit for {CacheKey}", PortfolioUserCacheKey);
            }
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PortfolioUserDto>> GetPortfolioUserById(int id)
        {
            var user = await _context.PortfolioUsers
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new PortfolioUserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Bio = u.Bio,
                    ProfileImageUrl = u.ProfileImageUrl
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound();
            return Ok(user);
        }

        private void ClearPortfolioUserCache()
        {
            if (_cache is MemoryCache memoryCache)
            {
                var field = typeof(MemoryCache).GetField("_entries", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var entries = field.GetValue(memoryCache) as IDictionary<object, object>;
                    if (entries != null)
                    {
                        var keysToRemove = entries.Keys
                            .Where(k => k is string s && s.StartsWith(PortfolioUserCacheKey))
                            .ToList();
                        foreach (var key in keysToRemove)
                        {
                            _cache.Remove(key);
                        }
                    }
                }
            }
            _cache.Remove(PortfolioUserCacheKey);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddPortfolioUser([FromBody] PortfolioUser newUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.PortfolioUsers.Add(newUser);
            await _context.SaveChangesAsync();

            ClearPortfolioUserCache(); // cache cleared after create

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

            ClearPortfolioUserCache(); // cache cleared after update

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

            ClearPortfolioUserCache(); // cache cleared after delete

            return NoContent();
        }
    }
}
