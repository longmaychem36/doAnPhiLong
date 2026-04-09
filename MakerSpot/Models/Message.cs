using System;

namespace MakerSpot.Models
{
    public class Message
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }

        public string Content { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // New properties for Image and Product Sharing
        public string? ImageUrl { get; set; }
        public int? SharedProductId { get; set; }

        // Navigation properties
        public virtual Conversation Conversation { get; set; } = null!;
        public virtual User Sender { get; set; } = null!;
        public virtual Product? SharedProduct { get; set; }
    }
}
