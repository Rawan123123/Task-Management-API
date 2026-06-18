using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Task_Management_Project.Hubs
{
    [Authorize]
    public class TaskHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = Context.User?.FindFirstValue("sub");

                // log كل الـ claims الموجودة
                var claims = Context.User?.Claims.Select(c => $"{c.Type}: {c.Value}");
                Console.WriteLine("=== SignalR Connection ===");
                Console.WriteLine($"UserId from sub: {userId}");
                Console.WriteLine($"All Claims: {string.Join(", ", claims ?? new List<string>())}");
                Console.WriteLine($"IsAuthenticated: {Context.User?.Identity?.IsAuthenticated}");

                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
                    Console.WriteLine($"Added to group: user_{userId}");
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue("sub");
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinTaskRoom(int taskId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"task_{taskId}");
        }

        public async Task LeaveTaskRoom(int taskId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"task_{taskId}");
        }
    }
}