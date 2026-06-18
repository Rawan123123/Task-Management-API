namespace Task_Management_Project.Services
{
    public interface ITaskHubService
    {
        Task SendNotificationToUser(int userId , string message , string type);
        Task SendTaskStatusUpdate(int taskId, string newStatus, string changedBy);
        Task SendNewComment(int taskId , object commentData);
    }
}
