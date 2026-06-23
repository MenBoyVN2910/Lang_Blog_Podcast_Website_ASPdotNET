using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace Lang_Blog_Podcast_Website_ASPdotNET.Data;

public class ApplicationUser : IdentityUser
{

    [Required]
    public string? FullName { get; set; }

    // THÊM DÒNG NÀY: Lưu ảnh dưới dạng mảng byte
    public byte[]? ProfilePicture { get; set; }

    // Thông tin trang cá nhân
    [MaxLength(500)]
    public string? Bio { get; set; } // Giới thiệu ngắn
    [MaxLength(100)]
    public string? Slogan { get; set; } // Câu slogan
    public string? CoverImagePath { get; set; } // Ảnh bìa lưu file wwwroot
}
