using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // ------------------------------------------------------------------
        // SECTION 1: QUẢN LÝ USER & PHÂN QUYỀN (TỐI ƯU HIỆU NĂNG)
        // ------------------------------------------------------------------

        public async Task<IActionResult> Index()
        {
            // Lấy câu chuyện chờ duyệt
            var pendingStories = await _db.Stories
                .Include(s => s.Category)
                .Where(s => s.Status == StoryStatus.Pending)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            // Lấy câu chuyện đã duyệt
            var approvedStories = await _db.Stories
                .Include(s => s.Category)
                .Where(s => s.Status == StoryStatus.Approved)
                .OrderByDescending(s => s.PublishDate)
                .ToListAsync();

            // Lấy danh sách danh mục
            var categories = await _db.Categories.ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                PendingStories = pendingStories,
                ApprovedStories = approvedStories,
                Categories = categories,
                PendingCount = pendingStories.Count,
                ApprovedCount = approvedStories.Count,
                TotalCategoriesCount = categories.Count
            };

            return View(viewModel);
        }

        // Hàm Thêm Danh Mục Mới (Sẽ được gọi từ Form)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string CategoryName)
        {
            if (!string.IsNullOrWhiteSpace(CategoryName))
            {
                var exists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == CategoryName.Trim().ToLower());
                if (!exists)
                {
                    _db.Categories.Add(new Category { Name = CategoryName.Trim() });
                    await _db.SaveChangesAsync();
                    TempData["AdminMessage"] = "Đã thêm danh mục mới thành công!";
                }
                else
                {
                    TempData["AdminError"] = "Tên danh mục này đã tồn tại!";
                }
            }
            return RedirectToAction(nameof(Index)); // Xong thì quay lại trang Index
        }

        // 3. Hàm Xóa Danh Mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat != null)
            {
                _db.Categories.Remove(cat);
                await _db.SaveChangesAsync();
                TempData["AdminMessage"] = "Đã xóa danh mục!";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/ManageUsers
        public async Task<IActionResult> ManageUsers()
        {
            // 💡 TỐI ƯU: Lấy trước danh sách Admin để tránh lỗi N+1 Queries (gọi DB liên tục trong vòng lặp foreach)
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = adminUsers.Select(u => u.Id).ToHashSet();

            var users = await _userManager.Users.ToListAsync();

            var userRolesViewModel = users.Select(user => new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsAdmin = adminIds.Contains(user.Id) // Kiểm tra cực nhanh trên HashSet bộ nhớ
            }).ToList();

            return View(userRolesViewModel);
        }

        // POST: Admin/ToggleAdminRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // 🛡️ BẢO MẬT: Ngăn chặn Admin tự hạ quyền của chính mình gây khóa tài khoản
            var currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            {
                TempData["AdminError"] = "Bạn không thể tự gỡ quyền Admin của chính mình!";
                return RedirectToAction(nameof(ManageUsers));
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                TempData["AdminMessage"] = $"Đã thu hồi quyền Admin của tài khoản {user.UserName}.";
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                TempData["AdminMessage"] = $"Đã cấp quyền Admin cho tài khoản {user.UserName} thành công.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

        // ------------------------------------------------------------------
        // SECTION 2: CHỨC NĂNG DUYỆT CÂU CHUYỆN
        // ------------------------------------------------------------------

        // (Đã xóa bỏ hoàn toàn hàm ReviewStories theo yêu cầu)

        // GET: Admin/StoryDetails/5
        public async Task<IActionResult> StoryDetails(int id)
        {
            // 💡 CẬP NHẬT: Thêm .Include(s => s.Category) để xem chi tiết thông tin danh mục của bài viết
            var story = await _db.Stories
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (story == null)
            {
                return NotFound();
            }
            return View(story);
        }

        // POST: Admin/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string finalIssueNumber)
        {
            // 💡 TỐI ƯU: Sử dụng FindAsync thay cho FirstOrDefaultAsync để tìm kiếm theo Primary Key nhanh hơn
            var story = await _db.Stories.FindAsync(id);
            if (story == null)
            {
                return NotFound();
            }

            story.Status = StoryStatus.Approved;
            story.PublishDate = DateTime.Now;

            if (!string.IsNullOrEmpty(finalIssueNumber))
            {
                story.IssueNumber = finalIssueNumber;
            }

            await _db.SaveChangesAsync();
            TempData["AdminMessage"] = $"Đã duyệt và xuất bản bài viết '{story.Title}' thành công!";

            // 💡 Đã đổi hướng trang về Index thay vì ReviewStories
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var story = await _db.Stories.FindAsync(id);
            if (story == null)
            {
                return NotFound();
            }

            // Capture file paths before deleting the DB record
            var imagePath = story.ImagePath;
            // If you have other file properties (e.g. AudioPath), capture them here as well

            // Remove the story entity from the database
            _db.Stories.Remove(story);
            await _db.SaveChangesAsync();

            // Delete physical files after DB change (so we don't lose files if DB update fails)
            DeletePhysicalFile(imagePath);

            TempData["AdminMessage"] = $"Đã xóa bài viết '{story.Title}' và dọn dẹp file liên quan.";

            // 💡 Đã đổi hướng trang về Index thay vì ReviewStories
            return RedirectToAction(nameof(Index));
        }

        // ------------------------------------------------------------------
        // SECTION 3: CÁC HÀM TRỢ GIÚP (HELPER METHODS)
        // ------------------------------------------------------------------

        // Hàm hỗ trợ xóa file vật lý trong thư mục wwwroot một cách an toàn
        private void DeletePhysicalFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            try
            {
                // Tìm đường dẫn tuyệt đối trên ổ cứng của server
                var absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));

                if (System.IO.File.Exists(absolutePath))
                {
                    System.IO.File.Delete(absolutePath);
                }
            }
            catch (Exception)
            {
                // Ghi log lỗi nếu cần, tạm thời bỏ qua để không làm gián đoạn luồng xử lý chính
            }
        }
    }
}