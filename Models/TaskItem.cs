using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Task_Management_Project.Enum;

namespace Task_Management_Project.Models
{
    public class TaskItem
    {

        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
        public Enum.TaskStatus Status { get; set; } = Enum.TaskStatus.Pending;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } 
        public DateTime? CompletedAt { get; set; }


        public int? AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }
        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
    
}
