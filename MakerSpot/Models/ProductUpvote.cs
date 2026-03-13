using System;

namespace MakerSpot.Models
{
    public class ProductUpvote
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
