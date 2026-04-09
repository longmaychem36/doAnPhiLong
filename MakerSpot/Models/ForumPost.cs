using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class ForumPost
    {
        [Key]
        public int ForumPostId { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        [MaxLength(50)]
        public string? Tag { get; set; } // Hỏi đáp, Chia sẻ, Thảo luận, Tuyển dụng ...

        public int ViewCount { get; set; } = 0;
        public bool IsPinned { get; set; } = false;
        public bool IsLocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public virtual User User { get; set; } = null!;
        public virtual ICollection<ForumReply> Replies { get; set; } = new List<ForumReply>();
    }
}
