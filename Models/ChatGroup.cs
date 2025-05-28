using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("ChatGroup")]
    public class ChatGroup
    {
        public int ID { get; set; }
        public int ID_Chat { get; set; }
        public int ID_Group { get; set; }

        public Chat Chat { get; set; }
        public Group Group { get; set; }
    }
} 