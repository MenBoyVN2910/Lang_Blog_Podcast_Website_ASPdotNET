using System;
using System.Collections.Generic;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class ProfileViewModel
    {
        // Thông tin chung
        public ApplicationUser User { get; set; } = null!;
        public bool IsOwner { get; set; }           // True nếu đang xem profile của chính mình
        public string RoleBadge { get; set; } = string.Empty;       // VD: "Admin ✦", "Nhà Sáng Tạo ✎", "Thành Viên"
        
        // Thống kê
        public int TotalStories { get; set; }
        public int TotalPodcasts { get; set; }
        public int TotalViews { get; set; }
        public int TotalFollowers { get; set; }     // Placeholder hiện tại
        
        // Nội dung hiển thị ở các Tab
        public List<Story> Stories { get; set; } = new List<Story>();
        public List<PodCast> Podcasts { get; set; } = new List<PodCast>();
        
        // Mục yêu thích (Tab 3 - chỉ chủ sở hữu xem được)
        public List<Story> FavoriteStories { get; set; } = new List<Story>();
        public List<PodCast> FavoritePodcasts { get; set; } = new List<PodCast>();
        
        // Gamification: Thành tựu cá nhân
        public List<AchievementItem> Achievements { get; set; } = new List<AchievementItem>();
    }

    public class AchievementItem
    {
        public string IconClass { get; set; } = string.Empty; // Lớp icon FontAwesome, vd: fa-solid fa-microphone
        public string IconColor { get; set; } = string.Empty; // Màu sắc, vd: #f57c00
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; }
    }
}
