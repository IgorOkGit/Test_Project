namespace ChatApp.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<ChatUser> Users { get; set; }
    }

    public class ChatUser
    {
        public string UserId { get; set; }
        public int ChatId { get; set; }
        public Chat Chat { get; set; }
    }
}
