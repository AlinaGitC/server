using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace server.Hubs
{
    /// <summary>
    /// SignalR-хаб для чатов и онлайн-статусов
    /// </summary>
    public class ChatHub : Hub
    {
        // userId -> connection count
        private static ConcurrentDictionary<int, int> OnlineUsers = new();

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId != null)
            {
                OnlineUsers.AddOrUpdate(userId.Value, 1, (key, old) => old + 1);
                await Clients.All.SendAsync("UserStatusChanged", userId, true);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId != null)
            {
                OnlineUsers.AddOrUpdate(userId.Value, 0, (key, old) => Math.Max(0, old - 1));
                if (OnlineUsers[userId.Value] == 0)
                    await Clients.All.SendAsync("UserStatusChanged", userId, false);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Получить список онлайн-юзеров (для контроллера)
        public static IReadOnlyCollection<int> GetOnlineUserIds() => OnlineUsers.Where(x => x.Value > 0).Select(x => x.Key).ToList();

        // Пример получения userId из контекста (замените на свою логику)
        private int? GetUserId()
        {
            // Например, если userId передаётся как query string: ?userId=123
            var userIdStr = Context.GetHttpContext()?.Request.Query["userId"];
            if (int.TryParse(userIdStr, out var id))
                return id;
            return null;
        }

        // Отправка сообщения в чат
        public async Task SendMessageToChat(int chatId, string user, string message)
        {
            await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", user, message);
        }

        // Присоединение к чату (группе)
        public async Task JoinChat(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        // Покинуть чат (группу)
        public async Task LeaveChat(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }
    }
} 