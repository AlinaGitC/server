using System;

namespace server.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Firstname { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public byte[]? Avatar { get; set; }
        public bool IsActive { get; set; }
    }
} 