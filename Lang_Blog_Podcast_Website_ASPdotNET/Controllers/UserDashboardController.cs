using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using Lang_Blog_Podcast_Website_ASPdotNET.Services;
using Microsoft.AspNetCore.Http;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    [Authorize]
    public class UserDashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;

        public UserDashboardController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, NotificationService notificationService)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var recentStories = await _db.Stories
                .Include(s => s.Category)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentPodcasts = await _db.PodCasts
                .Include(p => p.Category)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            var myStories = await _db.Stories
                .Include(s => s.Category)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var myPodcasts = await _db.PodCasts
                .Include(p => p.Category)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var pendingRevisions = await _db.PostRevisions
                .Where(r => r.UserId == userId && r.Status == StoryStatus.Pending)
                .ToListAsync();

            var viewModel = new UserDashboardViewModel
            {
                RecentSubmittedStories = recentStories,
                RecentSubmittedPodcasts = recentPodcasts,
                MyStories = myStories,
                MyPodcasts = myPodcasts,
                PendingRevisions = pendingRevisions,
                TotalStories = myStories.Count,
                TotalPodcasts = myPodcasts.Count,
                PendingCount = myStories.Count(s => s.Status == StoryStatus.Pending) + myPodcasts.Count(p => p.Status == StoryStatus.Pending) + pendingRevisions.Count,
                ApprovedCount = myStories.Count(s => s.Status == StoryStatus.Approved) + myPodcasts.Count(p => p.Status == StoryStatus.Approved)
            };

            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(viewModel);
        }

        // ===================================== SUBMIT STORY =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitStory(StoryUploadViewModel model)
        {
            ModelState.Remove("IssueNumber");

            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                TempData["DashboardError"] = "Vui lòng chọn ảnh bìa cho câu chuyện của bạn!";
                TempData["StoryFormData"] = System.Text.Json.JsonSerializer.Serialize(new {
                    model.Title, model.IssueNumber, model.CategoryId, model.Content
                });
                return RedirectToAction(nameof(Index), new { tab = "write-story" });
            }

            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                var currentUserId = _userManager.GetUserId(User);
                string cleanTitle = model.Title.Trim().ToLower();
                string cleanIssueNumber = string.IsNullOrWhiteSpace(model.IssueNumber) ? "none" : model.IssueNumber.Trim().ToLower();

                // Khác user mà trùng tiêu đề → từ chối
                bool otherUserSameTitle = await _db.Stories
                    .AnyAsync(s => s.Title.Trim().ToLower() == cleanTitle && s.UserId != currentUserId);
                if (otherUserSameTitle)
                {
                    TempData["DashboardError"] = "Tiêu đề này đã được sử dụng bởi tác giả khác!";
                    TempData["StoryFormData"] = System.Text.Json.JsonSerializer.Serialize(new {
                        model.Title, model.IssueNumber, model.CategoryId, model.Content
                    });
                    return RedirectToAction(nameof(Index), new { tab = "write-story" });
                }

                // Cùng user, trùng tiêu đề, trùng cả số phát hành → từ chối
                bool sameUserSameTitleSameIssue = await _db.Stories
                    .AnyAsync(s => s.Title.Trim().ToLower() == cleanTitle
                                && s.UserId == currentUserId
                                && (s.IssueNumber ?? "None").Trim().ToLower() == cleanIssueNumber);
                if (sameUserSameTitleSameIssue)
                {
                    TempData["DashboardError"] = "Bạn đã có bài viết cùng tiêu đề và số phát hành này!";
                    TempData["StoryFormData"] = System.Text.Json.JsonSerializer.Serialize(new {
                        model.Title, model.IssueNumber, model.CategoryId, model.Content
                    });
                    return RedirectToAction(nameof(Index), new { tab = "write-story" });
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string uploadedFileName = UploadFile(model.ImageFile, "uploads/images");

                    var newStory = new Story
                    {
                        Title = model.Title,
                        Content = model.Content,
                        UserId = _userManager.GetUserId(User),
                        // Note: Author column removed from Story; User.FullName will be used as display name where applicable. This assignment keeps author relation via UserId only.
                        IssueNumber = string.IsNullOrWhiteSpace(model.IssueNumber) ? "None" : model.IssueNumber,
                        CategoryId = model.CategoryId,
                        ImagePath = uploadedFileName,
                        Status = StoryStatus.Pending,
                        CreatedAt = DateTime.Now
                    };

                    _db.Stories.Add(newStory);
                    await _db.SaveChangesAsync();

                    var userName = User.Identity?.Name ?? "Người dùng";
                    await _notificationService.NotifyAllAdminsAsync("Bài viết mới", $"Người dùng {userName} vừa gửi một bài viết mới: '{model.Title}'", "NewSubmission");

                    TempData["DashboardSuccess"] = "Mảnh ghép của bạn đã được gửi thành công và đang chờ duyệt.";
                }
                catch (Exception)
                {
                    TempData["DashboardError"] = "Đã xảy ra sự cố hệ thống. Vui lòng thử lại sau.";
                }
            }
            else
            {
                var errorMessages = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToList();
                TempData["DashboardError"] = string.Join("<br/>", errorMessages);
                TempData["StoryFormData"] = System.Text.Json.JsonSerializer.Serialize(new {
                    model.Title, model.IssueNumber, model.CategoryId, model.Content
                });
            }

            return RedirectToAction(nameof(Index), new { tab = "write-story" });
        }

        // ===================================== SUBMIT PODCAST =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPodcast(PodCastUploadViewModel model)
        {
            // Kiểm tra trùng lặp tiêu đề Podcast (có phân biệt theo người dùng)
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                var currentUserId = _userManager.GetUserId(User);
                string cleanTitle = model.Title.Trim().ToLower();

                // Khác user mà trùng tiêu đề → từ chối
                bool otherUserSameTitle = await _db.PodCasts
                    .AnyAsync(p => p.Title.Trim().ToLower() == cleanTitle && p.UserId != currentUserId);
                if (otherUserSameTitle)
                {
                    TempData["DashboardError"] = "Tiêu đề Podcast này đã được sử dụng bởi người dẫn khác!";
                    TempData["PodcastFormData"] = System.Text.Json.JsonSerializer.Serialize(new {
                        model.Title, model.Author, model.EpisodeNumber, model.CategoryId, model.Description
                    });
                    return RedirectToAction(nameof(Index), new { tab = "upload-podcast" });
                }

                // Cùng user, trùng tiêu đề, trùng cả số tập → từ chối
                bool sameUserSameTitleSameEpisode = await _db.PodCasts
                    .AnyAsync(p => p.Title.Trim().ToLower() == cleanTitle
                                && p.UserId == currentUserId
                                && p.EpisodeNumber == model.EpisodeNumber);
                if (sameUserSameTitleSameEpisode)
                {
                    TempData["DashboardError"] = "Bạn đã có podcast cùng tiêu đề và số tập này!";
                    TempData["PodcastFormData"] = System.Text.Json.JsonSerializer.Serialize(new {
                        model.Title, model.Author, model.EpisodeNumber, model.CategoryId, model.Description
                    });
                    return RedirectToAction(nameof(Index), new { tab = "upload-podcast" });
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string imageFileName = UploadFile(model.ImageFile, "images/Podcasts");
                    string audioFileName = UploadFile(model.AudioFile, "audio/Podcasts");

                    var newPodcast = new PodCast
                    {
                        Title = model.Title,
                        Author = model.Author,
                        Description = model.Description,
                        EpisodeNumber = model.EpisodeNumber,
                        CategoryId = model.CategoryId,
                        ImagePath = imageFileName,
                        AudioPath = audioFileName,
                        Status = StoryStatus.Pending,
                        CreatedAt = DateTime.Now,
                        UserId = _userManager.GetUserId(User),
                        Duration = model.Duration
                    };

                    _db.PodCasts.Add(newPodcast);
                    await _db.SaveChangesAsync();

                    var userName = User.Identity?.Name ?? "Người dùng";
                    await _notificationService.NotifyAllAdminsAsync("Podcast mới", $"Người dùng {userName} vừa gửi một Podcast mới: '{model.Title}'", "NewSubmission");

                    TempData["DashboardSuccess"] = "Podcast của bạn đã được gửi thành công và đang chờ xét duyệt!";
                }
                catch (Exception ex)
                {
                    TempData["DashboardError"] = "Đã có lỗi xảy ra: " + ex.Message;
                }
            }
            else
            {
                var errorMessages = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToList();
                TempData["DashboardError"] = string.Join("<br/>", errorMessages);
                TempData["PodcastFormData"] = System.Text.Json.JsonSerializer.Serialize(new {
                    model.Title, model.Author, model.EpisodeNumber, model.CategoryId, model.Description
                });
            }

            return RedirectToAction(nameof(Index), new { tab = "upload-podcast" });
        }


        // ===================================== EDIT STORY =====================================

        [HttpGet]
        public async Task<IActionResult> EditStory(int id)
        {
            var userId = _userManager.GetUserId(User);
            var story = await _db.Stories.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (story == null) return NotFound();

            var model = new StoryUploadViewModel
            {
                Title = story.Title,
                Content = story.Content,
                CategoryId = story.CategoryId,
                IssueNumber = story.IssueNumber
            };

            ViewBag.StoryId = story.Id;
            ViewBag.CurrentImagePath = story.ImagePath;
            ViewBag.Categories = await _db.Categories.ToListAsync();
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStory(int id, StoryUploadViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var story = await _db.Stories.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (story == null) return NotFound();

            ModelState.Remove("ImageFile");
            ModelState.Remove("IssueNumber");

            // Kiểm tra trùng lặp tiêu đề (loại trừ chính bài đang sửa)
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                string cleanTitle = model.Title.Trim().ToLower();
                string cleanIssueNumber = string.IsNullOrWhiteSpace(model.IssueNumber) ? "none" : model.IssueNumber.Trim().ToLower();

                bool otherUserSameTitle = await _db.Stories
                    .AnyAsync(s => s.Id != id && s.Title.Trim().ToLower() == cleanTitle && s.UserId != userId);
                if (otherUserSameTitle)
                {
                    ModelState.AddModelError("Title", "Tiêu đề này đã được sử dụng bởi tác giả khác!");
                }

                bool sameUserSameTitleSameIssue = await _db.Stories
                    .AnyAsync(s => s.Id != id && s.Title.Trim().ToLower() == cleanTitle
                                && s.UserId == userId
                                && (s.IssueNumber ?? "None").Trim().ToLower() == cleanIssueNumber);
                if (sameUserSameTitleSameIssue)
                {
                    ModelState.AddModelError("Title", "Bạn đã có bài viết cùng tiêu đề và số phát hành này!");
                }
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToList();
                ViewBag.ErrorMessage = string.Join("<br/>", errorMessages);
                ViewBag.StoryId = story.Id;
                ViewBag.CurrentImagePath = story.ImagePath;
                ViewBag.Categories = await _db.Categories.ToListAsync();
                return View(model);
            }

            string newImagePath = story.ImagePath;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                newImagePath = UploadFile(model.ImageFile, "uploads/images");
            }

            // Logic Shadow Copy
            if (story.Status == StoryStatus.Approved)
            {
                var revision = new PostRevision
                {
                    ContentType = "Story",
                    OriginalPostId = story.Id,
                    Title = model.Title,
                    Content = model.Content,
                    CategoryId = model.CategoryId,
                    ImagePath = newImagePath,
                    UserId = userId,
                    Status = StoryStatus.Pending,
                    CreatedAt = DateTime.Now
                };
                
                story.Status = StoryStatus.EditPending;
                story.IssueNumber = string.IsNullOrWhiteSpace(model.IssueNumber) ? "None" : model.IssueNumber;

                _db.PostRevisions.Add(revision);
                
                var userName = User.Identity?.Name ?? "Người dùng";
                await _notificationService.NotifyAllAdminsAsync("Bản chỉnh sửa mới", $"Người dùng {userName} vừa gửi bản chỉnh sửa cho bài viết: '{story.Title}'", "NewSubmission");
                
                TempData["DashboardSuccess"] = "Bản chỉnh sửa đã được gửi đi và đang chờ Admin phê duyệt.";
            }
            else if (story.Status == StoryStatus.Pending || story.Status == StoryStatus.Rejected)
            {
                story.Title = model.Title;
                story.Content = model.Content;
                story.CategoryId = model.CategoryId;
                story.IssueNumber = string.IsNullOrWhiteSpace(model.IssueNumber) ? "None" : model.IssueNumber;
                story.ImagePath = newImagePath;
                story.Status = StoryStatus.Pending; // Nếu đang Reject thì sửa xong chuyển lại thành Pending
                story.RejectionReason = null; // Xóa lý do từ chối cũ

                TempData["DashboardSuccess"] = "Đã cập nhật bài viết thành công.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===================================== EDIT PODCAST =====================================

        [HttpGet]
        public async Task<IActionResult> EditPodcast(int id)
        {
            var userId = _userManager.GetUserId(User);
            var podcast = await _db.PodCasts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (podcast == null) return NotFound();

            var model = new PodCastUploadViewModel
            {
                Title = podcast.Title,
                Author = podcast.Author,
                Description = podcast.Description,
                EpisodeNumber = podcast.EpisodeNumber,
                CategoryId = podcast.CategoryId,
                Duration = podcast.Duration
            };

            ViewBag.PodcastId = podcast.Id;
            ViewBag.CurrentImagePath = podcast.ImagePath;
            ViewBag.CurrentAudioPath = podcast.AudioPath;
            ViewBag.Categories = await _db.Categories.ToListAsync();
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPodcast(int id, PodCastUploadViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var podcast = await _db.PodCasts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (podcast == null) return NotFound();

            ModelState.Remove("ImageFile");
            ModelState.Remove("AudioFile");

            // Kiểm tra trùng lặp tiêu đề Podcast (loại trừ chính podcast đang sửa)
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                string cleanTitle = model.Title.Trim().ToLower();

                bool otherUserSameTitle = await _db.PodCasts
                    .AnyAsync(p => p.Id != id && p.Title.Trim().ToLower() == cleanTitle && p.UserId != userId);
                if (otherUserSameTitle)
                {
                    ModelState.AddModelError("Title", "Tiêu đề này đã được sử dụng bởi người dẫn khác!");
                }

                bool sameUserSameTitleSameEpisode = await _db.PodCasts
                    .AnyAsync(p => p.Id != id && p.Title.Trim().ToLower() == cleanTitle
                                && p.UserId == userId
                                && p.EpisodeNumber == model.EpisodeNumber);
                if (sameUserSameTitleSameEpisode)
                {
                    ModelState.AddModelError("Title", "Bạn đã có podcast cùng tiêu đề và số tập này!");
                }
            }

            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage)
                                    .ToList();
                ViewBag.ErrorMessage = string.Join("<br/>", errorMessages);
                ViewBag.PodcastId = podcast.Id;
                ViewBag.CurrentImagePath = podcast.ImagePath;
                ViewBag.CurrentAudioPath = podcast.AudioPath;
                ViewBag.Categories = await _db.Categories.ToListAsync();
                return View(model);
            }

            string newImagePath = podcast.ImagePath;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                newImagePath = UploadFile(model.ImageFile, "images/Podcasts");
            }

            string newAudioPath = podcast.AudioPath;
            if (model.AudioFile != null && model.AudioFile.Length > 0)
            {
                newAudioPath = UploadFile(model.AudioFile, "audio/Podcasts");
            }

            // Logic Shadow Copy
            if (podcast.Status == StoryStatus.Approved)
            {
                var revision = new PostRevision
                {
                    ContentType = "Podcast",
                    OriginalPostId = podcast.Id,
                    Title = model.Title,
                    Description = model.Description,
                    CategoryId = model.CategoryId,
                    ImagePath = newImagePath,
                    AudioPath = newAudioPath,
                    UserId = userId,
                    Status = StoryStatus.Pending,
                    CreatedAt = DateTime.Now
                };
                
                podcast.Status = StoryStatus.EditPending;

                _db.PostRevisions.Add(revision);
                
                var userName = User.Identity?.Name ?? "Người dùng";
                await _notificationService.NotifyAllAdminsAsync("Bản chỉnh sửa mới", $"Người dùng {userName} vừa gửi bản chỉnh sửa cho Podcast: '{podcast.Title}'", "NewSubmission");
                
                TempData["DashboardSuccess"] = "Bản chỉnh sửa Podcast đã gửi đi và đang chờ Admin phê duyệt.";
            }
            else if (podcast.Status == StoryStatus.Pending || podcast.Status == StoryStatus.Rejected)
            {
                podcast.Title = model.Title;
                podcast.Description = model.Description;
                podcast.CategoryId = model.CategoryId;
                podcast.EpisodeNumber = model.EpisodeNumber;
                podcast.Duration = model.Duration;
                podcast.ImagePath = newImagePath;
                podcast.AudioPath = newAudioPath;
                podcast.Status = StoryStatus.Pending; 
                podcast.RejectionReason = null;

                TempData["DashboardSuccess"] = "Đã cập nhật Podcast thành công.";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ===================================== DELETE =====================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStory(int id)
        {
            var userId = _userManager.GetUserId(User);
            var story = await _db.Stories.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (story != null)
            {
                // Delete associated revisions if any
                var revisions = await _db.PostRevisions.Where(r => r.ContentType == "Story" && r.OriginalPostId == story.Id).ToListAsync();
                _db.PostRevisions.RemoveRange(revisions);

                _db.Stories.Remove(story);
                await _db.SaveChangesAsync();
                
                if (!string.IsNullOrEmpty(story.ImagePath)) DeletePhysicalFile(story.ImagePath);
                TempData["DashboardSuccess"] = "Đã xóa kỷ niệm thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePodcast(int id)
        {
            var userId = _userManager.GetUserId(User);
            var podcast = await _db.PodCasts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (podcast != null)
            {
                var revisions = await _db.PostRevisions.Where(r => r.ContentType == "Podcast" && r.OriginalPostId == podcast.Id).ToListAsync();
                _db.PostRevisions.RemoveRange(revisions);

                _db.PodCasts.Remove(podcast);
                await _db.SaveChangesAsync();
                
                if (!string.IsNullOrEmpty(podcast.ImagePath)) DeletePhysicalFile(podcast.ImagePath);
                if (!string.IsNullOrEmpty(podcast.AudioPath)) DeletePhysicalFile(podcast.AudioPath);
                
                TempData["DashboardSuccess"] = "Đã xóa podcast thành công.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ===================================== PREVIEW =====================================

        public async Task<IActionResult> PreviewStory(int id)
        {
            var userId = _userManager.GetUserId(User);
            var story = await _db.Stories.Include(s => s.Category).Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
            if (story == null) return NotFound();
            return View("~/Views/Story/Details.cshtml", story);
        }

        public async Task<IActionResult> PreviewPodcast(int id)
        {
            var userId = _userManager.GetUserId(User);
            var podcast = await _db.PodCasts.Include(p => p.Category).Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (podcast == null) return NotFound();
            return View("~/Views/Podcast/Details.cshtml", podcast);
        }

        // ===================================== NOTIFICATIONS =====================================

        [HttpPost]
        public async Task<IActionResult> DismissNotification(int id, string type)
        {
            var userId = _userManager.GetUserId(User);
            
            if (type == "Story")
            {
                var story = await _db.Stories.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
                if (story != null && (story.Status == StoryStatus.Approved || story.Status == StoryStatus.Rejected))
                {
                    // For now, we don't have a flag to "hide" it. We can just keep it in DB but maybe the UI handles it differently.
                    // If we need to dismiss, maybe we can clear RejectionReason or add an IsDismissed flag. 
                    // To keep it simple without DB changes, let's just return Ok for Optimistic UI or update a session.
                    return Ok();
                }
            }
            else if (type == "Podcast")
            {
                var podcast = await _db.PodCasts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
                if (podcast != null && (podcast.Status == StoryStatus.Approved || podcast.Status == StoryStatus.Rejected))
                {
                    return Ok();
                }
            }

            return BadRequest();
        }

        // ===================================== HELPERS =====================================

        private string UploadFile(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0) return "#";

            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, folderPath);
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

            string uniqueFileName = $"{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return $"/{folderPath}/{uniqueFileName}";
        }

        private void DeletePhysicalFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath) || relativePath == "#") return;
            try
            {
                string absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
                if (System.IO.File.Exists(absolutePath))
                {
                    System.IO.File.Delete(absolutePath);
                }
            }
            catch (Exception) { }
        }
    }
}
