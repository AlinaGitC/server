using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("ChatMembers")]
    public class ChatMember
    {
        public int ID { get; set; }
        public int ID_Userr { get; set; }
        public int ID_Chat { get; set; }
        public int ID_ChatRole { get; set; }
        public DateTime DateJoining { get; set; }

        public Userr Userr { get; set; }
        public Chat Chat { get; set; }
        public ChatRole ChatRole { get; set; }
    }
} 