using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.DTOs.UserDTOs
{
    public class UpdateProfileDTO
    {
        [Required, MaxLength(100), MinLength(3)]
        public string UserName { get; set; }

        public string? ProfileImgUrl { get; set; }

    }
}
