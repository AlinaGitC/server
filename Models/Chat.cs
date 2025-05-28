using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Chat")]
    public class Chat
    {
        public int ID { get; set; }
        public string Name { get; set; }
        [ForeignKey("ID_User_CreatedBy")]
        public int ID_User_CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        [ForeignKey("ID_ChatType")]
        public int ID_ChatType { get; set; }
        public string ChatTopic { get; set; }

        public Userr Userr { get; set; }
        public ChatType ChatType { get; set; }
        public ICollection<ChatMember> ChatMembers { get; set; }
        public ICollection<Message> Messages { get; set; }
        public ICollection<ChatGroup> ChatGroups { get; set; }
    }
} 