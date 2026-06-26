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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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

            // 5 Bài viết/Podcast mới nhất cho Carousel
            var latestPodcasts = await _db.PodCasts
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.Status == StoryStatus.Approved)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new {
                    Type = "podcast",
                    p.Id,
                    p.Title,
                    p.Description,
                    p.ImagePath,
                    p.CreatedAt,
                    p.ViewCount,
                    Author = p.Author ?? p.User.FullName,
                    CategoryName = p.Category.Name,
                    AudioPath = p.AudioPath
                })
                .ToListAsync();

            var latestStories = await _db.Stories
                .Include(s => s.Category)
                .Include(s => s.User)
                .Where(s => s.Status == StoryStatus.Approved)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new {
                    Type = "story",
                    s.Id,
                    s.Title,
                    Description = s.Content.Length > 120 ? s.Content.Substring(0, 120) + "..." : s.Content,
                    s.ImagePath,
                    s.CreatedAt,
                    s.ViewCount,
                    Author = s.User.FullName,
                    CategoryName = s.Category.Name,
                    AudioPath = (string)null
                })
                .ToListAsync();

            ViewBag.CarouselItems = latestPodcasts.Concat(latestStories)
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToList();

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
