using System.Collections.Generic;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class UserDashboardViewModel
    {
        // Tab "Đăng Bài" — Hiển thị trạng thái các bài vừa gửi gần đây
        public List<Story> RecentSubmittedStories { get; set; } = new();
        public List<PodCast> RecentSubmittedPodcasts { get; set; } = new();

        // Tab "Câu Chuyện Của Tôi"
        public List<Story> MyStories { get; set; } = new();

        // Tab "Podcast Của Tôi"
        public List<PodCast> MyPodcasts { get; set; } = new();

        // Các bản chỉnh sửa đang chờ duyệt
        public List<PostRevision> PendingRevisions { get; set; } = new();

        // Thống kê
        public int TotalStories { get; set; }
        public int TotalPodcasts { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
    }
}
