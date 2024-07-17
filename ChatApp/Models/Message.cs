using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ChatApp.Models
{
    public class Message
    {
        public int Id { get; set; }
        
        [Required]
        public int ChatId { get; set; }
        
        public string User { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }

        [JsonIgnore] 
        public Chat Chat { get; set; }
    }
}
