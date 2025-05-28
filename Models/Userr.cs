using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    [Table("Userr")]
    public class Userr
    {
        public int ID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Firstname { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public byte[]? Avatar { get; set; }
        public bool IsActive { get; set; }

        public ICollection<ChatMember> ChatMembers { get; set; }
        public ICollection<GroupMember> GroupMembers { get; set; }
        public ICollection<Message> Messages { get; set; }
        
    }
} 