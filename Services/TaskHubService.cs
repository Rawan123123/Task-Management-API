using Microsoft.AspNetCore.SignalR;
using Task_Management_Project.Hubs;

namespace Task_Management_Project.Services
{
    public class TaskHubService : ITaskHubService
    {
        private readonly IHubContext<TaskHub> _hubContext;
        public TaskHubService(IHubContext<TaskHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendNewComment(int taskId, object commentData)
        {
            await _hubContext.Clients
                .Group($"task_{taskId}")
                .SendAsync("NewCommentAdded" , commentData);
            
        }

        public async Task SendNotificationToUser(int userId, string message, string type)
        {
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    message = message,
                    type = type,
                    CreatedAt = DateTime.UtcNow

                });
        }

        public async Task SendTaskStatusUpdate(int taskId, string newStatus, string changedBy)
        {
            await _hubContext.Clients
                .Groups($"task_{taskId}")
                .SendAsync("TaskStatusUpdated", new
                {
                    taskId = taskId,
                    newStatus = newStatus,
                    ChangedBy = changedBy,
                    UpdatedAt = DateTime.UtcNow
                });
        }
    }
}
