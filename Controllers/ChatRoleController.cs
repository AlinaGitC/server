using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.DTOs;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatRoleController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ChatRoleController(AppDbContext context) { _context = context; }

        /// <summary>
        /// Получить все роли чата
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _context.ChatRole.ToListAsync();
            return Ok(roles);
        }

        /// <summary>
        /// Получить роль чата по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var role = await _context.ChatRole.FindAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        /// <summary>
        /// Создать роль чата
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateChatRoleDto dto)
        {
            var role = new ChatRole { Name = dto.Name };
            _context.ChatRole.Add(role);
            await _context.SaveChangesAsync();
            return Ok(new { id = role.ID });
        }

        /// <summary>
        /// Обновить роль чата
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateChatRoleDto dto)
        {
            var role = await _context.ChatRole.FindAsync(id);
            if (role == null) return NotFound();
            role.Name = dto.Name;
            await _context.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Удалить роль чата
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _context.ChatRole.FindAsync(id);
            if (role == null) return NotFound();
            _context.ChatRole.Remove(role);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
} 