using Microsoft.AspNetCore.Mvc;
using server.Models;
using Microsoft.EntityFrameworkCore;
using server.DTOs;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FileController(AppDbContext context) { _context = context; }

        /// <summary>
        /// Загрузка изображения (только image/*)
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
        {
            var file = dto.File;
            if (file == null)
                return BadRequest("Файл не выбран");

            // Определяем тип файла (по умолчанию 2 = документ, 1 = изображение)
            int fileType = file.ContentType.StartsWith("image/") ? 1 : 2;

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var entity = new Models.File
            {
                FileName = file.FileName,
                FileDate = ms.ToArray(),
                URL = null,
                ID_FileType = fileType
            };
            _context.File.Add(entity);
            await _context.SaveChangesAsync();

            // Формируем URL для скачивания
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var fileUrl = $"{baseUrl}/api/File/{entity.ID}";
            entity.URL = fileUrl;
            await _context.SaveChangesAsync();

            return Ok(new { entity.ID, entity.ID_FileType, entity.FileName, Url = fileUrl });
        }

        /// <summary>
        /// Получить изображение по id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var file = await _context.File.FirstOrDefaultAsync(f => f.ID == id);
            if (file == null) return NotFound();
            return File(file.FileDate, "application/octet-stream", file.FileName);
        }
    }
} 