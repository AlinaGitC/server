namespace server.DTOs
{
    public class ChatDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ChatTopic { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ID_ChatType { get; set; } // Тип чата: 1 — личный, 2 — групповой
    }
} 