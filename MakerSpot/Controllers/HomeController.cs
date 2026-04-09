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

        // GET: / — Home Feed với phân trang và bộ lọc
        public async Task<IActionResult> Index(string sort = "trending", string? search = null, string? topic = null, int page = 1)
        {
            const int pageSize = 20;

            var query = _context.Products
                .Include(p => p.ProductTopics)
                    .ThenInclude(pt => pt.Topic)
                .Where(p => p.Status == "Approved")
                .AsNoTracking();

            // Tìm kiếm theo tên hoặc tagline
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || p.Tagline.Contains(search));
            }

            // Lọc theo Topic slug
            if (!string.IsNullOrWhiteSpace(topic))
            {
                query = query.Where(p => p.ProductTopics.Any(pt => pt.Topic.Slug == topic));
            }

            // Sắp xếp
            if (sort == "newest")
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                query = query.OrderByDescending(p => p.UpvoteCount)
                             .ThenByDescending(p => p.CreatedAt);
            }

            // Đếm tổng để tính số trang
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var topics = await _context.Topics.AsNoTracking().OrderBy(t => t.TopicName).ToListAsync();

            // Lấy sản phẩm nổi bật (IsFeatured, tối đa 5)
            var featuredProducts = await _context.Products
                .Include(p => p.ProductTopics).ThenInclude(pt => pt.Topic)
                .Where(p => p.Status == "Approved" && p.IsFeatured)
                .AsNoTracking()
                .OrderByDescending(p => p.UpvoteCount)
                .Take(5)
                .ToListAsync();

            var vm = new HomeViewModel
            {
                Products = products,
                FeaturedProducts = featuredProducts,
                SortBy = sort,
                SearchQuery = search,
                SelectedTopicSlug = topic,
                Topics = topics,
                CurrentPage = page,
                TotalPages = totalPages
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
