using System.ComponentModel.DataAnnotations;

namespace server.DTOs
{
    /// <summary>
    /// DTO для добавления участника в чат
    /// </summary>
    public class AddChatMemberDto
    {
        [Required]
        public int ID_Chat { get; set; }
        [Required]
        public int ID_Userr { get; set; }
        [Required]
        public int ID_ChatRole { get; set; }
    }
} 