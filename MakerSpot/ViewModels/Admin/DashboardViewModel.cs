using System.Collections.Generic;
using MakerSpot.Models;

namespace MakerSpot.ViewModels.Admin
{
    public class DashboardViewModel
    {
        // Global Metrics
        public int TotalProducts { get; set; }
        public int TotalPendingProducts { get; set; }
        public int TotalApprovedProducts { get; set; }
        public int TotalRejectedProducts { get; set; }
        public int TotalHiddenProducts { get; set; }
        
        public int TotalUsers { get; set; }
        public int TotalComments { get; set; }
        public int TotalUpvotes { get; set; }
        public int TotalCollections { get; set; }
        public int TotalUnreadNotis { get; set; }
        
        // Lists
        public List<Product> RecentPendingProducts { get; set; } = new List<Product>();
        public List<TopProductStat> TopProducts { get; set; } = new List<TopProductStat>();
        public List<TopUserStat> TopMakers { get; set; } = new List<TopUserStat>();
        public List<TopUserStat> TopFollowedUsers { get; set; } = new List<TopUserStat>();
        public List<TopTopicStat> TopTopics { get; set; } = new List<TopTopicStat>();
    }

    public class TopProductStat
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? LogoUrl { get; set; }
        public int UpvoteCount { get; set; }
    }

    public class TopUserStat
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int Count { get; set; } // Can be ProductCount or FollowerCount depending on list
    }

    public class TopTopicStat
    {
        public string TopicName { get; set; } = null!;
        public int ProductCount { get; set; }
        public double Percentage { get; set; }
    }
}
