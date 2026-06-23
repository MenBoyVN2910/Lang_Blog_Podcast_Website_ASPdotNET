using System;
using System.ComponentModel.DataAnnotations;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class PostRevision
    {
        [Key]
        public int Id { get; set; }

        // Loại nội dung: "Story" hoặc "Podcast"
        [Required]
        public string ContentType { get; set; }

        // ID của bài gốc (Story.Id hoặc PodCast.Id)
        public int OriginalPostId { get; set; }

        // Dữ liệu chỉnh sửa
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        public string Title { get; set; }

        public string? Content { get; set; }       // Cho Story
        public string? Description { get; set; }   // Cho Podcast
        public string? ImagePath { get; set; }
        public string? AudioPath { get; set; }     // Cho Podcast
        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        // Trạng thái duyệt bản sửa
        public StoryStatus Status { get; set; } = StoryStatus.Pending;
        public string? RejectionReason { get; set; }

        // Metadata
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
