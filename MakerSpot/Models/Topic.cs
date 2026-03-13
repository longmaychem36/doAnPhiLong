using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class Topic
    {
        [Key]
        public int TopicId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string TopicName { get; set; } = null!;
        
        [Required]
        [MaxLength(120)]
        public string Slug { get; set; } = null!;
        
        [MaxLength(300)]
        public string? Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<ProductTopic> ProductTopics { get; set; } = new List<ProductTopic>();
    }
}
