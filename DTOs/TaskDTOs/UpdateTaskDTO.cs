using System.ComponentModel.DataAnnotations;
using Task_Management_Project.Enum;

namespace Task_Management_Project.DTOs.TaskDTOs
{
    public class UpdateTaskDTO
    {
        [Required, MaxLength(200)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
