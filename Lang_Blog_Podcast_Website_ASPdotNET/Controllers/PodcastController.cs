using Microsoft.AspNetCore.Mvc;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng liên quan đến việc phát và danh sách Podcast.
    /// </summary>
    public class PodcastController : Controller
    {
        /// <summary>
        /// GET: /Podcast/
        /// Hiển thị giao diện danh sách các số phát sóng Podcast.
        /// </summary>
        public IActionResult Index() => View();
    }
}
