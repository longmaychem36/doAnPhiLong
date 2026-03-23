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

        // GET: Admin/Products (All Products) - Admin Only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewData["Title"] = "Tất cả sản phẩm";
            return View("List", products);
        }

        // GET: Admin/Products/Pending
        public async Task<IActionResult> Pending()
        {
            var products = await _context.Products
                .Include(p => p.User)
                .Where(p => p.Status == "Pending")
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

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
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string returnUrl)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Valid constraints (Pending, Approved, Rejected, Hidden)
            var validStatuses = new[] { "Pending", "Approved", "Rejected", "Hidden" };
            if (!validStatuses.Contains(status))
            {
                TempData["ErrorMessage"] = "Trạng thái không hợp lệ!";
                return Redirect(returnUrl ?? "/Admin/Products");
            }

            product.Status = status;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật trạng thái sản phẩm thành {status}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
