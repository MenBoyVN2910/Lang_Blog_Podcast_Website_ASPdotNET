using Lang_Blog_Podcast_Website_ASPdotNET.Data;
using System.ComponentModel.DataAnnotations;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class PodCast
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [Display(Name = "Tiêu đề câu chuyện")]
        public string Title { get; set; }
        [Display(Name = "Tác giả")]
        public string Author { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        [Required]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }
        public int? EpisodeNumber { get; set; }
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        [Display(Name = "Ảnh bìa")]
        public string ImagePath { get; set; } 
        public StoryStatus Status { get; set; } = StoryStatus.Pending;
        [Display(Name = "Ngày gửi")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Display(Name = "Ngày xuất bản")]
        public DateTime? PublishDate { get; set; }
        [Display(Name = "File âm thanh")]
        public string AudioPath { get; set; }
        public int ViewCount { get; set; } = 0; 
        [Display(Name = "Thời lượng")]
        public string? Duration { get; set; }

        [Display(Name = "Lý do từ chối")]
        public string? RejectionReason { get; set; }
    }
}
