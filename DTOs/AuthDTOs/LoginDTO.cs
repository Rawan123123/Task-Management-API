using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.DTOs.AuthDTOs
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
