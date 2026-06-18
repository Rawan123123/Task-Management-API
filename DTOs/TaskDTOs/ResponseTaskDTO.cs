    using Task_Management_Project.Enum;

namespace Task_Management_Project.DTOs.TaskDTOs
{
    public class ResponseTaskDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public Enum.TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? AssignedToUsername { get; set; }
        public int? RelatedProjectId { get; set; }

    }
}
