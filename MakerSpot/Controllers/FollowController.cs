using System.Security.Claims;
using MakerSpot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Controllers
{
    public class FollowController : Controller
    {
        private readonly MakerSpotContext _context;

        public FollowController(MakerSpotContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int followingId, string returnUrl)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            // Prevent self-follow
            if (userId == followingId)
            {
                TempData["ErrorMessage"] = "Không thể tự follow chính mình!";
                return Redirect(returnUrl ?? "/");
            }

            var existing = await _context.Followers
                .FirstOrDefaultAsync(f => f.FollowerId == userId && f.FollowingId == followingId);

            if (existing != null)
            {
                _context.Followers.Remove(existing);
            }
            else
            {
                _context.Followers.Add(new Follower
                {
                    FollowerId = userId,
                    FollowingId = followingId
                });

                // Create notification for the followed user
                var currentUser = await _context.Users.FindAsync(userId);
                _context.Notifications.Add(new Notification
                {
                    UserId = followingId,
                    Type = "NewFollower",
                    ReferenceId = userId,
                    Message = $"{currentUser!.FullName} vừa theo dõi bạn."
                });
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
