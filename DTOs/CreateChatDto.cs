namespace server.DTOs
{
    public class CreateChatDto
    {
        public string Name { get; set; }
        public int ID_User_CreatedBy { get; set; }
        public int ID_ChatType { get; set; }
        public string ChatTopic { get; set; }
        public int MemberUserId { get; set; }
    }
} 