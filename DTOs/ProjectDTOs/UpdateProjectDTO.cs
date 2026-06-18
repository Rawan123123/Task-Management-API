using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.DTOs.ProjectDTOs
{
    public class UpdateProjectDTO
    {
        [Required, MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
