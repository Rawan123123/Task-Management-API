using System.ComponentModel.DataAnnotations;
using Task_Management_Project.Enum;

namespace Task_Management_Project.DTOs.TaskDTOs
{
    public class ChangeTaskStatusDTO
    {
        [Required]
        public Enum.TaskStatus Status { get; set; }
    }
}
