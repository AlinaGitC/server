using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("ChatRole")]
    public class ChatRole
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<ChatMember> ChatMembers { get; set; }
    }
} 