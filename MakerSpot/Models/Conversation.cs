using System;
using System.Collections.Generic;

namespace MakerSpot.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set; }

        public DateTime LastMessageAt { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual User User1 { get; set; } = null!;
        public virtual User User2 { get; set; } = null!;
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

        // Tiện ích để lấy User khác với id của user hiện tại
        public User GetOtherUser(int currentUserId)
        {
            return User1Id == currentUserId ? User2 : User1;
        }
    }
}
