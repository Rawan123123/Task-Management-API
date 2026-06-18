using System.ComponentModel.DataAnnotations;
using Task_Management_Project.Enum;

namespace Task_Management_Project.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public int UserId { get; set; }
        public User User { get; set; }

    }
}
