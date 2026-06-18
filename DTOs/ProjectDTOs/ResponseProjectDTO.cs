namespace Task_Management_Project.DTOs.ProjectDTOs
{
    public class ResponseProjectDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public string? CreatedByUsername { get; set; }

        // Statistics
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
    }
}
