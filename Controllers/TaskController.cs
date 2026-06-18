using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Task_Management_Project.Controllers.Base;
using Task_Management_Project.DTOs.CommentDTOs;
using Task_Management_Project.DTOs.TaskDTOs;
using Task_Management_Project.Enum;
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
    public class TaskController : BaseController
    {
        private readonly Context _context;
        private readonly INotificationService _notificationService;
        public TaskController(Context context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskDTO TaskFromRequest)
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            // check if project exists
            if (TaskFromRequest.ProjectId != null)
            {
                var projectExists = await _context.Projects
                    .AnyAsync(p => p.Id == TaskFromRequest.ProjectId);

                if (!projectExists)
                {
                    throw new NotFoundException($"Project with ID {TaskFromRequest.ProjectId} not found");
                }
            }
            // check if User exists
            if (TaskFromRequest.AssignedToUserId != null)
            {
                var userExists = await _context.Users
                    .AnyAsync(u => u.Id == TaskFromRequest.AssignedToUserId);

                if (!userExists)
                {
                    throw new NotFoundException($"User with ID {TaskFromRequest.AssignedToUserId} not found");
                }
            }

            TaskItem newTask = new TaskItem
            {
                Title = TaskFromRequest.Title,
                Description = TaskFromRequest.Description,
                Status = TaskFromRequest.Status,
                CreatedByUserId = userId,
                AssignedToUserId = TaskFromRequest.AssignedToUserId,
                ProjectId = TaskFromRequest.ProjectId,
                Priority = TaskFromRequest.Priority,
                DueDate = TaskFromRequest.DueDate,
                CreatedAt = DateTime.UtcNow,
            };

            _context.TaskItems.Add(newTask);
            await _context.SaveChangesAsync();

            //send notification to assigned user if task is assigned to someone else
            if (TaskFromRequest.AssignedToUserId.HasValue && TaskFromRequest.AssignedToUserId.Value != userId)
            {
                await _notificationService.NotifyTaskAssigned(
                    newTask.Id,
                    TaskFromRequest.AssignedToUserId.Value,
                    newTask.Title
                    );
            }
            return CreatedAtAction(
                            nameof(GetTasksById),
                            new { id = newTask.Id },
                            new { Message = "Task created successfully", TaskId = newTask.Id }
                        );
        }

        //get my tasks
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTasksById(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            TaskItem task = await _context.TaskItems
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) throw new NotFoundException($"Task with Id {id} not found.");
            if (task.CreatedByUserId != userId 
                && task.AssignedToUserId != userId
                && userRole != "Admin")
            {
                throw new UnauthorizedException("You are not authorized to view this task.");
            }

            return Ok(new ResponseTaskDTO
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                CreatedByUserId = task.CreatedByUserId,
                CreatedByUserName = task.CreatedByUser?.Username,
                AssignedToUserId = task.AssignedToUserId,
                AssignedToUsername = task.AssignedToUser?.Username,
                RelatedProjectId = task.ProjectId,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                CompletedAt = task.CompletedAt,
            });


        }

        //GET /api/Task?PageNumber=1&PageSize=10&status=0&priority=3&search=buy
        [HttpGet]
        public async Task<IActionResult> GetMyTasks(
            [FromQuery] PaginationParams paginationParams,
            [FromQuery] Enum.TaskStatus? status = null,
            [FromQuery] TaskPriority? priority = null,
            [FromQuery] string? search = null,
            [FromQuery] bool? isOverdue = null)
        {
            int currentUserId = GetCurrentUserId();

            var query = _context.TaskItems
                .Where(t => t.AssignedToUserId == currentUserId)
                .AsQueryable();

            // Filter by Status
            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            // Filter by Priority
            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            // Search by Title or Description
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t =>
                    t.Title.Contains(search) ||
                    (t.Description != null && t.Description.Contains(search))
                );
            }

            // Filter Overdue Tasks
            if (isOverdue.HasValue && isOverdue.Value)
            {
                var now = DateTime.UtcNow;
                query = query.Where(t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value < now &&
                    t.Status != Enum.TaskStatus.Completed
                );
            }

            // Sort by DueDate (nearest first)
            query = query.OrderBy(t => t.DueDate ?? DateTime.MaxValue);

            IQueryable<ResponseTaskDTO> pagedQuery = query.Select(t => new ResponseTaskDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CompletedAt = t.CompletedAt,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUserName = t.CreatedByUser != null ? t.CreatedByUser.Username : null,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUsername = t.AssignedToUser != null ? t.AssignedToUser.Username : null,
                RelatedProjectId = t.ProjectId
            });

            //extension method for pagination
            PagedResult<ResponseTaskDTO> pagedResult = await pagedQuery.ToPagedResultAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize
            );

            return Ok(pagedResult);
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetTaskComments(PaginationParams paginationParams, int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) throw new NotFoundException($"Task with Id {id} not found.");

            bool isAuthorized = task.AssignedToUserId == userId ||
                                task.CreatedByUserId == userId ||
                               task.Project?.CreatedByUserId == userId ||
                               userRole == "Admin";

            if (!isAuthorized)
                throw new UnauthorizedException("You are not authorized to view comments for this task");


            //fetch comments
            var query = _context.Comments
                .Include(c => c.User)
                .Include(c => c.Task)
                .Where(c => c.TaskId == id)
                .OrderBy(c => c.CreatedAt);


            var pagedQuery = query.Select(c => new ResponseCommentDTO
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                UserId = c.UserId,
                UserName = c.User != null ? c.User.Username : null,
                TaskId = c.TaskId,
                TaskName = c.Task != null ? c.Task.Title : null,
            });
            var pagedResult = await pagedQuery.ToPagedResultAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize
            );

            return Ok(pagedResult);
        }

        [HttpGet("{id}/comments/count")]
        public async Task<IActionResult> GetTaskCommentsCount(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) throw new NotFoundException($"Task with Id {id} not found.");

            bool isAuthorized = task.AssignedToUserId == userId ||
                                 task.CreatedByUserId == userId ||
                               task.Project?.CreatedByUserId == userId ||
                               userRole == "Admin";

            if (!isAuthorized)
                throw new UnauthorizedException("You are not authorized to view comments for this task");

            int commentsCount = await _context.Comments
                .Where(c => c.TaskId == id)
                .CountAsync();
            
            return Ok(new { TaskId = id, CommentsCount = commentsCount });
        }


        //update my task
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDTO taskFromRequest)
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            TaskItem task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) throw new NotFoundException($"Task with Id {id} not found.");

            if (task.AssignedToUserId != userId &&
                task.CreatedByUserId != userId) 
                throw new UnauthorizedException("You are not authorized to update this task.");

            task.Title = taskFromRequest.Title;
            task.Description = taskFromRequest.Description;
            task.Priority = taskFromRequest.Priority;
            task.DueDate = taskFromRequest.DueDate;
            task.UpdatedAt = DateTime.UtcNow;


            await _context.SaveChangesAsync();

            return Ok(new { Message = "Task updated successfully" });

        }

        //change status of my task
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeTaskStatus(int id, ChangeTaskStatusDTO statusFromRequest)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);


            TaskItem task = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) throw new NotFoundException($"Task with Id {id} not found.");
            if (task.AssignedToUserId != userId && task.CreatedByUserId != userId && userRole != "Admin") throw new UnauthorizedException("You are not authorized to update this task.");

            var oldStatus = task.Status;

            task.Status = statusFromRequest.Status;
            task.UpdatedAt = DateTime.UtcNow;
             
            if(statusFromRequest.Status == Enum.TaskStatus.Completed && task.CompletedAt == null)
            {
                task.CompletedAt = DateTime.UtcNow;

                //send notification to project creator if task is completed
                if(task.Project != null && task.Project.CreatedByUserId != userId)
                {
                    await _notificationService.NotifyTaskCompleted(
                        task.Id,
                        task.Project.CreatedByUserId,
                        task.Title
                        );
                }
            }
            //send notification to task creator if status is changed by someone else
            if (task.CreatedByUserId != userId && oldStatus != statusFromRequest.Status)
            {
                var currentUser = await _context.Users.FindAsync(userId);
                await _notificationService.NotifyTaskStatusChanged(
                    task.Id,
                    task.CreatedByUserId,
                    task.Title,
                    statusFromRequest.Status,
                    currentUser?.Username ?? "Someone"
                );
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Task status updated successfully" });
        }

        //delete my task
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            TaskItem task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) throw new NotFoundException($"Task with Id {id} not found.");

            if (task.AssignedToUserId != userId && task.CreatedByUserId != userId)
                 throw new UnauthorizedException("You are not authorized to delete this task.");

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Task deleted successfully" });
        }

        //statistics for my tasks
        [HttpGet("statistics")]
        public async Task<IActionResult> GetTaskStatistics()
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var task = await _context.TaskItems
                .Where(t => t.AssignedToUserId == userId).ToListAsync();

            var now = DateTime.UtcNow;
            var stats = new
            {
                totalTasks = task.Count,
                PendingTasks = task.Count(t => t.Status == Enum.TaskStatus.Pending),
                InProgressTasks = task.Count(t => t.Status == Enum.TaskStatus.InProgress),
                CompletedTasks = task.Count(t => t.Status == Enum.TaskStatus.Completed),
                CancelledTasks = task.Count(t => t.Status == Enum.TaskStatus.Cancelled),
                OnHoldTasks = task.Count(t => t.Status == Enum.TaskStatus.OnHold),

                OverdueTasks = task.Count(t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value < now &&
                    t.Status != Enum.TaskStatus.Completed
                ),
                TasksDueToday = task.Count(t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date == now.Date
                ),
                TasksDueThisWeek = task.Count(t =>
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date >= now.Date &&
                    t.DueDate.Value.Date <= now.Date.AddDays(7)
                )

            };
            return Ok(stats);
        }
    }
}
