using Task_Management_Project.Enum;
using Task_Management_Project.Models;

namespace Task_Management_Project.Services
{
    public interface INotificationService
    {
        Task CreateNotification(int userId, string message, NotificationType type);
        Task NotifyTaskAssigned(int taskId, int assignedUserId, string taskTitle);
        Task NotifyTaskCompleted(int taskId, int projectOwnerId, string taskTitle);
        Task NotifyCommentAdded(int taskId, int taskOwnerId, string commenterName, string taskTitle);
        Task NotifyTaskStatusChanged(int taskId, int taskCreatorId, string taskTitle, Enum.TaskStatus newStatus, string changedByUsername);
        Task NotifyProjectUpdated(int projectId, List<int> teamMemberIds, string projectName);
        Task NotifyDeadlineApproaching(int taskId, int assignedUserId, string taskTitle, DateTime dueDate);
    }
    public class NotificationServicecs : INotificationService
    {
        private readonly Context _context;
        private readonly ITaskHubService _hubService;

        public NotificationServicecs(Context context , ITaskHubService hubService)
        {
            _context = context;
            _hubService = hubService;
        }

        public async Task CreateNotification(int userId, string message, NotificationType type)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            //send real time to client
            await _hubService.SendNotificationToUser(userId , message , type.ToString());

            //send number of unread notification
            int unreadCount = _context.Notifications
                .Count(n => n.UserId == userId && !n.IsRead);
        }

        public async Task NotifyTaskAssigned(int taskId, int assignedUserId, string taskTitle)
        {
            string message = $"You have been assigned to the task: {taskTitle}";
            await CreateNotification(assignedUserId, message, NotificationType.TaskAssigned);
        }

        public async Task NotifyTaskCompleted(int taskId, int projectOwnerId, string taskTitle)
        {
            string message = $"The task '{taskTitle}' has been completed.";
            await CreateNotification(projectOwnerId, message, NotificationType.TaskCompleted);
        }

        public async Task NotifyTaskStatusChanged(int taskId, int taskCreatorId, string taskTitle, Enum.TaskStatus newStatus, string changedByUsername)
        {
            string statusText = newStatus switch
            {
                Enum.TaskStatus.Pending => "Pending",
                Enum.TaskStatus.InProgress => "In Progress",
                Enum.TaskStatus.Completed => "Completed",
                Enum.TaskStatus.OnHold => "On Hold",
                Enum.TaskStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };

            string message = $"{changedByUsername} changed the status of your task '{taskTitle}' to {statusText}";
            await CreateNotification(taskCreatorId, message, NotificationType.TaskStatusChanged);

            //send real time update to all users who open this task
            await _hubService.SendTaskStatusUpdate(taskId , statusText, changedByUsername);
        }

        public async Task NotifyCommentAdded(int taskId, int taskOwnerId, string commenterName, string taskTitle)
        {
            string message = $"{commenterName} commented on your task: {taskTitle}";
            await CreateNotification(taskOwnerId, message, NotificationType.CommentAdded);
        }

        public async Task NotifyProjectUpdated(int projectId, List<int> teamMemberIds, string projectName)
        {
            string message = $"The project '{projectName}' has been updated.";
            foreach (var userId in teamMemberIds)
            {
                await CreateNotification(userId, message, NotificationType.ProjectUpdated);
            }
        }

        public async Task NotifyDeadlineApproaching(int taskId, int assignedUserId, string taskTitle, DateTime dueDate)
        {
            string message = $"The deadline for the task '{taskTitle}' is approaching on {dueDate.ToShortDateString()}.";
            await CreateNotification(assignedUserId, message, NotificationType.DeadlineApproaching);
        }
    }
}
