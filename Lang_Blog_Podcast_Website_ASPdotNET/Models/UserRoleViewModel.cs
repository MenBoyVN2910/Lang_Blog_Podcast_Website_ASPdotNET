namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } // Trạng thái: True nếu đang là Admin
    }
}