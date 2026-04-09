using MakerSpot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly MakerSpotContext _context;

        public AuditLogsController(MakerSpotContext context)
        {
            _context = context;
        }

        // GET: Admin/AuditLogs — Nhật ký hệ thống (Admin only)
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 30;
            var query = _context.AuditLogs
                .Include(a => a.User)
                .AsNoTracking()
                .OrderByDescending(a => a.CreatedAt);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;

            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            return View(logs);
        }
    }
}
