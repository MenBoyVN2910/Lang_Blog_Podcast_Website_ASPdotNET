using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using System.IO;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Profile/{username?}
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? username)
        {
            ApplicationUser targetUser;
            bool isOwner = false;

            if (string.IsNullOrEmpty(username))
            {
                // Truy cập trang cá nhân của chính mình
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
                targetUser = await _userManager.GetUserAsync(User);
                isOwner = true;
            }
            else
            {
                // Xem profile của người khác
                targetUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == username);
                if (targetUser == null)
                {
                    return NotFound("Không tìm thấy người dùng này.");
                }

                if (User.Identity.IsAuthenticated)
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (currentUser != null && currentUser.Id == targetUser.Id)
                    {
                        isOwner = true;
                    }
                }
            }

            var roles = await _userManager.GetRolesAsync(targetUser);
            string roleBadge = "Thành Viên";
            if (roles.Contains("Admin")) roleBadge = "Admin ✦";
            else if (roles.Contains("Creator")) roleBadge = "Nhà Sáng Tạo ✎"; // Tùy chọn

            // Lấy nội dung
            var stories = await _db.Stories
                .Include(s => s.Category)
                .Where(s => s.UserId == targetUser.Id && s.Status == StoryStatus.Approved)
                .OrderByDescending(s => s.PublishDate ?? s.CreatedAt)
                .ToListAsync();

            var podcasts = await _db.PodCasts
                .Include(p => p.Category)
                .Where(p => p.UserId == targetUser.Id && p.Status == StoryStatus.Approved)
                .OrderByDescending(p => p.PublishDate ?? p.CreatedAt)
                .ToListAsync();

            // Tính toán gamification
            int totalStories = stories.Count;
            int totalPodcasts = podcasts.Count;
            int totalViews = stories.Sum(s => s.ViewCount) + podcasts.Sum(p => p.ViewCount);
            
            var achievements = CalculateAchievements(totalStories, totalPodcasts, totalViews);

            var vm = new ProfileViewModel
            {
                User = targetUser,
                IsOwner = isOwner,
                RoleBadge = roleBadge,
                TotalStories = totalStories,
                TotalPodcasts = totalPodcasts,
                TotalViews = totalViews,
                TotalFollowers = 0, // Placeholder
                Stories = stories,
                Podcasts = podcasts,
                Achievements = achievements
            };

            // Nếu là chính chủ, lấy thêm mục yêu thích
            if (isOwner)
            {
                var favStoryIds = await _db.UserFavorites
                    .Where(f => f.UserId == targetUser.Id && f.ContentType == "Story")
                    .Select(f => f.ContentId)
                    .ToListAsync();
                    
                vm.FavoriteStories = await _db.Stories
                    .Include(s => s.Category)
                    .Where(s => favStoryIds.Contains(s.Id))
                    .ToListAsync();

                var favPodcastIds = await _db.UserFavorites
                    .Where(f => f.UserId == targetUser.Id && f.ContentType == "Podcast")
                    .Select(f => f.ContentId)
                    .ToListAsync();
                    
                vm.FavoritePodcasts = await _db.PodCasts
                    .Include(p => p.Category)
                    .Where(p => favPodcastIds.Contains(p.Id))
                    .ToListAsync();
            }

            return View(vm);
        }

        // Cập nhật Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string bio, string slogan, IFormFile coverImage, IFormFile avatarImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.Bio = bio;
            user.Slogan = slogan;

            // Xử lý upload ảnh bìa (Lưu ra file)
            if (coverImage != null && coverImage.Length > 0)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "covers");
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImage.FileName);
                string filePath = Path.Combine(uploadDir, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(fileStream);
                }

                // Xóa ảnh cũ
                if (!string.IsNullOrEmpty(user.CoverImagePath))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.CoverImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.CoverImagePath = "/uploads/covers/" + fileName;
            }

            // Xử lý upload ảnh đại diện (Lưu byte[])
            if (avatarImage != null && avatarImage.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await avatarImage.CopyToAsync(ms);
                    user.ProfilePicture = ms.ToArray();
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu hồ sơ.";
            }

            return RedirectToAction("Index");
        }

        // Toggle Favorite (Ajax)
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(string contentType, int contentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Chưa đăng nhập" });

            var existingFav = await _db.UserFavorites.FirstOrDefaultAsync(f => 
                f.UserId == user.Id && 
                f.ContentType == contentType && 
                f.ContentId == contentId);

            if (existingFav != null)
            {
                _db.UserFavorites.Remove(existingFav);
                await _db.SaveChangesAsync();
                return Json(new { success = true, isFavorited = false });
            }
            else
            {
                _db.UserFavorites.Add(new UserFavorite
                {
                    UserId = user.Id,
                    ContentType = contentType,
                    ContentId = contentId
                });
                await _db.SaveChangesAsync();
                return Json(new { success = true, isFavorited = true });
            }
        }

        private List<AchievementItem> CalculateAchievements(int stories, int podcasts, int views)
        {
            var list = new List<AchievementItem>();

            // Achievement 1: Podcast đầu tiên
            list.Add(new AchievementItem
            {
                IconClass = "fa-solid fa-microphone",
                IconColor = "#8B0000",
                Title = "Tiếng Nói Đầu Tiên",
                Description = "Đã xuất bản Podcast đầu tiên",
                IsUnlocked = podcasts > 0
            });

            // Achievement 2: 10 Bài viết
            list.Add(new AchievementItem
            {
                IconClass = "fa-solid fa-pen-nib",
                IconColor = "#258cfb",
                Title = "Cây Bút Chăm Chỉ",
                Description = "Đã xuất bản 10 bài viết",
                IsUnlocked = stories >= 10
            });

            // Achievement 3: 100 Views
            list.Add(new AchievementItem
            {
                IconClass = "fa-solid fa-eye",
                IconColor = "#f57c00",
                Title = "Khởi Đầu Tốt Đẹp",
                Description = "Đạt tổng cộng 100 lượt xem",
                IsUnlocked = views >= 100
            });

            // Achievement 4: 1000 Views
            list.Add(new AchievementItem
            {
                IconClass = "fa-solid fa-crown",
                IconColor = "#fbc02d",
                Title = "Ngôi Sao Đang Lên",
                Description = "Đạt tổng cộng 1000 lượt xem",
                IsUnlocked = views >= 1000
            });

            return list;
        }
    }
}
