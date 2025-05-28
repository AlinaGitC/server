using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.DTOs;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using server.Hubs;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;
        public GroupController(AppDbContext context, IHubContext<ChatHub> chatHub) { _context = context; _chatHub = chatHub; }

        /// <summary>
        /// Получить все группы
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<GroupDto>>> GetAll()
        {
            var groups = await _context.Group.ToListAsync();
            var dtos = groups.Select(g => new GroupDto { Id = g.ID, Name = g.Name }).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Получить группу по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupDto>> Get(int id)
        {
            var group = await _context.Group.FindAsync(id);
            if (group == null) return NotFound();
            var dto = new GroupDto { Id = group.ID, Name = group.Name };
            return Ok(dto);
        }

        /// <summary>
        /// Создать группу и добавить создателя в участники
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupDto dto)
        {
            var group = new Group { Name = dto.Name };
            _context.Group.Add(group);
            await _context.SaveChangesAsync();

            // Добавляем создателя в участники группы
            if (dto.CreatorUserId != 0)
            {
                var member = new GroupMember { GroupId = group.ID, UserId = dto.CreatorUserId };
                _context.GroupMember.Add(member);
                await _context.SaveChangesAsync();
            }

            return Ok(new { id = group.ID });
        }

        /// <summary>
        /// Обновить группу
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateGroupDto dto)
        {
            var group = await _context.Group.FindAsync(id);
            if (group == null) return NotFound();
            group.Name = dto.Name;
            await _context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Удалить группу
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.Group.FindAsync(id);
            if (group == null) return NotFound();
            _context.Group.Remove(group);
            await _context.SaveChangesAsync();

            // SignalR: оповестить участников группы об удалении
            await _chatHub.Clients.Group($"group_{id}")
                .SendAsync("GroupDeleted", id);

            return Ok();
        }

        /// <summary>
        /// Добавить чат в группу
        /// </summary>
        [HttpPost("{groupId}/add-chat/{chatId}")]
        public async Task<IActionResult> AddChatToGroup(int groupId, int chatId)
        {
            if (!await _context.Group.AnyAsync(g => g.ID == groupId) || !await _context.Chat.AnyAsync(c => c.ID == chatId))
                return NotFound();
            if (await _context.ChatGroup.AnyAsync(cg => cg.ID_Group == groupId && cg.ID_Chat == chatId))
                return BadRequest("Чат уже в группе");
            var chatGroup = new ChatGroup { ID_Group = groupId, ID_Chat = chatId };
            _context.ChatGroup.Add(chatGroup);
            await _context.SaveChangesAsync();

            // SignalR: оповестить о добавлении чата в группу
            await _chatHub.Clients.Group($"group_{groupId}")
                .SendAsync("ChatAddedToGroup", chatId);

            return Ok();
        }

        /// <summary>
        /// Удалить чат из группы
        /// </summary>
        [HttpDelete("{groupId}/remove-chat/{chatId}")]
        public async Task<IActionResult> RemoveChatFromGroup(int groupId, int chatId)
        {
            var chatGroup = await _context.ChatGroup.FirstOrDefaultAsync(cg => cg.ID_Group == groupId && cg.ID_Chat == chatId);
            if (chatGroup == null) return NotFound();
            _context.ChatGroup.Remove(chatGroup);
            await _context.SaveChangesAsync();

            // SignalR: оповестить о удалении чата из группы
            await _chatHub.Clients.Group($"group_{groupId}")
                .SendAsync("ChatRemovedFromGroup", chatId);

            return Ok();
        }

        /// <summary>
        /// Получить все чаты группы
        /// </summary>
        [HttpGet("{groupId}/chats")]
        public async Task<ActionResult<List<ChatDto>>> GetChatsOfGroup(int groupId)
        {
            var chats = await _context.ChatGroup
                .Where(cg => cg.ID_Group == groupId)
                .Include(cg => cg.Chat)
                .Select(cg => new ChatDto
                {
                    Id = cg.Chat.ID,
                    Name = cg.Chat.Name,
                    ChatTopic = cg.Chat.ChatTopic,
                    CreatedByUserId = cg.Chat.ID_User_CreatedBy,
                    CreatedDate = cg.Chat.CreatedDate
                })
                .ToListAsync();
            return Ok(chats);
        }

        /// <summary>
        /// Получить все группы чата
        /// </summary>
        [HttpGet("chat/{chatId}/groups")]
        public async Task<ActionResult<List<GroupDto>>> GetGroupsOfChat(int chatId)
        {
            var groups = await _context.ChatGroup
                .Where(cg => cg.ID_Chat == chatId)
                .Include(cg => cg.Group)
                .Select(cg => new GroupDto { Id = cg.Group.ID, Name = cg.Group.Name })
                .ToListAsync();
            return Ok(groups);
        }

        /// <summary>
        /// Получить все группы, в которых состоит пользователь
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<GroupDto>>> GetUserGroups(int userId)
        {
            var groups = await _context.GroupMember
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                .Select(gm => new GroupDto
                {
                    Id = gm.Group.ID,
                    Name = gm.Group.Name
                })
                .ToListAsync();
            return Ok(groups);
        }
    }
} 