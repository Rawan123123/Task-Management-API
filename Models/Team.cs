using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation properties
        //public ICollection<Project> Projects { get; set; } = new List<Project>();
        //public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();

    }
}
