using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace server.DTOs
{
    /// <summary>
    /// DTO для отправки сообщения в чат
    /// </summary>
    public class SendMessageDto
    {
        [Required]
        public int ID_Chat { get; set; } // ID чата
        [Required]
        public int ID_User { get; set; } // ID пользователя
        [Required]
        public string ContentMessages { get; set; } // Текст сообщения
        public List<int>? Files { get; set; } = new(); // Список ID файлов (если есть)
    }
} 