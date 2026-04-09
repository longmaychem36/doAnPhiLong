using MakerSpot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MakerSpot.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly MakerSpotContext _context;

        public ChatController(MakerSpotContext context)
        {
            _context = context;
        }

        // GET: /Chat -> Mở màn hình danh sách Chat
        public async Task<IActionResult> Index(int? convId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var currentUserId)) return Unauthorized();

            // Lấy danh sách các cuộc trò chuyện của user
            var conversationsQuery = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt))
                .Where(c => c.User1Id == currentUserId || c.User2Id == currentUserId)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            // Nếu muốn giới hạn 1 message cuối cùng để tiết kiệm bộ nhớ:
            foreach(var conv in conversationsQuery)
            {
                var lastMsg = conv.Messages.FirstOrDefault();
                conv.Messages.Clear();
                if (lastMsg != null) conv.Messages.Add(lastMsg);
            }

            var conversations = conversationsQuery;

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.ActiveConvId = convId;

            return View(conversations);
        }

        // POST: /Chat/Start/{username} -> Bấm nút Chat từ Profile
        [HttpPost]
        public async Task<IActionResult> Start(string username)
        {
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdStr, out var currentUserId)) return Unauthorized();

            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (targetUser == null) return NotFound();

            if (currentUserId == targetUser.UserId) return RedirectToAction("Index"); // Tự chat với mình thì về Index

            // Tìm conversation đã tồn tại chưa (User1Id < User2Id)
            int minId = Math.Min(currentUserId, targetUser.UserId);
            int maxId = Math.Max(currentUserId, targetUser.UserId);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.User1Id == minId && c.User2Id == maxId);

            if (conversation == null)
            {
                // Chưa từng chat => Tạo mới
                conversation = new Conversation
                {
                    User1Id = minId,
                    User2Id = maxId,
                    CreatedAt = DateTime.Now,
                    LastMessageAt = DateTime.Now
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { convId = conversation.ConversationId });
        }

        // API GET: /Chat/GetMessages/{id} -> SignalR client gọi để load tin nhắn cũ
        [HttpGet]
        [Route("Chat/GetMessages/{id}")]
        public async Task<IActionResult> GetMessages(int id)
        {
            int convId = id;
            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(currentUserIdStr, out var currentUserId)) return Unauthorized();

            // Kiểm tra quyền
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.ConversationId == convId && (c.User1Id == currentUserId || c.User2Id == currentUserId));
            
            if (conversation == null) return Forbid();

            // Đánh dấu các tin nhắn là đã đọc
            var unreadMessages = await _context.Messages
                .Where(m => m.ConversationId == convId && m.SenderId != currentUserId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach(var m in unreadMessages) m.IsRead = true;
                await _context.SaveChangesAsync();
            }

            var messages = await _context.Messages
                .Include(m => m.SharedProduct)
                .Where(m => m.ConversationId == convId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new {
                    m.MessageId,
                    m.SenderId,
                    m.Content,
                    m.ImageUrl,
                    SharedProduct = m.SharedProduct == null ? null : new {
                        m.SharedProduct.ProductId,
                        m.SharedProduct.ProductName,
                        m.SharedProduct.LogoUrl,
                        m.SharedProduct.Tagline,
                        m.SharedProduct.Slug
                    },
                    CreatedAt = m.CreatedAt.ToString("HH:mm")
                })
                .ToListAsync();

            return Json(messages);
        }

        // POST: /Chat/UploadImage
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromServices] Services.IPhotoService photoService, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");
            
            if (file.Length > 2 * 1024 * 1024) return BadRequest("Ảnh tối đa 2MB");

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp") return BadRequest("Sai định dạng ảnh");

            var uploadResult = await photoService.AddPhotoAsync(file);
            if (uploadResult.Error != null) return StatusCode(500, uploadResult.Error.Message);

            return Json(new { imageUrl = uploadResult.SecureUrl.ToString() });
        }

        // POST: /Chat/ShareProduct
        [HttpPost]
        public async Task<IActionResult> ShareProduct(int productId, int conversationId, string? messageContent)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var currentUserId)) return Unauthorized();

            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.ConversationId == conversationId && (c.User1Id == currentUserId || c.User2Id == currentUserId));
            if (conversation == null) return Forbid();

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = currentUserId,
                Content = string.IsNullOrWhiteSpace(messageContent) ? "Tôi vừa chia sẻ một sản phẩm với bạn!" : messageContent,
                SharedProductId = productId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Messages.Add(message);
            conversation.LastMessageAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Lấy lại info product để trigger SignalR (thực tế hub sẽ gọi ở client, nhưng ta có thể báo cho client reload hoặc push data giả lập)
            return Ok(); // Client sau khi gọi API này sẽ load lại khung chat
        }
        // API GET: /Chat/GetMyConversations
        [HttpGet]
        public async Task<IActionResult> GetMyConversations()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var currentUserId)) return Unauthorized();

            var conversations = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Where(c => c.User1Id == currentUserId || c.User2Id == currentUserId)
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new {
                    c.ConversationId,
                    OtherUser = c.User1Id == currentUserId ? c.User2.FullName : c.User1.FullName,
                    AvatarUrl = c.User1Id == currentUserId ? c.User2.AvatarUrl : c.User1.AvatarUrl
                })
                .ToListAsync();

            return Json(conversations);
        }
    }
}
