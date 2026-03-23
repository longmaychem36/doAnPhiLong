using System.Security.Claims;
using MakerSpot.Models;
using MakerSpot.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Controllers
{
    public class CollectionController : Controller
    {
        private readonly MakerSpotContext _context;

        public CollectionController(MakerSpotContext context)
        {
            _context = context;
        }

        private int? GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(str, out var id) ? id : null;
        }

        // GET: /Collection/MyCollections
        [Authorize]
        public async Task<IActionResult> MyCollections()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var collections = await _context.Collections
                .Include(c => c.CollectionItems)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(collections);
        }

        // GET: /Collection/Detail/5
        public async Task<IActionResult> Detail(int id)
        {
            var userId = GetUserId();

            var collection = await _context.Collections
                .Include(c => c.User)
                .Include(c => c.CollectionItems)
                    .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.ProductTopics)
                    .ThenInclude(pt => pt.Topic)
                .FirstOrDefaultAsync(c => c.CollectionId == id);

            if (collection == null) return NotFound();

            // Private collections only viewable by owner
            if (!collection.IsPublic && (userId == null || userId != collection.UserId))
                return NotFound();

            ViewBag.IsOwner = userId != null && userId == collection.UserId;
            return View(collection);
        }

        // GET: /Collection/Create
        [Authorize]
        public IActionResult Create()
        {
            return View(new CollectionFormViewModel());
        }

        // POST: /Collection/Create
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CollectionFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var collection = new Collection
            {
                UserId = userId.Value,
                CollectionName = model.CollectionName,
                Description = model.Description,
                IsPublic = model.IsPublic
            };

            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo collection thành công!";
            return RedirectToAction("MyCollections");
        }

        // GET: /Collection/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var collection = await _context.Collections.FindAsync(id);
            if (collection == null || collection.UserId != userId) return NotFound();

            var vm = new CollectionFormViewModel
            {
                CollectionId = collection.CollectionId,
                CollectionName = collection.CollectionName,
                Description = collection.Description,
                IsPublic = collection.IsPublic
            };
            return View(vm);
        }

        // POST: /Collection/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CollectionFormViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = GetUserId();
            var collection = await _context.Collections.FindAsync(id);
            if (collection == null || collection.UserId != userId) return NotFound();

            collection.CollectionName = model.CollectionName;
            collection.Description = model.Description;
            collection.IsPublic = model.IsPublic;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật collection thành công!";
            return RedirectToAction("Detail", new { id });
        }

        // POST: /Collection/Delete/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var collection = await _context.Collections.FindAsync(id);
            if (collection == null || collection.UserId != userId) return NotFound();

            _context.Collections.Remove(collection);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa collection thành công!";
            return RedirectToAction("MyCollections");
        }

        // POST: /Collection/AddItem
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(int collectionId, int productId, string returnUrl)
        {
            var userId = GetUserId();
            var collection = await _context.Collections.FindAsync(collectionId);
            if (collection == null || collection.UserId != userId) return NotFound();

            var exists = await _context.CollectionItems
                .AnyAsync(ci => ci.CollectionId == collectionId && ci.ProductId == productId);

            if (!exists)
            {
                _context.CollectionItems.Add(new CollectionItem
                {
                    CollectionId = collectionId,
                    ProductId = productId
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào collection!";
            }
            else
            {
                TempData["ErrorMessage"] = "Sản phẩm đã có trong collection!";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Detail", new { id = collectionId });
        }

        // POST: /Collection/RemoveItem
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int collectionId, int productId, string returnUrl)
        {
            var userId = GetUserId();
            var collection = await _context.Collections.FindAsync(collectionId);
            if (collection == null || collection.UserId != userId) return NotFound();

            var item = await _context.CollectionItems
                .FirstOrDefaultAsync(ci => ci.CollectionId == collectionId && ci.ProductId == productId);

            if (item != null)
            {
                _context.CollectionItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi collection!";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Detail", new { id = collectionId });
        }
    }
}
