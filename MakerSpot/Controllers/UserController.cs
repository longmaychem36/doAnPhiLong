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

        [Route("@@{username}")] // Friendly URL like /@namnguyen or just /User/Profile?username=namnguyen based on routing
        public async Task<IActionResult> Profile(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !user.IsActive) return NotFound();

            // Submitted Products (Includes Pending if viewing own profile, otherwise only Approved)
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

            // Upvoted Products (Only Approved)
            var upvotedProducts = await _context.ProductUpvotes
                .Include(up => up.Product)
                    .ThenInclude(p => p.ProductTopics).ThenInclude(pt => pt.Topic)
                .Where(up => up.UserId == user.UserId && up.Product.Status == "Approved")
                .OrderByDescending(up => up.CreatedAt)
                .Select(up => up.Product)
                .ToListAsync();

            var vm = new UserProfileViewModel
            {
                User = user,
                SubmittedProducts = submittedProducts,
                UpvotedProducts = upvotedProducts
            };

            return View(vm);
        }
    }
}
