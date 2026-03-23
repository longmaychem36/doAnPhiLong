using System;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class Follower
    {
        public int FollowerId { get; set; }
        public int FollowingId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User FollowerUser { get; set; } = null!;
        public User FollowingUser { get; set; } = null!;
    }
}
