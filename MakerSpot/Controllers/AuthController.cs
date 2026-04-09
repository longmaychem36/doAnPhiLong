using System.Security.Claims;
using MakerSpot.Models;
using MakerSpot.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Controllers
{
    /// <summary>
    /// Xác thực người dùng: Đăng nhập (Cookie Auth), Đăng ký (tự động gán Role Member), Đăng xuất.
    /// Sử dụng PasswordHasher<User> của ASP.NET Identity để băm mật khẩu an toàn.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly MakerSpotContext _context;

        public AuthController(MakerSpotContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login — Nếu đã đăng nhập thì chuyển về Home
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Auth/Login — Xác thực bằng Cookie Claims (PRG Pattern)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            var hasher = new PasswordHasher<User>();
            var passwordVerify = hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            // Fallback to plain text for old demo accounts
            if (passwordVerify == PasswordVerificationResult.Failed && user.PasswordHash != model.Password)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }
            
            // Auto upgrade hash if needed
            if (passwordVerify == PasswordVerificationResult.SuccessRehashNeeded || 
                user.PasswordHash == model.Password) // Plain text migration
            {
                user.PasswordHash = hasher.HashPassword(user, model.Password);
                await _context.SaveChangesAsync();
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

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Register — Form đăng ký
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Auth/Register — Tạo User mới với Role Member, tự động đăng nhập sau khi thành công
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid) return View(model);

            if (await _context.Users.AsNoTracking().AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            if (await _context.Users.AsNoTracking().AnyAsync(u => u.Email == model.Email))
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
                AvatarUrl = "https://ui-avatars.com/api/?name=" + Uri.EscapeDataString(model.FullName) + "&background=6366f1&color=fff&size=200",
                IsActive = true,
                IsVerified = false
            };
            
            var hasher = new PasswordHasher<User>();
            newUser.PasswordHash = hasher.HashPassword(newUser, model.Password);

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

        // POST: /Auth/Logout — Xóa cookie xác thực, chào tạm biệt người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
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
