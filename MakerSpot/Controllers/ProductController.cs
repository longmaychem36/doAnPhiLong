using System.Security.Claims;
using MakerSpot.Models;
using MakerSpot.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Controllers
{
    public class ProductController : Controller
    {
        private readonly MakerSpotContext _context;

        public ProductController(MakerSpotContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products
                .Include(p => p.User)
                .Include(p => p.ProductMedia.OrderBy(m => m.DisplayOrder))
                .Include(p => p.ProductTopics).ThenInclude(pt => pt.Topic)
                .Include(p => p.ProductMakers).ThenInclude(pm => pm.User)
                .Include(p => p.Comments.Where(c => c.IsDeleted == false && c.ParentCommentId == null).OrderByDescending(c => c.CreatedAt))
                    .ThenInclude(c => c.User)
                .Include(p => p.Comments.Where(c => c.IsDeleted == false && c.ParentCommentId == null).OrderByDescending(c => c.CreatedAt))
                    .ThenInclude(c => c.Replies.Where(r => r.IsDeleted == false).OrderBy(r => r.CreatedAt))
                        .ThenInclude(r => r.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null || product.Status != "Approved")
            {
                return NotFound();
            }

            // Increment ViewCount separately to avoid tracking conflict
            await _context.Products.Where(p => p.ProductId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.ViewCount, p => p.ViewCount + 1));

            bool hasUpvoted = false;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out var userId))
                {
                    hasUpvoted = await _context.ProductUpvotes.AnyAsync(u => u.ProductId == id && u.UserId == userId);
                }
            }

            var vm = new ProductDetailViewModel
            {
                Product = product,
                HasUpvoted = hasUpvoted
            };

            // Load current user's collections for 'Add to Collection' dropdown
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var uidStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(uidStr, out var uid))
                {
                    vm.UserCollections = await _context.Collections
                        .Where(c => c.UserId == uid)
                        .OrderBy(c => c.CollectionName)
                        .ToListAsync();
                }
            }

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upvote(int productId, string? returnUrl = null)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var upvote = await _context.ProductUpvotes
                .FirstOrDefaultAsync(u => u.ProductId == productId && u.UserId == userId);

            if (upvote != null)
            {
                // Remove upvote
                _context.ProductUpvotes.Remove(upvote);
                await _context.Products.Where(p => p.ProductId == productId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.UpvoteCount, p => p.UpvoteCount - 1));
            }
            else
            {
                // Add upvote
                _context.ProductUpvotes.Add(new ProductUpvote
                {
                    ProductId = productId,
                    UserId = userId
                });
                await _context.Products.Where(p => p.ProductId == productId)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.UpvoteCount, p => p.UpvoteCount + 1));
            }

            // Save the upvote changes
            await _context.SaveChangesAsync();

            // Create notification for product owner (only on new upvote, i.e. upvote was null before adding)
            if (upvote == null)
            {
                var product = await _context.Products.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product != null && product.UserId != userId)
                {
                    var currentUser = await _context.Users.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == userId);
                    _context.Notifications.Add(new Notification
                    {
                        UserId = product.UserId,
                        Type = "NewUpvote",
                        ReferenceId = productId,
                        Message = $"{currentUser!.FullName} vừa upvote sản phẩm {product.ProductName}."
                    });
                    await _context.SaveChangesAsync();
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Detail", new { id = productId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostComment(int productId, string content, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return RedirectToAction("Detail", new { id = productId });
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var comment = new Comment
            {
                ProductId = productId,
                UserId = userId,
                Content = content,
                ParentCommentId = parentCommentId
            };

            _context.Comments.Add(comment);
            
            // Generate notification, handle comment increment safely
            await _context.Products.Where(p => p.ProductId == productId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentCount, p => p.CommentCount + 1));

            await _context.SaveChangesAsync();

            // Create notification for product owner
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product != null && product.UserId != userId)
            {
                var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
                _context.Notifications.Add(new Notification
                {
                    UserId = product.UserId,
                    Type = "NewComment",
                    ReferenceId = productId,
                    Message = $"{currentUser!.FullName} vừa bình luận trên sản phẩm {product.ProductName}."
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Detail", new { id = productId });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Submit()
        {
            var vm = new SubmitProductViewModel();
            await PopulateTopicsAsync(vm);
            vm.LaunchDate = DateTime.Today;
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(SubmitProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateTopicsAsync(model);
                return View(model);
            }

            // Check if slug is unique
            if (await _context.Products.AsNoTracking().AnyAsync(p => p.Slug == model.Slug))
            {
                ModelState.AddModelError("Slug", "Slug này đã được sử dụng. Vui lòng chọn Slug khác.");
                await PopulateTopicsAsync(model);
                return View(model);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var product = new Product
            {
                UserId = userId,
                ProductName = model.ProductName,
                Slug = model.Slug,
                Tagline = model.Tagline,
                Description = model.Description,
                LogoUrl = model.LogoUrl,
                WebsiteUrl = model.WebsiteUrl,
                DemoUrl = model.DemoUrl,
                LaunchDate = model.LaunchDate,
                Status = "Pending" // Mặc định Pending chờ Admin duyệt
            };

            // Add Topics
            if (model.SelectedTopicIds != null && model.SelectedTopicIds.Any())
            {
                foreach (var topicId in model.SelectedTopicIds)
                {
                    product.ProductTopics.Add(new ProductTopic { TopicId = topicId });
                }
            }

            // Tự động thêm user hiện tại làm Founder
            product.ProductMakers.Add(new ProductMaker
            {
                UserId = userId,
                MakerRole = "Founder"
            });

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bạn đã submit sản phẩm thành công! Sản phẩm đang trong trạng thái chờ duyệt (Pending).";
            
            // Redirect sang Profile thay vì Home vì trang Home chỉ hiện Approved (Pending không lên feed)
            return RedirectToAction("Profile", "User", new { username = User.Identity!.Name });
        }

        private async Task PopulateTopicsAsync(SubmitProductViewModel vm)
        {
            var topics = await _context.Topics.OrderBy(t => t.TopicName).ToListAsync();
            vm.AvailableTopics = topics.Select(t => new SelectListItem
            {
                Value = t.TopicId.ToString(),
                Text = t.TopicName
            }).ToList();
        }
    }
}
