using System;

namespace MakerSpot.Models
{
    public class CollectionItem
    {
        public int CollectionId { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public Collection Collection { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
