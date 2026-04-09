using System;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class ForumReply
    {
        [Key]
        public int ForumReplyId { get; set; }

        public int ForumPostId { get; set; }
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual ForumPost ForumPost { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
