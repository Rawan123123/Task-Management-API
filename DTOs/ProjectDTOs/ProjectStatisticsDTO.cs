namespace Task_Management_Project.DTOs.ProjectDTOs
{
    public class ProjectStatisticsDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int CancelledTasks { get; set; }
        public int OnHoldTasks { get; set; }
        public int OverdueTasks { get; set; }
        public double CompletionPercentage { get; set; } = 0;

    }
}
