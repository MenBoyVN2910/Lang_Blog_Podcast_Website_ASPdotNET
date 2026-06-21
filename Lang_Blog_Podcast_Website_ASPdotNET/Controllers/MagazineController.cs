using Microsoft.AspNetCore.Mvc;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng liên quan đến việc hiển thị Tạp chí (Magazine).
    /// </summary>
    public class MagazineController : Controller
    {
        /// <summary>
        /// GET: /Magazine/
        /// Hiển thị giao diện danh sách hoặc trang bìa Tạp chí.
        /// </summary>
        public IActionResult Index() => View();
    }
}
