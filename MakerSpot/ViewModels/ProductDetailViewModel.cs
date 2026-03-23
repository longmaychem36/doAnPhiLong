using MakerSpot.Models;

namespace MakerSpot.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;
        public bool HasUpvoted { get; set; } = false;
        public string NewCommentContent { get; set; } = string.Empty;
        public int? ReplyToCommentId { get; set; }
        public List<Collection> UserCollections { get; set; } = new List<Collection>();
    }
}
