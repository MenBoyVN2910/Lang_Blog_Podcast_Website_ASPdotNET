using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng liên quan đến việc phát và danh sách Podcast.
    /// </summary>
    public class PodcastController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PodcastController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// GET: /Podcast/
        /// Hiển thị giao diện danh sách các số phát sóng Podcast.
        /// </summary>
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var query = _db.PodCasts
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.Status == StoryStatus.Approved);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Title.Contains(search) || p.Author.Contains(search) || p.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var podcasts = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Categories = await _db.Categories.ToListAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            return View(podcasts);
        }

        /// <summary>
        /// GET: /Podcast/Submit
        /// Hiển thị form gửi podcast.
        /// </summary>
        public async Task<IActionResult> Submit()
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View();
        }

        /// <summary>
        /// POST: /Podcast/Submit
        /// Xử lý lưu podcast mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(PodCastUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string imageFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "Podcasts");
                    string audioFolder = Path.Combine(_webHostEnvironment.WebRootPath, "audio", "Podcasts");

                    if (!Directory.Exists(imageFolder))
                        Directory.CreateDirectory(imageFolder);

                    if (!Directory.Exists(audioFolder))
                        Directory.CreateDirectory(audioFolder);

                    string imageFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ImageFile.FileName);
                    string imagePath = Path.Combine(imageFolder, imageFileName);

                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    string audioFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.AudioFile.FileName);
                    string audioPath = Path.Combine(audioFolder, audioFileName);

                    using (var fileStream = new FileStream(audioPath, FileMode.Create))
                    {
                        await model.AudioFile.CopyToAsync(fileStream);
                    }

                    var newPodcast = new PodCast
                    {
                        Title = model.Title,
                        Author = model.Author,
                        Description = model.Description,
                        EpisodeNumber = model.EpisodeNumber,
                        CategoryId = model.CategoryId,
                        ImagePath = "/images/Podcasts/" + imageFileName,
                        AudioPath = "/audio/Podcasts/" + audioFileName,
                        Status = StoryStatus.Pending,
                        CreatedAt = DateTime.Now,
                        UserId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null,
                        Duration = model.Duration
                    };

                    _db.PodCasts.Add(newPodcast);
                    await _db.SaveChangesAsync();

                    ViewBag.SuccessMessage = "Podcast của bạn đã được gửi thành công và đang chờ xét duyệt!";
                    ViewBag.Categories = await _db.Categories.ToListAsync();
                    ModelState.Clear();
                    return View(new PodCastUploadViewModel());
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Đã có lỗi xảy ra: " + ex.Message;
                }
            }

            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(model);
        }

        /// <summary>
        /// GET: /Podcast/Details/5
        /// Hiển thị chi tiết Podcast (đã duyệt).
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var podcast = await _db.PodCasts
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (podcast == null || podcast.Status != StoryStatus.Approved)
            {
                return NotFound();
            }

            podcast.ViewCount++;
            await _db.SaveChangesAsync();

            return View(podcast);
        }
    }
}
