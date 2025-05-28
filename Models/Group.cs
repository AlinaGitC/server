using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Group")]
    public class Group
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<ChatGroup> ChatGroups { get; set; }
        public ICollection<GroupMember> GroupMembers { get; set; }
    }
} 