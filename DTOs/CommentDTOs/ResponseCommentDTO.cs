namespace Task_Management_Project.DTOs.CommentDTOs
{
    public class ResponseCommentDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; } = null;
        public string? ProfileImageUrl { get; set; }

        public int TaskId { get; set; }
        public string? TaskName { get; set; } = null;
    }
}
