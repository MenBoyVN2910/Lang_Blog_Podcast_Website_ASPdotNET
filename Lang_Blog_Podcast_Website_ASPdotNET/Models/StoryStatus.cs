namespace Lang_Blog_Podcast_Website_ASPdotNET.Models
{
    public enum StoryStatus
    {
        Pending,   // Chờ duyệt
        Approved,  // Đã duyệt (Xuất bản)
        Rejected,  // Từ chối duyệt
        EditPending // Bản chỉnh sửa đang chờ duyệt (Shadow Copy)
    }
}