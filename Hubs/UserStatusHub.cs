using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace server.Hubs
{
    public class UserStatusHub : Hub
    {
        // Хранилище статусов пользователей (userId -> статус)
        private static ConcurrentDictionary<string, string> UserStatuses = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
            UserStatuses[userId] = "Online";
            await Clients.All.SendAsync("UserStatusChanged", userId, "Online");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
            UserStatuses[userId] = "Offline";
            await Clients.All.SendAsync("UserStatusChanged", userId, "Offline");
            await base.OnDisconnectedAsync(exception);
        }

        public Task SetStatus(string status)
        {
            var userId = Context.User?.Identity?.Name ?? Context.ConnectionId;
            UserStatuses[userId] = status;
            return Clients.All.SendAsync("UserStatusChanged", userId, status);
        }

        public Task<Dictionary<string, string>> GetAllStatuses()
        {
            return Task.FromResult(UserStatuses.ToDictionary(x => x.Key, x => x.Value));
        }
    }
} 