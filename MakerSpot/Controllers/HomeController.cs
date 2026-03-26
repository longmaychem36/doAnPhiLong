using MakerSpot.Models;
using MakerSpot.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MakerSpot.Controllers
{
    public class HomeController : Controller
    {
        private readonly MakerSpotContext _context;

        public HomeController(MakerSpotContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string sort = "trending", string? search = null, string? topic = null)
        {
            var query = _context.Products
                .Include(p => p.ProductTopics)
                    .ThenInclude(pt => pt.Topic)
                .Where(p => p.Status == "Approved")
                .AsNoTracking();

            // Handle Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || p.Tagline.Contains(search));
            }

            // Handle Topic Filter
            if (!string.IsNullOrWhiteSpace(topic))
            {
                query = query.Where(p => p.ProductTopics.Any(pt => pt.Topic.Slug == topic));
            }

            if (sort == "newest")
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                // Trending
                query = query.OrderByDescending(p => p.UpvoteCount)
                             .ThenByDescending(p => p.CreatedAt);
            }

            var products = await query.ToListAsync();
            var topics = await _context.Topics.OrderBy(t => t.TopicName).ToListAsync();

            var vm = new HomeViewModel
            {
                Products = products,
                SortBy = sort,
                SearchQuery = search,
                SelectedTopicSlug = topic,
                Topics = topics
            };

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
