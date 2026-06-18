using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public int TaskId { get; set; }
        public TaskItem Task { get; set; }
    }
}
