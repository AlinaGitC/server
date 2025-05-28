using Microsoft.AspNetCore.Mvc;
using server.Models;
using Microsoft.EntityFrameworkCore;
using server.DTOs;
using Microsoft.AspNetCore.SignalR;
using server.Hubs;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;
        public ChatController(AppDbContext context, IHubContext<ChatHub> chatHub) { _context = context; _chatHub = chatHub; }

        /// <summary>
        /// Создать новый чат (групповой или личный)
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateChatDto dto)
        {
            // Если личный чат — ищем существующий между этими пользователями
            if (dto.ID_ChatType == 1 && dto.MemberUserId != 0 && dto.MemberUserId != dto.ID_User_CreatedBy)
            {
                var existingChatId = await _context.Chat
                    .Where(c => c.ID_ChatType == 1)
                    .Join(_context.ChatMember, c => c.ID, cm => cm.ID_Chat, (c, cm) => new { c, cm })
                    .Where(x => x.cm.ID_Userr == dto.ID_User_CreatedBy || x.cm.ID_Userr == dto.MemberUserId)
                    .GroupBy(x => x.c.ID)
                    .Where(g => g.Count() == 2)
                    .Select(g => g.Key)
                    .FirstOrDefaultAsync();
                if (existingChatId != 0)
                    return Ok(new { id = existingChatId });
            }

            var chat = new Chat
            {
                Name = dto.Name,
                ID_User_CreatedBy = dto.ID_User_CreatedBy,
                ID_ChatType = dto.ID_ChatType,
                ChatTopic = dto.ChatTopic,
                CreatedDate = DateTime.UtcNow
            };
            _context.Chat.Add(chat);
            await _context.SaveChangesAsync();

            // Добавляем создателя
            var adminMember = new ChatMember
            {
                ID_Chat = chat.ID,
                ID_Userr = chat.ID_User_CreatedBy,
                ID_ChatRole = 1, // 1 — администратор
                DateJoining = DateTime.UtcNow
            };
            _context.ChatMember.Add(adminMember);

            // Если личный чат — добавляем второго пользователя
            if (dto.ID_ChatType == 1 && dto.MemberUserId != 0)
            {
                var member = new ChatMember
                {
                    ID_Chat = chat.ID,
                    ID_Userr = dto.MemberUserId,
                    ID_ChatRole = 2, // 2 — обычный участник
                    DateJoining = DateTime.UtcNow
                };
                _context.ChatMember.Add(member);
            }

            await _context.SaveChangesAsync();

            // Приветственное сообщение от бота только для группового чата
            if (chat.ID_ChatType == 2) // 2 — групповой чат
            {
                var welcome = "Привет! Я FAQ-бот. Я могу ответить на часто задаваемые вопросы. Просто напиши 'Бот, ...' и свой вопрос. Примеры: Бот, как восстановить пароль? Бот, как отправить файл в чат?";
                var botMessage = new Message
                {
                    ID_Chat = chat.ID,
                    ID_User = 100, // специальный ID для бота
                    ContentMessages = welcome,
                    Timestamp = DateTime.UtcNow
                };
                _context.Message.Add(botMessage);
                await _context.SaveChangesAsync();

                await _chatHub.Clients.Group($"chat_{chat.ID}")
                    .SendAsync("ReceiveMessage", "Бот", welcome, botMessage.Timestamp);
            }

            return Ok(new { id = chat.ID });
        }

        /// <summary>
        /// Получить чаты пользователя по его ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ChatDto>>> GetUserChats(int userId)
        {
            var chats = await _context.ChatMember
                .Where(cm => cm.ID_Userr == userId)
                .Include(cm => cm.Chat)
                .Select(cm => new ChatDto
                {
                    Id = cm.Chat.ID,
                    Name = cm.Chat.Name,
                    ChatTopic = cm.Chat.ChatTopic,
                    CreatedByUserId = cm.Chat.ID_User_CreatedBy,
                    CreatedDate = cm.Chat.CreatedDate,
                    ID_ChatType = cm.Chat.ID_ChatType
                })
                .ToListAsync();
            return Ok(chats);
        }

        /// <summary>
        /// Получить участников чата
        /// </summary>
        [HttpGet("{chatId}/members")]
        public async Task<ActionResult<List<UserDto>>> GetChatMembers(int chatId)
        {
            var onlineUserIds = server.Hubs.ChatHub.GetOnlineUserIds(); // Получаем онлайн-юзеров
            var members = await _context.ChatMember
                .Where(cm => cm.ID_Chat == chatId)
                .Include(cm => cm.Userr)
                .Select(cm => new UserDto
                {
                    Id = cm.Userr.ID,
                    Login = cm.Userr.Login,
                    Firstname = cm.Userr.Firstname,
                    MiddleName = cm.Userr.MiddleName,
                    LastName = cm.Userr.LastName,
                    Email = cm.Userr.Email,
                    Phone = cm.Userr.Phone,
                    Avatar = cm.Userr.Avatar,
                    IsActive = onlineUserIds.Contains(cm.Userr.ID)
                })
                .ToListAsync();
            return Ok(members);
        }

        /// <summary>
        /// Добавить пользователя в чат (только админ)
        /// </summary>
        [HttpPost("add-member")]
        public async Task<IActionResult> AddMember([FromBody] AddChatMemberDto dto, [FromQuery] int currentUserId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Проверка: текущий пользователь должен быть админом в этом чате
            var adminMember = await _context.ChatMember.FirstOrDefaultAsync(cm => cm.ID_Chat == dto.ID_Chat && cm.ID_Userr == currentUserId);
            if (adminMember == null)
                return StatusCode(403, "Вы не участник чата");
            var adminRole = await _context.ChatRole.FindAsync(adminMember.ID_ChatRole);
            if (adminRole == null || adminRole.Name.ToLower() != "администратор")
                return StatusCode(403, "Только администратор может добавлять участников");

            var member = new ChatMember
            {
                ID_Chat = dto.ID_Chat,
                ID_Userr = dto.ID_Userr,
                ID_ChatRole = dto.ID_ChatRole,
                DateJoining = DateTime.UtcNow
            };
            _context.ChatMember.Add(member);
            await _context.SaveChangesAsync();

            // --- Добавление пользователя в группу, если чат принадлежит группе ---
            var chatGroup = await _context.ChatGroup.FirstOrDefaultAsync(cg => cg.ID_Chat == dto.ID_Chat);
            if (chatGroup != null)
            {
                bool alreadyInGroup = await _context.GroupMember.AnyAsync(gm => gm.GroupId == chatGroup.ID_Group && gm.UserId == dto.ID_Userr);
                if (!alreadyInGroup)
                {
                    _context.GroupMember.Add(new GroupMember { GroupId = chatGroup.ID_Group, UserId = dto.ID_Userr });
                    await _context.SaveChangesAsync();
                }
            }

            // SignalR: оповестить участников чата о новом участнике
            await _chatHub.Clients.Group($"chat_{dto.ID_Chat}")
                .SendAsync("MemberAdded", dto.ID_Userr, dto.ID_ChatRole);

            // SignalR: обновить количество участников группы
            var membersCount = await _context.ChatMember.CountAsync(cm => cm.ID_Chat == dto.ID_Chat);
            await _chatHub.Clients.Group($"chat_{dto.ID_Chat}")
                .SendAsync("GroupMembersChanged", dto.ID_Chat, membersCount);

            return Ok();
        }

        /// <summary>
        /// Сменить роль участника чата (только админ)
        /// </summary>
        [HttpPut("{chatId}/member/{userId}/role")]
        public async Task<IActionResult> ChangeMemberRole(int chatId, int userId, [FromBody] ChangeChatMemberRoleDto dto, [FromQuery] int currentUserId)
        {
            // Проверка: текущий пользователь должен быть админом в этом чате
            var adminMember = await _context.ChatMember.FirstOrDefaultAsync(cm => cm.ID_Chat == chatId && cm.ID_Userr == currentUserId);
            if (adminMember == null)
                return StatusCode(403, "Вы не участник чата");
            var adminRole = await _context.ChatRole.FindAsync(adminMember.ID_ChatRole);
            if (adminRole == null || adminRole.Name.ToLower() != "администратор")
                return StatusCode(403, "Только администратор может менять роли");

            var member = await _context.ChatMember.FirstOrDefaultAsync(cm => cm.ID_Chat == chatId && cm.ID_Userr == userId);
            if (member == null) return NotFound();
            member.ID_ChatRole = dto.ID_ChatRole; // Меняем роль
            await _context.SaveChangesAsync();

            // SignalR: оповестить участников чата о смене роли
            await _chatHub.Clients.Group($"chat_{chatId}")
                .SendAsync("MemberRoleChanged", userId, dto.ID_ChatRole);

            return Ok();
        }

        /// <summary>
        /// Удалить участника из чата (только админ)
        /// </summary>
        [HttpDelete("{chatId}/member/{userId}")]
        public async Task<IActionResult> RemoveMember(int chatId, int userId, [FromQuery] int currentUserId)
        {
            // Проверка: текущий пользователь должен быть админом в этом чате
            var adminMember = await _context.ChatMember.FirstOrDefaultAsync(cm => cm.ID_Chat == chatId && cm.ID_Userr == currentUserId);
            if (adminMember == null)
                return StatusCode(403, "Вы не участник чата");
            var adminRole = await _context.ChatRole.FindAsync(adminMember.ID_ChatRole);
            if (adminRole == null || adminRole.Name.ToLower() != "администратор")
                return StatusCode(403, "Только администратор может удалять участников");

            var member = await _context.ChatMember.FirstOrDefaultAsync(cm => cm.ID_Chat == chatId && cm.ID_Userr == userId);
            if (member == null) return NotFound();
            _context.ChatMember.Remove(member);
            await _context.SaveChangesAsync();

            // SignalR: оповестить участников чата об удалении пользователя
            await _chatHub.Clients.Group($"chat_{chatId}")
                .SendAsync("MemberRemoved", userId);

            return Ok();
        }

        /// <summary>
        /// Удалить чат по ID (личный или групповой)
        /// </summary>
        [HttpDelete("delete/{chatId}")]
        public async Task<IActionResult> DeleteChat(int chatId, [FromQuery] int currentUserId)
        {
            var chat = await _context.Chat.Include(c => c.Messages).FirstOrDefaultAsync(c => c.ID == chatId);
            if (chat == null)
                return NotFound();

            // Проверка: только администратор может удалить групповой чат
            if (chat.ID_ChatType == 2) // 2 — групповой чат
            {
                var member = await _context.ChatMember.FirstOrDefaultAsync(cm => cm.ID_Chat == chatId && cm.ID_Userr == currentUserId);
                if (member == null)
                    return StatusCode(403, "Вы не участник чата");
                var role = await _context.ChatRole.FindAsync(member.ID_ChatRole);
                if (role == null || role.Name.ToLower() != "администратор")
                    return StatusCode(403, "Только администратор может удалить групповой чат");
            }

            // Удаляем связи с группами
            var chatGroups = await _context.ChatGroup.Where(cg => cg.ID_Chat == chatId).ToListAsync();
            _context.ChatGroup.RemoveRange(chatGroups);
            // Удаляем все сообщения чата
            var messages = await _context.Message.Where(m => m.ID_Chat == chatId).Include(m => m.Files).ToListAsync();
            foreach (var message in messages)
            {
                if (message.Files != null)
                {
                    foreach (var file in message.Files)
                        _context.File.Remove(file);
                }
                _context.Message.Remove(message);
            }
            // Удаляем участников чата
            var members = await _context.ChatMember.Where(cm => cm.ID_Chat == chatId).ToListAsync();
            _context.ChatMember.RemoveRange(members);
            // Удаляем сам чат
            _context.Chat.Remove(chat);
            await _context.SaveChangesAsync();
            // Оповещение через SignalR (опционально)
            await _chatHub.Clients.Group($"chat_{chatId}").SendAsync("ChatDeleted", chatId);
            return Ok();
        }
    }
} 