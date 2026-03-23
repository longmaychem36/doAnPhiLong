using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class Collection
    {
        [Key]
        public int CollectionId { get; set; }
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CollectionName { get; set; } = null!;

        [MaxLength(300)]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<CollectionItem> CollectionItems { get; set; } = new List<CollectionItem>();
    }
}
