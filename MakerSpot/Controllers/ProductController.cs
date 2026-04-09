using System.Security.Claims;
using MakerSpot.Models;
using MakerSpot.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Controllers
{
    /// <summary>
    /// Quản lý sản phẩm: Xem chi tiết, Upvote/Bỏ Upvote (atomic counter), Bình luận, Submit sản phẩm mới.
    /// </summary>
    public class ProductController : Controller
    {
        private readonly MakerSpotContext _context;
        private readonly Services.IPhotoService _photoService;

        public ProductController(MakerSpotContext context, Services.IPhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
        }

        // GET: /Product/Detail/5 — Xem chi tiết sản phẩm (chỉ Approved)
        public async Task<IActionResult> Detail(int id)
        {
            // Load sản phẩm kèm các quan hệ cần thiết (1 query duy nhất)
            var product = await _context.Products
                .Include(p => p.User)
                .Include(p => p.ProductMedia.OrderBy(m => m.DisplayOrder))
                .Include(p => p.ProductTopics).ThenInclude(pt => pt.Topic)
                .Include(p => p.ProductMakers).ThenInclude(pm => pm.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null || product.Status != "Approved")
            {
                return NotFound();
            }

            // Load comments riêng để tránh duplicate Include filter 
            product.Comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Replies.Where(r => !r.IsDeleted).OrderBy(r => r.CreatedAt))
                    .ThenInclude(r => r.User)
                .AsNoTracking()
                .Where(c => c.ProductId == id && !c.IsDeleted && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Take(50) // Giới hạn 50 comments gốc
                .ToListAsync();

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

        // POST: /Product/Upvote — Toggle Upvote. Dùng ExecuteUpdateAsync để cập nhật counter atomíc
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

        // POST: /Product/PostComment — Thêm comment/reply và tạo notification cho chủ sản phẩm
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

        // GET: /Product/Submit — Form gửi sản phẩm mới (chỉ cho đăng nhập)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Submit()
        {
            var vm = new SubmitProductViewModel();
            await PopulateTopicsAsync(vm);
            vm.LaunchDate = DateTime.Today;
            return View(vm);
        }

        // POST: /Product/Submit — Lưu sản phẩm với status Pending, gán Topics và Maker
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

            // Xử lý upload ảnh Logo lên Cloudinary
            string imageUrl = "/images/default-logo.png"; // Ảnh mặc định
            if (model.LogoFile != null && model.LogoFile.Length > 0)
            {
                // Validate size < 2MB and extension
                if (model.LogoFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("LogoFile", "Dung lượng ảnh không được vượt quá 2MB.");
                    await PopulateTopicsAsync(model);
                    return View(model);
                }
                var ext = Path.GetExtension(model.LogoFile.FileName).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
                {
                    ModelState.AddModelError("LogoFile", "Chỉ hỗ trợ file ảnh .jpg, .jpeg, .png, .webp.");
                    await PopulateTopicsAsync(model);
                    return View(model);
                }

                var uploadResult = await _photoService.AddPhotoAsync(model.LogoFile);
                if (uploadResult.Error != null)
                {
                    ModelState.AddModelError("LogoFile", "Có lỗi xảy ra khi upload ảnh lên Cloudinary.");
                    await PopulateTopicsAsync(model);
                    return View(model);
                }
                imageUrl = uploadResult.SecureUrl.ToString();
            }
            else if (!string.IsNullOrEmpty(model.LogoUrl))
            {
                // Vẫn hỗ trợ URL nếu người dùng nhập tay (tương lai có thể bỏ)
                imageUrl = model.LogoUrl;
            }

            var product = new Product
            {
                UserId = userId,
                ProductName = model.ProductName,
                Slug = model.Slug,
                Tagline = model.Tagline,
                Description = model.Description,
                LogoUrl = imageUrl,
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

            // Upload ảnh sản phẩm (tối đa 10 ảnh) vào ProductMedia
            if (model.ProductImages != null && model.ProductImages.Any())
            {
                var validImages = model.ProductImages.Take(10).ToList();
                int order = 1;
                foreach (var imgFile in validImages)
                {
                    if (imgFile.Length == 0) continue;
                    if (imgFile.Length > 5 * 1024 * 1024) continue; // Skip files > 5MB
                    var imgExt = Path.GetExtension(imgFile.FileName).ToLower();
                    if (imgExt != ".jpg" && imgExt != ".jpeg" && imgExt != ".png" && imgExt != ".webp" && imgExt != ".gif") continue;

                    var imgResult = await _photoService.AddPhotoAsync(imgFile);
                    if (imgResult.Error != null) continue;

                    _context.ProductMedia.Add(new ProductMedia
                    {
                        ProductId = product.ProductId,
                        MediaType = "Image",
                        MediaUrl = imgResult.SecureUrl.ToString(),
                        DisplayOrder = order++
                    });
                }
                await _context.SaveChangesAsync();
            }

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

        // POST: /Product/EditComment — User sửa comment của chính mình (IsEdited = true)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int commentId, string content, int productId)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction("Detail", new { id = productId });

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
            if (comment == null) return NotFound();

            // Chỉ chủ comment hoặc Admin/Mod mới được sửa
            bool isOwner = comment.UserId == userId;
            bool isAdminOrMod = User.IsInRole("Admin") || User.IsInRole("Moderator");
            if (!isOwner && !isAdminOrMod) return Forbid();

            comment.Content = content.Trim();
            comment.IsEdited = true;
            comment.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = productId });
        }

        // POST: /Product/DeleteComment — Soft delete comment (IsDeleted = true) + giảm counter
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int productId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
            if (comment == null) return NotFound();

            bool isOwner = comment.UserId == userId;
            bool isAdminOrMod = User.IsInRole("Admin") || User.IsInRole("Moderator");
            if (!isOwner && !isAdminOrMod) return Forbid();

            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.Now;

            // Giảm counter trên Product
            await _context.Products.Where(p => p.ProductId == productId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CommentCount, p => p.CommentCount - 1));

            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = productId });
        }

        // POST: /Product/VoteComment — Toggle vote cho comment (giống upvote product)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VoteComment(int commentId, int productId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var existingVote = await _context.CommentVotes
                .FirstOrDefaultAsync(v => v.CommentId == commentId && v.UserId == userId);

            if (existingVote != null)
            {
                _context.CommentVotes.Remove(existingVote);
            }
            else
            {
                _context.CommentVotes.Add(new CommentVote
                {
                    CommentId = commentId,
                    UserId = userId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Detail", new { id = productId });
        }
    }
}
