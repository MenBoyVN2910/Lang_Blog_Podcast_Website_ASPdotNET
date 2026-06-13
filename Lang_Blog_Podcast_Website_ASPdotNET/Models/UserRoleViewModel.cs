namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; } // Trạng thái: True nếu đang là Admin
    }
}