using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        public string Name { get; set; }

        // Mối quan hệ: Một danh mục có thể chứa nhiều Story (và tương lai là Podcast, Post)
        public ICollection<Story> Stories { get; set; } = new List<Story>();

        // Sau này bạn làm thêm bảng nào thì chỉ cần thêm liên kết vào đây:
        public ICollection<PodCast> Podcasts { get; set; } = new List<PodCast>();
    }
}