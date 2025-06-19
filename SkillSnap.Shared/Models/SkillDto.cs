namespace SkillSnap.Shared.Models
{
    public class SkillDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Level { get; set; } = string.Empty; // e.g., Beginner, Intermediate, Advanced
    }
}
