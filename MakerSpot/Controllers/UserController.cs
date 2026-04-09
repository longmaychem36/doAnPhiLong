using System.Security.Claims;
using MakerSpot.Models;
using MakerSpot.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Controllers
{
    public class UserController : Controller
    {
        private readonly MakerSpotContext _context;
        private readonly Services.IPhotoService _photoService;

        public UserController(MakerSpotContext context, Services.IPhotoService photoService)
        {
            _context = context;
            _photoService = photoService;
        }

        [Route("@@{username}")]
        public async Task<IActionResult> Profile(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return NotFound();

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !user.IsActive) return NotFound();

            var isOwnProfile = User.Identity!.IsAuthenticated && User.Identity.Name == username;
            
            var submittedQuery = _context.Products
                .Include(p => p.ProductTopics).ThenInclude(pt => pt.Topic)
                .AsNoTracking()
                .Where(p => p.UserId == user.UserId);

            if (!isOwnProfile)
            {
                submittedQuery = submittedQuery.Where(p => p.Status == "Approved");
            }

            var submittedProducts = await submittedQuery
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var upvotedProducts = await _context.ProductUpvotes
                .Include(up => up.Product)
                    .ThenInclude(p => p.ProductTopics).ThenInclude(pt => pt.Topic)
                .AsNoTracking()
                .Where(up => up.UserId == user.UserId && up.Product.Status == "Approved")
                .OrderByDescending(up => up.CreatedAt)
                .Select(up => up.Product)
                .ToListAsync();

            // Phase 4: Follower counts
            var followerCount = await _context.Followers.CountAsync(f => f.FollowingId == user.UserId);
            var followingCount = await _context.Followers.CountAsync(f => f.FollowerId == user.UserId);

            bool isFollowing = false;
            if (User.Identity.IsAuthenticated && !isOwnProfile)
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                isFollowing = await _context.Followers.AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == user.UserId);
            }

            // Phase 4: Public collections (or all if own profile)
            var collectionsQuery = _context.Collections
                .Include(c => c.CollectionItems)
                .AsNoTracking()
                .Where(c => c.UserId == user.UserId);

            if (!isOwnProfile)
            {
                collectionsQuery = collectionsQuery.Where(c => c.IsPublic);
            }

            var collections = await collectionsQuery.OrderByDescending(c => c.CreatedAt).ToListAsync();

            // Phase 8: Stats Calculation
            var totalUpvotesReceived = await _context.Products
                .Where(p => p.UserId == user.UserId && p.Status == "Approved")
                .SumAsync(p => p.UpvoteCount);
                
            var totalCommentsReceived = await _context.Products
                .Where(p => p.UserId == user.UserId && p.Status == "Approved")
                .SumAsync(p => p.CommentCount);
                
            var topProduct = await _context.Products
                .Include(p => p.ProductTopics).ThenInclude(pt => pt.Topic)
                .AsNoTracking()
                .Where(p => p.UserId == user.UserId && p.Status == "Approved")
                .OrderByDescending(p => p.UpvoteCount)
                .ThenByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            var vm = new UserProfileViewModel
            {
                User = user,
                SubmittedProducts = submittedProducts,
                UpvotedProducts = upvotedProducts,
                FollowerCount = followerCount,
                FollowingCount = followingCount,
                IsFollowing = isFollowing,
                Collections = collections,
                TotalUpvotesReceived = totalUpvotesReceived,
                TotalCommentsReceived = totalCommentsReceived,
                TopProduct = topProduct
            };

            return View(vm);
        }

        // GET: /User/EditProfile — Form chỉnh sửa hồ sơ (chỉ cho chính mình)
        [HttpGet]
        [Authorize]
        [Route("EditProfile")]
        public async Task<IActionResult> EditProfile()
        {
            var username = User.Identity!.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            var vm = new EditProfileViewModel
            {
                FullName = user.FullName,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                WebsiteUrl = user.WebsiteUrl,
                TwitterUrl = user.TwitterUrl,
                LinkedinUrl = user.LinkedinUrl
            };
            return View(vm);
        }

        // POST: /User/EditProfile — Lưu thay đổi hồ sơ
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [Route("EditProfile")]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var username = User.Identity!.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            // Xử lý upload ảnh Avatar lên Cloudinary
            string imageUrl = user.AvatarUrl ?? "/images/default-avatar.png";
            if (model.AvatarFile != null && model.AvatarFile.Length > 0)
            {
                // Validate size < 2MB and extension
                if (model.AvatarFile.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("AvatarFile", "Dung lượng ảnh không được vượt quá 2MB.");
                    return View(model);
                }
                var ext = Path.GetExtension(model.AvatarFile.FileName).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
                {
                    ModelState.AddModelError("AvatarFile", "Chỉ hỗ trợ file ảnh .jpg, .jpeg, .png, .webp.");
                    return View(model);
                }

                var uploadResult = await _photoService.AddPhotoAsync(model.AvatarFile);
                if (uploadResult.Error != null)
                {
                    ModelState.AddModelError("AvatarFile", "Có lỗi xảy ra khi upload ảnh lên Cloudinary. Chi tiết: " + uploadResult.Error.Message);
                    return View(model);
                }
                
                // (Tùy chọn) Xóa ảnh cũ trên Cloudinary nếu có public_id
                
                imageUrl = uploadResult.SecureUrl.ToString();
            }
            else if (!string.IsNullOrEmpty(model.AvatarUrl))
            {
                // Nếu người dùng dán URL tĩnh thay vì file upload
                imageUrl = model.AvatarUrl;
            }

            user.FullName = model.FullName;
            user.Bio = model.Bio;
            user.AvatarUrl = imageUrl;
            user.WebsiteUrl = model.WebsiteUrl;
            user.TwitterUrl = model.TwitterUrl;
            user.LinkedinUrl = model.LinkedinUrl;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Profile", new { username });
        }
    }
}
