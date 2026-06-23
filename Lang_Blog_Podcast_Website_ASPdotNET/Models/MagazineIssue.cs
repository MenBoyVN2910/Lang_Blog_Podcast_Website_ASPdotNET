using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class MagazineIssue
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Số phát hành không được để trống")]
        [StringLength(10, ErrorMessage = "Số phát hành tối đa 10 ký tự")]
        [Display(Name = "Số phát hành (Ví dụ: V.01)")]
        public string IssueNumber { get; set; }

        [Required(ErrorMessage = "Tên ấn phẩm không được để trống")]
        [StringLength(40, ErrorMessage = "Tên ấn phẩm tối đa 40 ký tự")]
        [Display(Name = "Tên ấn phẩm")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        [Display(Name = "Mô tả / Lời tựa")]
        public string Description { get; set; }

        [Display(Name = "Ảnh bìa")]
        public string CoverImagePath { get; set; }

        [Required(ErrorMessage = "Kỳ xuất bản không được để trống")]
        [StringLength(30, ErrorMessage = "Kỳ/Mùa tối đa 30 ký tự")]
        [Display(Name = "Kỳ / Mùa (Ví dụ: SUMMER 2026)")]
        public string Season { get; set; }

        [Display(Name = "Trạng thái")]
        public StoryStatus Status { get; set; } = StoryStatus.Pending; // Sử dụng lại Enum Pending (Draft), Approved (Published)

        [Display(Name = "Ngày xuất bản")]
        public DateTime? PublishDate { get; set; }

        [Display(Name = "Người biên tập (Admin)")]
        public string? EditorId { get; set; }
        public ApplicationUser? Editor { get; set; }

        // Liên kết 1-n: 1 Ấn Phẩm chứa nhiều Bài Viết
        public ICollection<MagazineArticle> Articles { get; set; } = new List<MagazineArticle>();
    }
}

