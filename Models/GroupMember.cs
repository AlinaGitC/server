using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("GroupMember")]
    public class GroupMember
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }

        public Group Group { get; set; }
        public Userr User { get; set; }
    }
}