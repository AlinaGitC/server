using System.ComponentModel.DataAnnotations;

namespace server.DTOs
{
    /// <summary>
    /// DTO для обновления группы чатов
    /// </summary>
    public class UpdateGroupDto
    {
        [Required]
        public string Name { get; set; } // Новое название группы
    }
} 