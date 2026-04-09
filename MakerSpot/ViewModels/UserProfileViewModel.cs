using MakerSpot.Models;

namespace MakerSpot.ViewModels
{
    public class UserProfileViewModel
    {
        public User User { get; set; } = null!;
        public List<Product> SubmittedProducts { get; set; } = new List<Product>();
        public List<Product> UpvotedProducts { get; set; } = new List<Product>();
        
        // Phase 4
        public int FollowerCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsFollowing { get; set; }
        public List<Collection> Collections { get; set; } = new List<Collection>();
        
        // Phase 8: Maker Stats
        public int TotalUpvotesReceived { get; set; }
        public int TotalCommentsReceived { get; set; }
        public Product? TopProduct { get; set; }
    }
}
