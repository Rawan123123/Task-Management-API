using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Task_Management_Project.Controllers.Base;
using Task_Management_Project.DTOs.NotificationDTOs;
using Task_Management_Project.Exeptions;
using Task_Management_Project.Extensions;
using Task_Management_Project.Models;
using Task_Management_Project.Models.Common;

namespace Task_Management_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly Context _context;
        public NotificationController(Context context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] PaginationParams paginationParams,
            [FromQuery] bool? isRead = null)
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .AsQueryable();
            // Filter by read status if provided
            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }
            //order by created date descending
            query = query.OrderByDescending(n => n.CreatedAt);

            var pagedQuery = query.Select(n => new ResponseNotificationDTO
            {
                Id = n.Id,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                UserId = n.UserId
            });
            var pagedResult = await pagedQuery.ToPagedResultAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize
                );
            return Ok(pagedResult);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotificationById(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                throw new NotFoundException($"Notification with ID {id} not found");
            }

            if (notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to access this notification");
            }
            return Ok(new ResponseNotificationDTO
            {
                Id = notification.Id,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                UserId = notification.UserId
            });
        }

        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);

            if (notification == null)
            {
                throw new NotFoundException($"Notification with ID {id} not found");
            }
            if (notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to access this notification");
            }
            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Notification marked as read" });

        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }
            return Ok(new { Message = $"{unreadNotifications.Count} notifications marked as read" });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            var unreadCount = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return Ok(new { UnreadCount = unreadCount });

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
            if (notification == null)
            {
                throw new NotFoundException($"Notification with ID {id} not found");
            }
            if (notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to access this notification");
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Notification deleted successfully" });
        }

        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            ValidateModel();
            int userId = GetCurrentUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();
            if (notifications.Any())
            {
                _context.Notifications.RemoveRange(notifications);
                await _context.SaveChangesAsync();
            }
            return Ok(new { Message = $"{notifications.Count} notifications deleted successfully" });
        }
    }
}