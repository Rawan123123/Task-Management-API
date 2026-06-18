using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.DTOs.CommentDTOs
{
    public class UpdateCommentDTO
    {
        [Required , MaxLength(2000)]
        public string? Content { get; set; }
    }
}
