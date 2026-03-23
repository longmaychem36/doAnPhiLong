using System.Security.Claims;
using MakerSpot.Models;
using MakerSpot.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Controllers
{
    public class AuthController : Controller
    {
        private readonly MakerSpotContext _context;

        public AuthController(MakerSpotContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(model);

            // In real app, hash password and compare. Here for simplicity, exact match or simple hash
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.PasswordHash == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName)
            };

            foreach (var ur in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, ur.Role.RoleName));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(model);

            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng.");
                return View(model);
            }

            var memberRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Member");
            if (memberRole == null)
            {
                // Safety fallback if database seeded incorrectly
                memberRole = new Role { RoleName = "Member" };
                _context.Roles.Add(memberRole);
            }

            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                PasswordHash = model.Password, // Simple plain mapping for Phase 1 demo, per requirements
                IsActive = true,
                IsVerified = false
            };

            newUser.UserRoles.Add(new UserRole { Role = memberRole });

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Auto sign in after register
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.UserId.ToString()),
                new Claim(ClaimTypes.Name, newUser.Username),
                new Claim("FullName", newUser.FullName),
                new Claim(ClaimTypes.Role, "Member")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
