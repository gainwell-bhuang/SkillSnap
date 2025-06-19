using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SkillSnap.Shared.Models;

namespace SkillSnap.Api {
    public class SkillSnapContext : IdentityDbContext<ApplicationUser> {
        public SkillSnapContext(DbContextOptions<SkillSnapContext> options) : base(options) { }
        
        public DbSet<PortfolioUser> PortfolioUsers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Skill> Skills { get; set; }
    }
}