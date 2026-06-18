namespace Task_Management_Project.DTOs.UserDTOs
{
    public class UserResponseDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? RoleName { get; set; }
        public string? ProfileImgURL { get; set; }
        public bool IsActive { get; set; }
    }
}
