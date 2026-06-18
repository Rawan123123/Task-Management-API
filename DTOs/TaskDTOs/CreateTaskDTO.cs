using System.ComponentModel.DataAnnotations;
using Task_Management_Project.Enum;

namespace Task_Management_Project.DTOs.TaskDTOs
{
    public class CreateTaskDTO
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
        public Enum.TaskStatus Status { get; set; } = Enum.TaskStatus.Pending;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime? DueDate { get; set; }
        public int ProjectId { get; set; }
        public int? AssignedToUserId { get; set; }
    }
}
