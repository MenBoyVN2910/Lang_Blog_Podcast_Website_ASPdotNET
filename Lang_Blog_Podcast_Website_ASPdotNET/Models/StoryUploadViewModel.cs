using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class StoryUploadViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề câu chuyện.")]
        public string Title { get; set; }

 
        [Required(ErrorMessage = "Vui lòng nhập tên tác giả.")]
        public string Author { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; } 

        [Required(ErrorMessage = "Vui lòng viết lời tự sự của bạn.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn một bức ảnh bìa thật thơ cho câu chuyện của bạn.")]
        public IFormFile ImageFile { get; set; }
    }
}