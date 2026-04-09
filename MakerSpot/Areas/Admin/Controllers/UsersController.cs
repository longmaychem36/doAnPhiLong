using MakerSpot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly MakerSpotContext _context;

        public UsersController(MakerSpotContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // POST: Admin/Users/ToggleLock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.IsActive == false && user.Username == "admin") 
            {
                // Safety: Cannot lock admin
                TempData["ErrorMessage"] = "Không thể thao tác trên tài khoản này.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = user.IsActive ? $"Đã mở khóa tài khoản {user.Username}." : $"Đã khóa tài khoản {user.Username}.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Users/CreateModerator
        public async Task<IActionResult> CreateModerator()
        {
            var topics = await _context.Topics.OrderBy(t => t.TopicName).ToListAsync();
            var vm = new MakerSpot.ViewModels.Admin.CreateModeratorViewModel
            {
                AvailableTopics = topics.Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = t.TopicId.ToString(),
                    Text = t.TopicName
                }).ToList()
            };
            return View(vm);
        }

        // POST: Admin/Users/CreateModerator
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModerator(MakerSpot.ViewModels.Admin.CreateModeratorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var topics = await _context.Topics.OrderBy(t => t.TopicName).ToListAsync();
                model.AvailableTopics = topics.Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = t.TopicId.ToString(),
                    Text = t.TopicName
                }).ToList();
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                var topics = await _context.Topics.OrderBy(t => t.TopicName).ToListAsync();
                model.AvailableTopics = topics.Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem { Value = t.TopicId.ToString(), Text = t.TopicName }).ToList();
                return View(model);
            }

            var modRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Moderator");
            if (modRole == null) 
            {
                modRole = new Role { RoleName = "Moderator" };
                _context.Roles.Add(modRole);
            }

            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                IsActive = true,
                IsVerified = true,
                AvatarUrl = "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(model.FullName) + "&background=d97706&color=fff&size=200"
            };

            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
            newUser.PasswordHash = hasher.HashPassword(newUser, model.Password);

            newUser.UserRoles.Add(new UserRole { Role = modRole });
            
            // Assign topics
            foreach(var topicId in model.SelectedTopicIds)
            {
                _context.ModeratorTopics.Add(new ModeratorTopic { User = newUser, TopicId = topicId });
            }

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Tạo Moderator {newUser.Username} thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
