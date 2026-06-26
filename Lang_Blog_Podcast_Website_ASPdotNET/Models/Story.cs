using System;
using System.ComponentModel.DataAnnotations;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class Story
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [Display(Name = "Tiêu đề câu chuyện")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        // Liên kết với ApplicationUser (Người gửi bài)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Display(Name = "Số phát hành")]
        public string? IssueNumber { get; set; } // Ví dụ: Autumn Editorial Series

        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Display(Name = "Ảnh bìa")]
        public string ImagePath { get; set; } // Đường dẫn lưu file ảnh trong wwwroot

        public StoryStatus Status { get; set; } = StoryStatus.Pending;

        [Display(Name = "Ngày gửi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày xuất bản")]
        public DateTime? PublishDate { get; set; } // Sẽ được cập nhật khi admin duyệt

        public int ViewCount { get; set; } = 0; // Số lượt xem (như trong ảnh có 837 lượt xem)

        [Display(Name = "Lý do từ chối")]
        public string? RejectionReason { get; set; }
    }
}