using System.ComponentModel.DataAnnotations;

namespace server.DTOs
{
    /// <summary>
    /// DTO для создания роли чата
    /// </summary>
    public class CreateChatRoleDto
    {
        [Required]
        public string Name { get; set; } // Название роли
    }
} 