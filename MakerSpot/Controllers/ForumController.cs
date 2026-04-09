using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MakerSpot.Models;
using System.Security.Claims;

namespace MakerSpot.Controllers
{
    [Authorize]
    public class ForumController : Controller
    {
        private readonly MakerSpotContext _context;

        public ForumController(MakerSpotContext context)
        {
            _context = context;
        }

        // GET: /Forum — Danh sách bài viết (ai cũng xem được)
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? tag, string? search, int page = 1)
        {
            const int pageSize = 15;

            var query = _context.ForumPosts
                .Include(fp => fp.User)
                .Include(fp => fp.Replies)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tag))
                query = query.Where(fp => fp.Tag == tag);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(fp => fp.Title.Contains(search) || fp.Content.Contains(search));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;

            var posts = await query
                .OrderByDescending(fp => fp.IsPinned)
                .ThenByDescending(fp => fp.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Title"] = "Diễn đàn thảo luận";
            ViewData["SelectedTag"] = tag;
            ViewData["SearchQuery"] = search;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;

            ViewBag.TotalPosts = await _context.ForumPosts.CountAsync();
            ViewBag.TotalReplies = await _context.ForumReplies.CountAsync(r => !r.IsDeleted);

            return View(posts);
        }

        // GET: /Forum/Detail/5 — Xem chi tiết bài viết + trả lời
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int id)
        {
            var post = await _context.ForumPosts
                .Include(fp => fp.User)
                .Include(fp => fp.Replies.Where(r => !r.IsDeleted).OrderBy(r => r.CreatedAt))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(fp => fp.ForumPostId == id);

            if (post == null) return NotFound();

            // Tăng lượt xem
            post.ViewCount++;
            await _context.SaveChangesAsync();

            ViewData["Title"] = post.Title;
            return View(post);
        }

        // GET: /Forum/Create — Form tạo bài viết mới
        public IActionResult Create()
        {
            ViewData["Title"] = "Tạo bài viết mới";
            return View();
        }

        // POST: /Forum/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string content, string? tag)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Tiêu đề và nội dung không được để trống.";
                return View();
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var post = new ForumPost
            {
                UserId = userId,
                Title = title.Trim(),
                Content = content.Trim(),
                Tag = string.IsNullOrWhiteSpace(tag) ? null : tag.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.ForumPosts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = post.ForumPostId });
        }

        // POST: /Forum/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int forumPostId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Nội dung trả lời không được để trống.";
                return RedirectToAction("Detail", new { id = forumPostId });
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var post = await _context.ForumPosts.FindAsync(forumPostId);
            if (post == null) return NotFound();
            if (post.IsLocked)
            {
                TempData["Error"] = "Bài viết đã bị khóa, không thể trả lời.";
                return RedirectToAction("Detail", new { id = forumPostId });
            }

            var reply = new ForumReply
            {
                ForumPostId = forumPostId,
                UserId = userId,
                Content = content.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.ForumReplies.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = forumPostId });
        }

        // POST: /Forum/Delete/5 — Xóa bài viết (chỉ chủ bài hoặc Admin/Mod)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null) return NotFound();

            bool isOwner = post.UserId == userId;
            bool isAdminOrMod = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (!isOwner && !isAdminOrMod) return Forbid();

            _context.ForumPosts.Remove(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa bài viết thành công.";
            return RedirectToAction("Index");
        }

        // POST: /Forum/DeleteReply/5 — Xóa trả lời 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReply(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var reply = await _context.ForumReplies.FindAsync(id);
            if (reply == null) return NotFound();

            bool isOwner = reply.UserId == userId;
            bool isAdminOrMod = User.IsInRole("Admin") || User.IsInRole("Moderator");

            if (!isOwner && !isAdminOrMod) return Forbid();

            reply.IsDeleted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", new { id = reply.ForumPostId });
        }
    }
}
