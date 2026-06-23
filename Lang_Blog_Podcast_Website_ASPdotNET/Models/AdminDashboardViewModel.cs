using System.Collections.Generic;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class AdminDashboardViewModel
    {
        // 1. Danh sách dữ liệu hiển thị ở các Tab
        public List<Story> PendingStories { get; set; } = new();
        public List<Story> ApprovedStories { get; set; } = new(); // 💡 Bổ sung để hiện ở Tab "Bài viết tạp chí"
        
        public List<PodCast> PendingPodCasts { get; set; } = new();
        public List<PodCast> ApprovedPodCasts { get; set; } = new();

        public List<MagazineIssue> MagazineIssues { get; set; } = new();

        public List<Category> Categories { get; set; } = new(); // 💡 Để hiện ở Tab "Quản lý danh mục"

        // 2. Các thuộc tính thống kê hiển thị ở các Card đầu trang
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int PendingPodCastsCount { get; set; }
        public int MagazineIssuesCount { get; set; }
        public int TotalCategoriesCount { get; set; }
        public int TotalPublishedCount { get; set; } // Tổng số bài Story + Podcast đã xuất bản

        // 3. Danh sách người dùng để phân quyền
        public List<UserRoleViewModel> Users { get; set; } = new();
    }
}