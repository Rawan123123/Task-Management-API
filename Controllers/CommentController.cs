using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task_Management_Project.Controllers.Base;
using Task_Management_Project.DTOs.CommentDTOs;
using Task_Management_Project.Exeptions;
using Task_Management_Project.Extensions;
using Task_Management_Project.Models;
using Task_Management_Project.Models.Common;
using Task_Management_Project.Services;

namespace Task_Management_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentController : BaseController
    {
        private readonly Context _context;
        private readonly INotificationService _notificationService;
        private readonly ITaskHubService _hubService;

        public CommentController(Context context , INotificationService notificationService , ITaskHubService hubService)
        {
            _context = context;
            _notificationService = notificationService;
            _hubService = hubService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateComment(CreateCommentDTO CommentFromRequest)
        {
            ValidateModel();
            int UserId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == CommentFromRequest.TaskId);

            if (task == null) throw new NotFoundException($"Task with Id {CommentFromRequest.TaskId} is not found");
            // Only the assigned user,
            // project creator,
            // or admin can comment on the task
            bool isAuthorized = task.AssignedToUserId == UserId ||
                                task.Project.CreatedByUserId == UserId ||
                                userRole == "Admin";

            if(!isAuthorized)
            {
                throw new UnauthorizedException("You are not authorized to comment on this task");
            }

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);

            var newComment = new Comment
            {
                Content = CommentFromRequest.Content,
                UserId = UserId,
                TaskId = CommentFromRequest.TaskId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            await _hubService.SendNewComment(newComment.TaskId, new
            {
                Id = newComment.Id,
                Content = newComment.Content,
                UserId = newComment.UserId,
                UserName = currentUser.Username,
                CreatedAt = newComment.CreatedAt
            });


            var notifiedUsers = new HashSet<int>();

            //send notification to the assined user if the commenter is not the assigned user(project creator or admin)
            if (newComment.UserId != task.AssignedToUserId && task.AssignedToUserId.HasValue)
            {
                await _notificationService.NotifyCommentAdded(
                                task.Id,
                                task.AssignedToUserId.Value,
                                currentUser.Username ?? "Someone",
                                task.Title
                );
                notifiedUsers.Add(task.AssignedToUserId.Value); 
            }
            //send notification to the project creator if the commenter is not the project creator or not assigned user (admin only)

            if (task.Project.CreatedByUserId != UserId && !notifiedUsers.Contains(task.Project.CreatedByUserId))
            {
                await _notificationService.NotifyCommentAdded(
                                    task.Id,
                                    task.Project.CreatedByUserId,
                                    currentUser?.Username ?? "Someone",
                                    task.Title
                );
            }

            return CreatedAtAction(
                nameof(GetCommentById),
                new { id = newComment.Id },
                new { Message = "Comment created successfully!", CommentId = newComment.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Task)
                    .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) throw new NotFoundException($"Comment with Id {id} is not found");

            bool isAuthorized = comment.UserId == userId ||
                                comment.Task.Project.CreatedByUserId == userId ||
                                userRole == "Admin";
            if (!isAuthorized)
            {
                throw new UnauthorizedException("You are not authorized to view this comment");
            }

            return Ok(new ResponseCommentDTO
            {
                Id = id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UserId = comment.UserId,
                UserName = comment.User?.Username,
                TaskId = comment.TaskId,
                TaskName = comment.Task?.Title,
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetMyComments([FromQuery] PaginationParams paginationParams)
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            var query = _context.Comments
                 .Include(c => c.User)
                 .Include(c => c.Task)
                 .Where(c => c.UserId == userId)
                 .AsQueryable();

            var pagedQuery = query.Select(c => new ResponseCommentDTO
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                UserId = c.UserId,
                UserName = c.User != null ? c.User.Username : null,
                ProfileImageUrl = c.User != null ? c.User.ProfileImageUrl : null,
                TaskId = c.TaskId,
                TaskName = c.Task != null ? c.Task.Title : null,
            });

            var pagedResult = await pagedQuery.ToPagedResultAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize);

            return Ok(pagedResult);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, UpdateCommentDTO commentFromRequest)
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) throw new NotFoundException($"Comment with Id {id} is not found");

            if (comment.UserId != userId )
            {
                throw new UnauthorizedException("You are not authorized to update this comment");
            }

            comment.Content = commentFromRequest.Content ?? comment.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Comment updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var comment = await _context.Comments
                .Include(c => c.Task)
                    .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) throw new NotFoundException($"Comment with Id {id} is not found");

            bool isAuthorized = comment.UserId == userId ||
                                comment.Task.Project?.CreatedByUserId == userId ||
                                userRole == "Admin";

            if (!isAuthorized)
                throw new UnauthorizedException("You are not authorized to delete this comment");


            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Comment deleted successfully!" });

        }
    }
}
