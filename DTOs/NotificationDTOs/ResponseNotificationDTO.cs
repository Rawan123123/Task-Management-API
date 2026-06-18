using Task_Management_Project.Enum;

namespace Task_Management_Project.DTOs.NotificationDTOs
{
    public class ResponseNotificationDTO
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }
    }
}
