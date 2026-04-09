using MakerSpot.Models;

namespace MakerSpot.ViewModels
{
    public class HomeViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();
        public string SortBy { get; set; } = "trending"; // trending, newest
        
        // Search & Filter
        public string? SearchQuery { get; set; }
        public string? SelectedTopicSlug { get; set; }
        public List<Topic> Topics { get; set; } = new List<Topic>();
        
        // Phân trang
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}
