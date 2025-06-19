using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SkillSnap.Shared.Models;

public class PortfolioUser {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string Bio { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
        
        // Navigation properties
        public List<Project> Projects { get; set; } = default!;
        public List<Skill> Skills { get; set; } = default!;
    } 