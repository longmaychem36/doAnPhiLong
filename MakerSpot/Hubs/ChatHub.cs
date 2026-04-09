using MakerSpot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MakerSpot.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly MakerSpotContext _context;

        public ChatHub(MakerSpotContext context)
        {
            _context = context;
        }

        // Tham gia vào một nhóm (Conversation)
        public async Task JoinConversation(int conversationId)
        {
            var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return;

            // Kiểm tra xem User có thuộc conversation này không
            var isMember = await _context.Conversations.AnyAsync(c => 
                c.ConversationId == conversationId && 
                (c.User1Id == userId || c.User2Id == userId)
            );

            if (isMember)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
            }
        }

        // Rời nhóm
        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        // Gửi tin nhắn mới
        public async Task SendMessage(int conversationId, string content, string? imageUrl, int? sharedProductId)
        {
            var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var senderId)) return;

            var conversation = await _context.Conversations.FirstOrDefaultAsync(c => c.ConversationId == conversationId);
            if (conversation == null) return;

            if (conversation.User1Id != senderId && conversation.User2Id != senderId) return; // Không có quyền

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                ImageUrl = imageUrl,
                SharedProductId = sharedProductId,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);
            conversation.LastMessageAt = DateTime.Now;
            
            await _context.SaveChangesAsync();

            // Load thêm info product nếu có để push realtime
            Object? sharedProductInfo = null;
            if (sharedProductId.HasValue)
            {
                var pd = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == sharedProductId.Value);
                if(pd != null) {
                    sharedProductInfo = new {
                        productId = pd.ProductId,
                        productName = pd.ProductName,
                        logoUrl = pd.LogoUrl,
                        tagline = pd.Tagline,
                        slug = pd.Slug
                    };
                }
            }

            // Gửi thông báo tới các connection đang mở trong group
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", new 
            {
                messageId = message.MessageId,
                conversationId = message.ConversationId,
                senderId = message.SenderId,
                content = message.Content,
                imageUrl = message.ImageUrl,
                sharedProduct = sharedProductInfo,
                createdAt = message.CreatedAt.ToString("HH:mm")
            });
        }
    }
}
