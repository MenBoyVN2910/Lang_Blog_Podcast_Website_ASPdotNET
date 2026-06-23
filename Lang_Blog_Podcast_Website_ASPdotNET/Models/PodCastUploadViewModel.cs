using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class PodCastUploadViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề podcast.")]
        [StringLength(40, ErrorMessage = "Tiêu đề không được vượt quá 40 ký tự.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên tác giả.")]
        [StringLength(30, ErrorMessage = "Tên tác giả không được vượt quá 30 ký tự.")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Vui lòng viết mô tả cho podcast.")]
        public string Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng nhập số tập hợp lệ (từ 1 trở lên).")]
        public int? EpisodeNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ảnh bìa cho podcast.")]
        public IFormFile ImageFile { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn file âm thanh cho podcast.")]
        public IFormFile AudioFile { get; set; }

        public string? Duration { get; set; }
    }
}
