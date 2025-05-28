using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Messages")]
    public class Message
    {
        public int ID { get; set; }
        public string ContentMessages { get; set; }
        public DateTime Timestamp { get; set; }
        [ForeignKey("ID_User")]
        public int ID_User { get; set; }
        public int ID_Chat { get; set; }
        
        public Userr Userr { get; set; }
        public Chat Chat { get; set; }
        public ICollection<File> Files { get; set; }
    }
} 