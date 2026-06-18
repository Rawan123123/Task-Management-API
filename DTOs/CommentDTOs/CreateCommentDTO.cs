using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.DTOs.CommentDTOs
{
    public class CreateCommentDTO
    {
        [Required , MaxLength(2000)]
        public string Content { get; set; }
        public int TaskId { get; set; }
    }
}
