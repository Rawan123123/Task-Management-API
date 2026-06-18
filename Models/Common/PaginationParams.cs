using System.ComponentModel.DataAnnotations;

namespace Task_Management_Project.Models.Common
{
    public class PaginationParams
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

    }
}
