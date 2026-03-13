using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class ProductMaker
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        
        [MaxLength(50)]
        public string? MakerRole { get; set; }
        
        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
