using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MakerSpot.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = null!;
        
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
