using System;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class ProductMedia
    {
        [Key]
        public int MediaId { get; set; }
        
        public int ProductId { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string MediaType { get; set; } = null!;
        
        [Required]
        [MaxLength(255)]
        public string MediaUrl { get; set; } = null!;
        
        [MaxLength(255)]
        public string? ThumbnailUrl { get; set; }
        
        public int DisplayOrder { get; set; } = 1;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public Product Product { get; set; } = null!;
    }
}
