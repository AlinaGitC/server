using FuzzySharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using server.Hubs;
using server.Models;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FAQController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;
        public FAQController(AppDbContext context, IHubContext<ChatHub> chatHub) { _context = context; _chatHub = chatHub; }

        /// <summary>
        /// Получить все вопросы и ответы FAQ
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var faqs = await _context.FAQ.ToListAsync();
            return Ok(faqs);
        }

        /// <summary>
        /// Получить ответ на вопрос (поиск по подстроке)
        /// </summary>
        [HttpGet("ask")]
        public async Task<IActionResult> Ask([FromQuery] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return BadRequest("Вопрос не задан");
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
                .FirstOrDefault(x => x.score > 50)?.f; // порог можно подобрать

            var faq = bestByKeywords ?? bestByFuzzy;

            if (faq == null)
                return Ok(new { answer = "Извините, я не знаю ответа на этот вопрос." });
            return Ok(new { answer = faq.Answer });
        }

        /// <summary>
        /// Бот: отправить ответ прямо в чат (SignalR)
        /// </summary>
        [HttpPost("ask-to-chat")]
        public async Task<IActionResult> AskToChat([FromQuery] int chatId, [FromBody] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return BadRequest("Вопрос не задан");
            var q = question.ToLower();
            var keywords = q.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            var faqs = await _context.FAQ.ToListAsync();

            var bestByKeywords = faqs
                .Select(f => new { f, count = keywords.Count(k => f.Question != null && f.Question.ToLower().Contains(k)) })
                .OrderByDescending(x => x.count)
                .FirstOrDefault(x => x.count > 0)?.f;

            var bestByFuzzy = faqs
                .Select(f => new { f, score = f.Question != null ? Fuzz.TokenSetRatio(f.Question.ToLower(), q) : 0 })
                .OrderByDescending(x => x.score)
                .FirstOrDefault(x => x.score > 70)?.f;

            var faq = bestByKeywords ?? bestByFuzzy;
            var answer = faq?.Answer ?? "Извините, я не знаю ответа на этот вопрос.";
            // SignalR: отправить ответ в чат
            await _chatHub.Clients.Group($"chat_{chatId}")
                .SendAsync("BotAnswer", answer);
            return Ok(new { answer });
        }
    }
}