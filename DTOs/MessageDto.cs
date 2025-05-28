using System;
using System.Collections.Generic;

namespace server.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public string UserLogin { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public List<FileDto> Files { get; set; }
    }
} 