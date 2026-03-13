using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerSpot.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;
        
        [MaxLength(255)]
        public string? AvatarUrl { get; set; }
        
        [MaxLength(500)]
        public string? Bio { get; set; }
        
        [MaxLength(255)]
        public string? WebsiteUrl { get; set; }
        
        [MaxLength(255)]
        public string? TwitterUrl { get; set; }
        
        [MaxLength(255)]
        public string? LinkedinUrl { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<ProductUpvote> ProductUpvotes { get; set; } = new List<ProductUpvote>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
        public ICollection<ProductMaker> ProductMakers { get; set; } = new List<ProductMaker>();
    }
}
