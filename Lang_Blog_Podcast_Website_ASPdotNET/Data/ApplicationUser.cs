using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace Lang_Blog_Podcast_Website_ASPdotNET.Data;

public class ApplicationUser : IdentityUser
{

    [Required]
    public string? FullName { get; set; }

    // THÊM DÒNG NÀY: Lưu ảnh dưới dạng mảng byte
    public byte[]? ProfilePicture { get; set; }
}
