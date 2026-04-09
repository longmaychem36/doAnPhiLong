using System.Security.Claims;
using MakerSpot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MakerSpot.Controllers
{
    /// <summary>
    /// Báo cáo vi phạm: User gửi report → Admin/Mod xử lý.
    /// </summary>
    [Authorize]
    public class ReportController : Controller
    {
        private readonly MakerSpotContext _context;

        public ReportController(MakerSpotContext context)
        {
            _context = context;
        }

        // POST: /Report/Submit — Gửi báo cáo vi phạm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string targetType, int targetId, string reason, string? description, string? returnUrl)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn lý do báo cáo.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }

            var report = new Report
            {
                ReporterUserId = userId,
                TargetType = targetType,
                TargetId = targetId,
                Reason = reason.Trim(),
                Description = description?.Trim()
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Báo cáo của bạn đã được gửi. Đội ngũ kiểm duyệt sẽ xem xét sớm nhất.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}
