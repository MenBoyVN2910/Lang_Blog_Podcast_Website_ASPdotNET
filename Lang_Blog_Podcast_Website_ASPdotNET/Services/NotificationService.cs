using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Tạo một thông báo cho 1 user
        public async Task CreateAsync(string recipientUserId, string title, string message, string type, string? linkUrl = null)
        {
            var notification = new Notification
            {
                RecipientUserId = recipientUserId,
                Title = title,
                Message = message,
                Type = type,
                LinkUrl = linkUrl
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // Tạo thông báo cho tất cả Admin
        public async Task NotifyAllAdminsAsync(string title, string message, string type, string? linkUrl = null)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var notifications = admins.Select(admin => new Notification
            {
                RecipientUserId = admin.Id,
                Title = title,
                Message = message,
                Type = type,
                LinkUrl = linkUrl
            }).ToList();

            if (notifications.Any())
            {
                _context.Notifications.AddRange(notifications);
                await _context.SaveChangesAsync();
            }
        }

        // Đếm số thông báo chưa đọc của user
        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .CountAsync();
        }

        // Lấy N thông báo mới nhất
        public async Task<List<Notification>> GetRecentAsync(string userId, int count = 5)
        {
            return await _context.Notifications
                .Where(n => n.RecipientUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
