using System.ComponentModel.DataAnnotations;

namespace server.DTOs
{
    /// <summary>
    /// DTO для создания группы чатов
    /// </summary>
    public class CreateGroupDto
    {
        [Required]
        public string Name { get; set; } // Название группы
        public int CreatorUserId { get; set; } // Id создателя группы
    }
} 