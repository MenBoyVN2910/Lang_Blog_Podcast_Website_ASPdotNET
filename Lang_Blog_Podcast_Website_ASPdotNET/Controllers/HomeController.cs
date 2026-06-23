using Lang_Blog_Podcast_Website_ASPdotNET.Data;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    /// <summary>
    /// Controller điều hướng các trang chính của hệ thống như Trang chủ, Giới thiệu và các trang lỗi.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET: /
        /// Hiển thị Trang chủ của website.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Top 2 Podcast có lượt xem cao nhất
            ViewBag.TopPodcasts = await _db.PodCasts
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.Status == StoryStatus.Approved)
                .OrderByDescending(p => p.ViewCount)
                .Take(2)
                .ToListAsync();

            // Top 2 Bài Viết (Story) có lượt xem cao nhất
            ViewBag.TopStories = await _db.Stories
                .Include(s => s.Category)
                .Include(s => s.User)
                .Where(s => s.Status == StoryStatus.Approved)
                .OrderByDescending(s => s.ViewCount)
                .Take(2)
                .ToListAsync();

            return View();
        }

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
