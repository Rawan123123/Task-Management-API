using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; }

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string? RoleName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<TaskItem> TasksAssigned { get; set; } = new List<TaskItem>();
        public ICollection<TaskItem> TasksCreated { get; set; } = new List<TaskItem>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();


    }
}
