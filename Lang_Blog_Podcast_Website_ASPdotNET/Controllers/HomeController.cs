using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    /// <summary>
    /// Controller điều hướng các trang chính của hệ thống như Trang chủ, Giới thiệu và các trang lỗi.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// GET: /
        /// Hiển thị Trang chủ của website.
        /// </summary>
        public IActionResult Index() => View();

        /// <summary>
        /// GET: /Home/About
        /// Hiển thị trang giới thiệu về "Lặng." (About Us).
        /// </summary>
        public IActionResult About() => View();

        /// <summary>
        /// GET: /Home/Error
        /// Xử lý và hiển thị thông tin lỗi hệ thống.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}
