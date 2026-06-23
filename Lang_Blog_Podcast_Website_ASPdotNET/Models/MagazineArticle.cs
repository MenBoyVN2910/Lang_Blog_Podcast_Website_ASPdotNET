using System;
using System.ComponentModel.DataAnnotations;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class MagazineArticle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Thuộc ấn phẩm")]
        public int IssueId { get; set; }
        public MagazineIssue? Issue { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(40, ErrorMessage = "Tiêu đề tối đa 40 ký tự")]
        [Display(Name = "Tiêu đề bài viết")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [Display(Name = "Nội dung")]
        public string Content { get; set; }

        [Display(Name = "Ảnh minh họa")]
        public string ImagePath { get; set; }

        [Required(ErrorMessage = "Phải chọn kiểu lưới hiển thị")]
        [Display(Name = "Kiểu Layout (tall, wide, minimal, short, tall-offset)")]
        public string LayoutType { get; set; }

        public int ViewCount { get; set; } = 0;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
