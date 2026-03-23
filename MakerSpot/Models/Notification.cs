using System;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = null!; // NewUpvote, NewComment, NewFollower

        public int? ReferenceId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
