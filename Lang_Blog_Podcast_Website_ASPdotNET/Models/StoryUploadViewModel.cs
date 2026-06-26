using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class StoryUploadViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề câu chuyện.")]
        [StringLength(50, ErrorMessage = "Tiêu đề không được vượt quá 50 ký tự.")]
        public string Title { get; set; }

 
        [StringLength(10, ErrorMessage = "Số phát hành không được vượt quá 10 ký tự.")]
        public string? IssueNumber { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; } 

        [Required(ErrorMessage = "Vui lòng viết lời tự sự của bạn.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một bức ảnh bìa thật thơ cho câu chuyện của bạn.")]
        public IFormFile ImageFile { get; set; }
    }
}