using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class ModeratorTopic
    {
        public int UserId { get; set; }
        public int TopicId { get; set; }
        
        public User User { get; set; } = null!;
        public Topic Topic { get; set; } = null!;
    }
}
