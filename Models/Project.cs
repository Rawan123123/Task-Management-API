using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } 


        // Navigation properties
        //project m:1 with user and team

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        //public int TeamId { get; set; }
        //public Team Team { get; set; }

        public ICollection<TaskItem> TaskItems { get; set; }


    }
}
