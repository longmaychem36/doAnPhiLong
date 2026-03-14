using MakerSpot.Models;
using MakerSpot.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class DashboardController : Controller
    {
        private readonly MakerSpotContext _context;

        public DashboardController(MakerSpotContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalPendingProducts = await _context.Products.CountAsync(p => p.Status == "Pending"),
                TotalUsers = await _context.Users.CountAsync(),
                TotalComments = await _context.Comments.Where(c => !c.IsDeleted).CountAsync(),
                RecentPendingProducts = await _context.Products
                    .Include(p => p.User)
                    .Where(p => p.Status == "Pending")
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
