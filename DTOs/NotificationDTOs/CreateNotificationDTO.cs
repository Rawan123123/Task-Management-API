using System.ComponentModel.DataAnnotations;
using Task_Management_Project.Enum;

namespace Task_Management_Project.DTOs.NotificationDTOs
{
    public class CreateNotificationDTO
    {
        [Required , StringLength(500)]
        public string Message { get; set; }
        public NotificationType Type { get; set; }

        public int UserId { get; set; }
    }
}
