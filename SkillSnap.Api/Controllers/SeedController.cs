using Microsoft.AspNetCore.Mvc;
using SkillSnap.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace SkillSnap.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase {
        private readonly SkillSnapContext _context;
        public SeedController(SkillSnapContext context) {
            _context = context;
        }
        [HttpPost]
        public IActionResult SeedData() {
            if (_context.PortfolioUsers.Any())
                return Ok("Data already exists.");
                
            var user = new PortfolioUser {
                Name = "John Doe",
                Bio = "Software developer with 10 years experience.",
                ProfileImageUrl = "https://example.com/profile.jpg",
                Projects = new List<Project> {
                    new Project { Title = "Project One", Description = "First project", ImageUrl = "https://example.com/proj1.jpg" }
                },
                Skills = new List<Skill> {
                    new Skill { Name = "C#", Level = "Expert" }
                }
            };

            _context.PortfolioUsers.Add(user);
            _context.SaveChanges();
            return Ok("Sample data inserted.");
        }
    }
}