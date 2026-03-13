using System;

namespace MakerSpot.Models
{
    public class CommentVote
    {
        public int CommentId { get; set; }
        public int UserId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Comment Comment { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
