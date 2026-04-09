using System.Security.Claims;
using MakerSpot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MakerSpot.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class ReportsController : Controller
    {
        private readonly MakerSpotContext _context;

        public ReportsController(MakerSpotContext context)
        {
            _context = context;
        }

        // GET: Admin/Reports — Danh sách báo cáo vi phạm
        public async Task<IActionResult> Index(string status = "Pending")
        {
            var query = _context.Reports
                .Include(r => r.ReporterUser)
                .Include(r => r.Reviewer)
                .AsNoTracking()
                .OrderByDescending(r => r.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(r => r.Status == status);
            }

            var reports = await query.Take(100).ToListAsync();

            ViewData["CurrentStatus"] = status;
            ViewData["PendingCount"] = await _context.Reports.CountAsync(r => r.Status == "Pending");
            return View(reports);
        }

        // POST: Admin/Reports/Review — Admin xử lý report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int reportId, string status)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            report.Status = status;
            report.ReviewedBy = userId;
            report.ReviewedAt = DateTime.Now;

            // Ghi AuditLog
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                ActionName = $"ReviewReport_{status}",
                TableName = "Reports",
                RecordId = reportId,
                NewData = $"Report #{reportId} → {status}"
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã cập nhật báo cáo #{reportId} thành {status}.";
            return RedirectToAction("Index");
        }
    }
}
