using System.ComponentModel.DataAnnotations;

namespace server.DTOs
{
    /// <summary>
    /// DTO для смены роли участника чата
    /// </summary>
    public class ChangeChatMemberRoleDto
    {
        [Required]
        public int ID_ChatRole { get; set; } // Новый ID роли
    }
} 