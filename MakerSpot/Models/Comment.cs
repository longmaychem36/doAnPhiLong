using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        
        public int ProductId { get; set; }
        public int UserId { get; set; }
        
        public int? ParentCommentId { get; set; }
        
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = null!;
        
        public bool IsEdited { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        public Product Product { get; set; } = null!;
        public User User { get; set; } = null!;
        public Comment? ParentComment { get; set; }
        
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();
    }
}
