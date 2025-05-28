using System.ComponentModel.DataAnnotations;

namespace server.DTOs
{
    /// <summary>
    /// DTO для обновления роли чата
    /// </summary>
    public class UpdateChatRoleDto
    {
        [Required]
        public string Name { get; set; } // Новое название роли
    }
} 