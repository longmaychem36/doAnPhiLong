using MakerSpot.Models;

namespace MakerSpot.ViewModels
{
    public class UserProfileViewModel
    {
        public User User { get; set; } = null!;
        public List<Product> SubmittedProducts { get; set; } = new List<Product>();
        public List<Product> UpvotedProducts { get; set; } = new List<Product>();
    }
}
