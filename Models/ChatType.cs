using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("ChatType")]
    public class ChatType
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<Chat> Chats { get; set; }
    }
} 