using System.Collections.Generic;
using MakerSpot.Models;

namespace MakerSpot.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalPendingProducts { get; set; }
        public int TotalUsers { get; set; }
        public int TotalComments { get; set; }
        
        public List<Product> RecentPendingProducts { get; set; } = new List<Product>();
    }
}
