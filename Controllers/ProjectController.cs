using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System.Security.Claims;
using System.Threading.Tasks;
using Task_Management_Project.Controllers.Base;
using Task_Management_Project.DTOs.ProjectDTOs;
using Task_Management_Project.DTOs.TaskDTOs;
using Task_Management_Project.Enum;
using Task_Management_Project.Exeptions;
using Task_Management_Project.Extensions;
using Task_Management_Project.Models;
using Task_Management_Project.Models.Common;

namespace Task_Management_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : BaseController
    {
        private readonly Context _context;
        public ProjectController(Context context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreateProject(CreateProjectDTO projectFromRequest)
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            // Validate dates
            if (projectFromRequest.StartDate.HasValue &&
                projectFromRequest.EndDate.HasValue &&
                projectFromRequest.EndDate.Value < projectFromRequest.StartDate.Value)
            {
                throw new BadRequestException("End date cannot be before start date");
            }

            var newProject = new Project
            {
                Name = projectFromRequest.Name,
                Description = projectFromRequest.Description,
                StartDate = projectFromRequest.StartDate,
                EndDate = projectFromRequest.EndDate,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
            };
            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();
            return CreatedAtAction(
                            nameof(GetProjectById),
                            new { id = newProject.Id },
                            new { Message = "Project created successfully", ProjectId = newProject.Id }
                        );
        }

        [HttpGet("{id}")] //get only one of my projects
        public async Task<IActionResult> GetProjectById(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            Project project =await _context.Projects
                          .Include(p => p.CreatedByUser)
                          .Include(p => p.TaskItems)
                          .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) throw new NotFoundException($"Project with ID {id} not found.");

            // Authorization check - only creator or admin can view
            if (userRole != "Admin" && project.CreatedByUserId != userId)
            {
                throw new UnauthorizedException("You do not have permission to view this project.");
            }
            var response = new ResponseProjectDTO
            {
                Id = id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt ?? project.CreatedAt,
                CreatedByUserId = project.CreatedByUserId,
                CreatedByUsername = project.CreatedByUser?.Username,
                TotalTasks = project.TaskItems.Count,
                CompletedTasks = project.TaskItems.Count(t => t.Status == Enum.TaskStatus.Completed)
            };
            return Ok(response);
        }

        [HttpGet("my-projects")] //get all my projects
        public async Task<IActionResult> GetMyProjects([FromQuery] PaginationParams paginationParams)
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            var query = _context.Projects
                .Include(p => p.CreatedByUser)
                .Include(p => p.TaskItems)
                .Where(p => p.CreatedByUserId == userId)
                .OrderByDescending(p => p.CreatedAt);

            var pagedQuery = query.Select(project => new ResponseProjectDTO
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt ?? project.CreatedAt,
                CreatedByUserId = project.CreatedByUserId,
                CreatedByUsername = project.CreatedByUser != null ? project.CreatedByUser.Username : null,
                TotalTasks = project.TaskItems.Count,
                CompletedTasks = project.TaskItems.Count(t => t.Status == Enum.TaskStatus.Completed)
            });
            var pagedResult = await pagedQuery.ToPagedResultAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize
            );

            return Ok(pagedResult);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProjects(
            [FromQuery] PaginationParams paginationParams,
            [FromQuery] string? search = null)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var query = _context.Projects
                .Include(p => p.TaskItems)
                .Include(p => p.CreatedByUser)
                .AsQueryable();

            // Filter based on role
            // Admin can see all projects
            // Others can see only their projects
            if (userRole != "Admin")
                query = query.Where(p => p.CreatedByUserId == userId);

            // Search by name or description
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p =>
                        p.Name.Contains(search) ||
                        (p.Description != null && p.Description.Contains(search))
                );
            }
            //sort by creationDate
            query = query.OrderByDescending(q => q.CreatedAt);

            var pagedQuery = query.Select(p => new ResponseProjectDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt ?? p.CreatedAt,
                CreatedByUserId = p.CreatedByUserId,
                CreatedByUsername = p.CreatedByUser != null ? p.CreatedByUser.Username : null,
                TotalTasks = p.TaskItems.Count,
                CompletedTasks = p.TaskItems.Count(t => t.Status == Enum.TaskStatus.Completed)
            });

            var pagedResult = await pagedQuery.ToPagedResultAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize
                );
            return Ok(pagedResult);

        }


        [HttpGet("{id}/tasks")] //get all tasks for a project
        public async Task<IActionResult> GetProjectTasks(
            int id,
            [FromQuery] PaginationParams paginationParams,
            [FromQuery] Enum.TaskStatus? status = null,
            [FromQuery] TaskPriority? priority = null)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var project = await _context.Projects.FindAsync(id);
            if (project == null) throw new NotFoundException($"Project with ID {id} not found");

            // Authorization check
            if (userRole != "Admin" && project.CreatedByUserId != userId)
            {
                throw new UnauthorizedException("You are not authorized to view this project's tasks");
            }

            //tasks of specific project
            var quey = _context.TaskItems
                 .Include(t => t.AssignedToUser)
                 .Where(t => t.ProjectId == id)
                 .AsQueryable();

            //filter by status
            if (status.HasValue)
            {
                quey = quey.Where(t => t.Status == status.Value);
            }
            //filter by priority
            if (priority.HasValue)
            {
                quey = quey.Where(t => t.Priority == priority.Value);
            }

            quey = quey.OrderBy(t => t.DueDate ?? DateTime.MaxValue);

            var pagedQuery = quey.Select(t => new ResponseTaskDTO
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
                RelatedProjectId = t.ProjectId,
                AssignedToUserId = t.AssignedToUserId,
                AssignedToUsername = t.AssignedToUser != null ? t.AssignedToUser.Username : null
            });
            var pagedresult = await pagedQuery.ToPagedResultAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize
            );
            return Ok(pagedresult);
        }
        

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDTO projectFromRequest)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null) throw new NotFoundException($"Project with Id {id} Not Found");

            if (userRole != "Admin" && project.CreatedByUserId != userId)
                throw new UnauthorizedException("You are not authorized to update this project");

            if (projectFromRequest.StartDate.HasValue &&
               projectFromRequest.EndDate.HasValue &&
               projectFromRequest.EndDate.Value < projectFromRequest.StartDate.Value
               )
            {
                throw new BadRequestException("End date cannot be before start date");
            }
            project.Name = projectFromRequest.Name;
            project.Description = projectFromRequest.Description;
            project.StartDate = projectFromRequest.StartDate;
            project.EndDate = projectFromRequest.EndDate;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Project updated successfully!" });

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DelteProject(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var project = await _context.Projects
                .Include(p => p.TaskItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) throw new NotFoundException($"Project with Id {id} Not Found");

            if (userRole != "Admin" && project.CreatedByUserId != userId)
                throw new UnauthorizedException("You are not authorized to delete this project");

            // Check if project has tasks
            if (project.TaskItems.Any())
            {
                return BadRequest("Cannot delete project with existing tasks. Please delete or reassign tasks first.");
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Project deleted successfully!" });

        }

        [HttpGet("{id}/statistics")]
        public async Task<IActionResult> GetProjectStatistics(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var project = await _context.Projects
                .Include(p => p.TaskItems)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) throw new NotFoundException($"Project with Id {id} Not Found");

            if (userRole != "Admin" && project.CreatedByUserId != userId)
            {
                throw new UnauthorizedException("You are not authorized to view this project's statistics");
            }

            var now = DateTime.UtcNow;
            var totalTasks = project.TaskItems.Count();
            var completedTasks = project.TaskItems.Count(t => t.Status == Enum.TaskStatus.Completed);

            var status = new ProjectStatisticsDTO
            {
                Id = id,
                Name = project.Name,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = project.TaskItems.Count(t => t.Status == Enum.TaskStatus.Pending),
                InProgressTasks = project.TaskItems.Count(t => t.Status == Enum.TaskStatus.InProgress),
                OnHoldTasks = project.TaskItems.Count(t => t.Status == Enum.TaskStatus.OnHold),
                CancelledTasks = project.TaskItems.Count(t => t.Status == Enum.TaskStatus.Cancelled),
                OverdueTasks = project.TaskItems.Count(t =>
                    t.DueDate.HasValue &&
                    t.DueDate < now &&
                    t.Status != Enum.TaskStatus.Completed
                ),
                CompletionPercentage = totalTasks > 0 ? Math.Round((double)completedTasks / totalTasks * 100, 2) : 0,
            };
            return Ok(status);
        }

    }
}

