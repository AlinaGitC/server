using Microsoft.AspNetCore.Mvc;
using server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using server.Hubs;
using FuzzySharp;
using Microsoft.Extensions.Logging;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly ILogger<MessageController> _logger;
        public MessageController(AppDbContext context, IHubContext<ChatHub> chatHub, ILogger<MessageController> logger) { _context = context; _chatHub = chatHub; _logger = logger; }

        /// <summary>
        /// Отправить сообщение в чат
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] DTOs.SendMessageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var message = new Message
            {
                ID_Chat = dto.ID_Chat, // ID чата
                ID_User = dto.ID_User, // ID пользователя
                ContentMessages = dto.ContentMessages, // Текст сообщения
                Timestamp = DateTime.UtcNow // Время отправки
            };

            _context.Message.Add(message);
            await _context.SaveChangesAsync();

            // Привязка файлов, если есть
            if (dto.Files != null && dto.Files.Count > 0)
            {
                var attachedFiles = await _context.File.Where(f => dto.Files.Contains(f.ID)).ToListAsync();
                foreach (var file in attachedFiles)
                {
                    file.ID_Messages = message.ID;
                }
                await _context.SaveChangesAsync();
                message.Files = attachedFiles;
            }
            else
            {
                _logger.LogInformation("[DEBUG] Нет файлов для привязки к сообщению {MessageId}", message.ID);
            }

            // Оповещение участников чата через SignalR
            var files = message.Files?.Select(f => {
                return new DTOs.FileDto
                {
                    Id = f.ID,
                    FileName = f.FileName,
                    FileTypeName = f.FileType?.Name,
                    URL = f.URL
                };
            }).ToList() ?? new List<DTOs.FileDto>();
            await _chatHub.Clients.Group($"chat_{dto.ID_Chat}")
                .SendAsync("ReceiveMessage", dto.ID_User, dto.ContentMessages, message.Timestamp, files);

            // --- FAQ-бот ---
            if (dto.ContentMessages.Trim().StartsWith("Бот", StringComparison.OrdinalIgnoreCase))
            {
                var question = dto.ContentMessages.Trim().Substring(3).Trim(' ', ',', ':', '?');
                var q = question.ToLower();
                var keywords = q.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                var faqs = await _context.FAQ.ToListAsync();

                // 1. Поиск по ключевым словам (максимальное количество совпадений)
                var bestByKeywords = faqs
                    .Select(f => new { f, count = keywords.Count(k => f.Question != null && f.Question.ToLower().Contains(k)) })
                    .OrderByDescending(x => x.count)
                    .FirstOrDefault(x => x.count > 0)?.f;

                // 2. Если не найдено — fuzzy matching (FuzzySharp)
                var bestByFuzzy = faqs
                    .Select(f => new { f, score = f.Question != null ? Fuzz.TokenSetRatio(f.Question.ToLower(), q) : 0 })
                    .OrderByDescending(x => x.score)
                    .FirstOrDefault(x => x.score > 70)?.f;

                var faq = bestByKeywords ?? bestByFuzzy;
                string botAnswer;
                if (faq != null)
                    botAnswer = faq.Answer;
                else
                    botAnswer = "Извините, я не знаю ответа на этот вопрос. Обратитесь к администратору.";

                var botMessage = new Message
                {
                    ID_Chat = dto.ID_Chat,
                    ID_User = 100, // специальный ID для бота
                    ContentMessages = botAnswer,
                    Timestamp = DateTime.UtcNow
                };
                _context.Message.Add(botMessage);
                await _context.SaveChangesAsync();

                await _chatHub.Clients.Group($"chat_{dto.ID_Chat}")
                    .SendAsync("ReceiveMessage", 100, botAnswer, botMessage.Timestamp, new List<DTOs.FileDto>());
            }

            return Ok(new { message.ID });
        }

        /// <summary>
        /// Получить сообщения чата
        /// </summary>
        [HttpGet("chat/{chatId}")]
        public async Task<ActionResult<List<DTOs.MessageDto>>> GetChatMessages(int chatId)
        {
            var messages = await _context.Message
                .Where(m => m.ID_Chat == chatId)
                .OrderBy(m => m.Timestamp)
                .Include(m => m.Userr)
                .Include(m => m.Files)
                .ThenInclude(f => f.FileType)
                .Select(m => new DTOs.MessageDto
                {
                    Id = m.ID,
                    ChatId = m.ID_Chat,
                    UserId = m.ID_User,
                    UserLogin = m.Userr.Login,
                    Text = m.ContentMessages,
                    Timestamp = m.Timestamp,
                    Files = m.Files.Select(f => new DTOs.FileDto
                    {
                        Id = f.ID,
                        FileName = f.FileName,
                        FileTypeName = f.FileType != null ? f.FileType.Name : null,
                        URL = f.URL
                    }).ToList()
                })
                .ToListAsync();
            return Ok(messages);
        }

        /// <summary>
        /// Удалить сообщение по ID (удаляет у всех)
        /// </summary>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.Message.Include(m => m.Files).FirstOrDefaultAsync(m => m.ID == id);
            if (message == null)
                return NotFound();
            _context.Message.Remove(message);
            await _context.SaveChangesAsync();
            await _chatHub.Clients.Group($"chat_{message.ID_Chat}").SendAsync("MessageDeleted", id);
            return Ok();
        }
    }
} 