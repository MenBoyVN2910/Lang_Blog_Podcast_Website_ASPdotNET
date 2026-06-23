using System;
using System.ComponentModel.DataAnnotations;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        // Người nhận thông báo
        [Required]
        public string RecipientUserId { get; set; }
        public ApplicationUser? RecipientUser { get; set; }

        // Nội dung thông báo
        [Required]
        public string Title { get; set; }       // VD: "Bài viết đã được duyệt"
        
        public string? Message { get; set; }    // Nội dung chi tiết nếu có
        
        // Phân loại thông báo: "Approved", "Rejected", "NewSubmission", "Welcome", v.v.
        [Required]
        public string Type { get; set; }        
        
        // Đường dẫn liên kết khi bấm vào thông báo (nếu có)
        public string? LinkUrl { get; set; }

        // Trạng thái đã đọc hay chưa
        public bool IsRead { get; set; } = false;
        
        // Thời gian tạo thông báo
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
