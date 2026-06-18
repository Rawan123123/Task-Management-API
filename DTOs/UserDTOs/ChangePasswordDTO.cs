using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.DTOs
{
    public class ChangePasswordDTO
    {
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        [MinLength(5, ErrorMessage = "Password must be at least 5 characters")]
        public string NewPassword { get; set; }
    }
}
