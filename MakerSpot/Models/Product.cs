using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; } = null!;
        
        [Required]
        [MaxLength(180)]
        public string Slug { get; set; } = null!;
        
        [Required]
        [MaxLength(255)]
        public string Tagline { get; set; } = null!;
        
        [Required]
        public string Description { get; set; } = null!;
        
        [MaxLength(255)]
        public string? LogoUrl { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string WebsiteUrl { get; set; } = null!;
        
        [MaxLength(255)]
        public string? DemoUrl { get; set; }
        
        public DateTime? LaunchDate { get; set; }
        
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";
        
        public bool IsFeatured { get; set; } = false;
        
        public int ViewCount { get; set; } = 0;
        public int UpvoteCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public User User { get; set; } = null!;
        public ICollection<ProductMedia> ProductMedia { get; set; } = new List<ProductMedia>();
        public ICollection<ProductTopic> ProductTopics { get; set; } = new List<ProductTopic>();
        public ICollection<ProductMaker> ProductMakers { get; set; } = new List<ProductMaker>();
        public ICollection<ProductUpvote> ProductUpvotes { get; set; } = new List<ProductUpvote>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
