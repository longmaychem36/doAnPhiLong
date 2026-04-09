using System;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    /// <summary>
    /// Báo cáo vi phạm: User gửi report về Product/Comment/User → Admin/Mod xử lý.
    /// </summary>
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        public int ReporterUserId { get; set; }

        [Required]
        [MaxLength(30)]
        public string TargetType { get; set; } = null!; // "Product", "Comment", "User"

        public int TargetId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Reason { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = "Pending"; // "Pending", "Reviewed", "Rejected"

        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public User ReporterUser { get; set; } = null!;
        public User? Reviewer { get; set; }
    }
}
