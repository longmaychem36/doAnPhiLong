using System.Security.Claims;
using MakerSpot.Models;
using MakerSpot.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Controllers
{
    public class UserController : Controller
    {
        private readonly MakerSpotContext _context;

        public UserController(MakerSpotContext context)
        {
            _context = context;
        }

        [Route("@@{username}")]
        public async Task<IActionResult> Profile(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !user.IsActive) return NotFound();

            var isOwnProfile = User.Identity!.IsAuthenticated && User.Identity.Name == username;
            
            var submittedQuery = _context.Products
                .Include(p => p.ProductTopics).ThenInclude(pt => pt.Topic)
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
                .Where(c => c.UserId == user.UserId);

            if (!isOwnProfile)
            {
                collectionsQuery = collectionsQuery.Where(c => c.IsPublic);
            }

            var collections = await collectionsQuery.OrderByDescending(c => c.CreatedAt).ToListAsync();

            var vm = new UserProfileViewModel
            {
                User = user,
                SubmittedProducts = submittedProducts,
                UpvotedProducts = upvotedProducts,
                FollowerCount = followerCount,
                FollowingCount = followingCount,
                IsFollowing = isFollowing,
                Collections = collections
            };

            return View(vm);
        }
    }
}
