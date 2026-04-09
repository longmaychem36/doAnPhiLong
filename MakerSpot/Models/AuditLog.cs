using System;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    /// <summary>
    /// Nhật ký hệ thống: Ghi lại mọi hành động quan trọng của Admin/Moderator.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int LogId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ActionName { get; set; } = null!; // "ApproveProduct", "LockUser", ...

        [Required]
        [MaxLength(100)]
        public string TableName { get; set; } = null!; // "Products", "Users", ...

        public int? RecordId { get; set; }

        public string? OldData { get; set; }
        public string? NewData { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public User? User { get; set; }
    }
}
