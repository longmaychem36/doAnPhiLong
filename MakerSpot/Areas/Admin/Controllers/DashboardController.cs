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
            // Calculate global totals
            var totalProducts = await _context.Products.CountAsync();
            
            var vm = new DashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalPendingProducts = await _context.Products.CountAsync(p => p.Status == "Pending"),
                TotalApprovedProducts = await _context.Products.CountAsync(p => p.Status == "Approved"),
                TotalRejectedProducts = await _context.Products.CountAsync(p => p.Status == "Rejected"),
                TotalHiddenProducts = await _context.Products.CountAsync(p => p.Status == "Hidden"),
                
                TotalUsers = await _context.Users.CountAsync(),
                TotalComments = await _context.Comments.CountAsync(c => !c.IsDeleted),
                TotalUpvotes = await _context.ProductUpvotes.CountAsync(),
                TotalCollections = await _context.Collections.CountAsync(),
                TotalUnreadNotis = await _context.Notifications.CountAsync(n => !n.IsRead)
            };

            // Queries for Top Lists
            vm.RecentPendingProducts = await _context.Products
                .Include(p => p.User)
                .AsNoTracking()
                .Where(p => p.Status == "Pending")
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            vm.TopProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == "Approved")
                .OrderByDescending(p => p.UpvoteCount)
                .Take(5)
                .Select(p => new TopProductStat
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    LogoUrl = p.LogoUrl,
                    UpvoteCount = p.UpvoteCount
                })
                .ToListAsync();

            vm.TopMakers = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status == "Approved")
                .GroupBy(p => p.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    ProductCount = g.Count()
                })
                .OrderByDescending(x => x.ProductCount)
                .Take(5)
                .Join(_context.Users, 
                      stat => stat.UserId, 
                      u => u.UserId, 
                      (stat, u) => new TopUserStat 
                      { 
                          UserId = u.UserId, 
                          UserName = u.Username, 
                          FullName = u.FullName,
                          Count = stat.ProductCount 
                      })
                .ToListAsync();

            vm.TopFollowedUsers = await _context.Followers
                .AsNoTracking()
                .GroupBy(f => f.FollowingId)
                .Select(g => new
                {
                    UserId = g.Key,
                    FollowerCount = g.Count()
                })
                .OrderByDescending(x => x.FollowerCount)
                .Take(5)
                .Join(_context.Users, 
                      stat => stat.UserId, 
                      u => u.UserId, 
                      (stat, u) => new TopUserStat 
                      { 
                          UserId = u.UserId, 
                          UserName = u.Username, 
                          FullName = u.FullName,
                          Count = stat.FollowerCount 
                      })
                .ToListAsync();

            // Calculate Topic percentages
            var topicGroups = await _context.ProductTopics
                .Include(pt => pt.Product)
                .AsNoTracking()
                .Where(pt => pt.Product.Status == "Approved")
                .GroupBy(pt => pt.TopicId)
                .Select(g => new
                {
                    TopicId = g.Key,
                    ProductCount = g.Count()
                })
                .OrderByDescending(x => x.ProductCount)
                .Take(5)
                .ToListAsync();

            var totalApprovedTopics = topicGroups.Sum(tg => tg.ProductCount);

            foreach (var tg in topicGroups)
            {
                var topicName = await _context.Topics.Where(t => t.TopicId == tg.TopicId).Select(t => t.TopicName).FirstOrDefaultAsync();
                vm.TopTopics.Add(new TopTopicStat
                {
                    TopicName = topicName ?? "Unknown",
                    ProductCount = tg.ProductCount,
                    Percentage = totalApprovedTopics > 0 ? (tg.ProductCount * 100.0 / totalApprovedTopics) : 0
                });
            }

            return View(vm);
        }
    }
}
