using System.Security.Claims;
using MakerSpot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class ProductsController : Controller
    {
        private readonly MakerSpotContext _context;

        public ProductsController(MakerSpotContext context)
        {
            _context = context;
        }

        // GET: Admin/Products (All Products) - Admin Only, phân trang
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 20;
            var query = _context.Products
                .Include(p => p.User)
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Title"] = "Tất cả sản phẩm";
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            return View("List", products);
        }

        // GET: Admin/Products/Pending
        public async Task<IActionResult> Pending()
        {
            var query = _context.Products
                .Include(p => p.User)
                .Include(p => p.ProductTopics)
                .AsNoTracking()
                .Where(p => p.Status == "Pending");

            if (User.IsInRole("Moderator") && !User.IsInRole("Admin"))
            {
                if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid))
                {
                    var allowedTopicIds = await _context.ModeratorTopics
                        .Where(mt => mt.UserId == uid)
                        .Select(mt => mt.TopicId)
                        .ToListAsync();

                    query = query.Where(p => p.ProductTopics.Any(pt => allowedTopicIds.Contains(pt.TopicId)));
                }
            }

            var products = await query.OrderBy(p => p.CreatedAt).ToListAsync();

            ViewData["Title"] = "Sản phẩm chờ duyệt";
            return View("List", products);
        }
        
        // GET: Admin/Products/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.User)
                .Include(p => p.ProductTopics).ThenInclude(pt => pt.Topic)
                .Include(p => p.ProductMedia)
                .Include(p => p.ProductMakers).ThenInclude(pm => pm.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/Products/UpdateStatus — Duyệt / Từ chối / Ẩn sản phẩm + Ghi AuditLog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string returnUrl)
        {
            var product = await _context.Products.Include(p => p.ProductTopics).FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) return NotFound();

            var validStatuses = new[] { "Pending", "Approved", "Rejected", "Hidden" };
            if (!validStatuses.Contains(status))
            {
                TempData["ErrorMessage"] = "Trạng thái không hợp lệ!";
                return Redirect(returnUrl ?? "/Admin/Products");
            }

            if (User.IsInRole("Moderator") && !User.IsInRole("Admin"))
            {
                if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var modUid))
                {
                    var allowedTopicIds = await _context.ModeratorTopics
                        .Where(mt => mt.UserId == modUid)
                        .Select(mt => mt.TopicId)
                        .ToListAsync();
                    
                    if (!product.ProductTopics.Any(pt => allowedTopicIds.Contains(pt.TopicId)))
                    {
                        TempData["ErrorMessage"] = "Bạn không có quyền duyệt sản phẩm thuộc chủ đề này!";
                        return Redirect(returnUrl ?? "/Admin/Products");
                    }
                }
            }

            string oldStatus = product.Status;
            product.Status = status;

            // Ghi AuditLog
            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : (int?)null;
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                ActionName = "UpdateProductStatus",
                TableName = "Products",
                RecordId = id,
                OldData = oldStatus,
                NewData = status
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái sản phẩm thành {status}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Products/ToggleFeatured — Ghim/bỏ ghim sản phẩm nổi bật
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int id, string? returnUrl)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsFeatured = !product.IsFeatured;

            var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : (int?)null;
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                ActionName = product.IsFeatured ? "FeatureProduct" : "UnfeatureProduct",
                TableName = "Products",
                RecordId = id,
                NewData = product.IsFeatured ? "Featured" : "Unfeatured"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = product.IsFeatured
                ? $"Đã ghim \"{product.ProductName}\" lên Nổi bật!"
                : $"Đã bỏ ghim \"{product.ProductName}\" khỏi Nổi bật.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
    }
}
