using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.DTOs.AuthDTOs
{
    public class RegisterDTO
    {
        [Required, MaxLength(100) , MinLength(3)]
        public string UserName { get; set; }

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; }

        [Required,MinLength(5)]
        public string Password { get; set; }
    }
}
