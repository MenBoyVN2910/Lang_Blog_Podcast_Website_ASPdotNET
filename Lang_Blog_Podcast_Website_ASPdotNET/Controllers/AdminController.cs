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

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    /// <summary>
    /// Controller xử lý các hoạt động quản trị hệ thống như duyệt câu chuyện, quản lý danh mục và người dùng.
    /// Yêu cầu vai trò Admin để truy cập toàn bộ các Action.
    /// </summary>
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

        /// <summary>
        /// GET: /Admin/
        /// Trang Dashboard chính của Admin: Hiển thị danh sách câu chuyện chờ duyệt, đã duyệt và quản lý danh mục.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Tải danh sách bài viết chờ duyệt
            var pendingStories = await _db.Stories
                .Include(s => s.Category)
                .Include(s => s.User)
                .Where(s => s.Status == StoryStatus.Pending)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            // Tải danh sách bài viết đã duyệt
            var approvedStories = await _db.Stories
                .Include(s => s.Category)
                .Include(s => s.User)
                .Where(s => s.Status == StoryStatus.Approved)
                .OrderByDescending(s => s.PublishDate)
                .ToListAsync();

            // Tải danh sách toàn bộ danh mục bài viết
            var categories = await _db.Categories.ToListAsync();

            // Tải danh sách người dùng để phân quyền
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = adminUsers.Select(u => u.Id).ToHashSet();
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = users.Select(user => new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsAdmin = adminIds.Contains(user.Id)
            }).ToList();

            var viewModel = new AdminDashboardViewModel
            {
                PendingStories = pendingStories,
                ApprovedStories = approvedStories,
                Categories = categories,
                PendingCount = pendingStories.Count,
                ApprovedCount = approvedStories.Count,
                TotalCategoriesCount = categories.Count,
                Users = userRolesViewModel
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST: /Admin/CreateCategory
        /// Thêm danh mục mới vào hệ thống (không cho phép trùng lặp tên).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                string cleanedName = categoryName.Trim();
                bool exists = await _db.Categories.AnyAsync(c => c.Name.ToLower() == cleanedName.ToLower());
                
                if (!exists)
                {
                    _db.Categories.Add(new Category { Name = cleanedName });
                    await _db.SaveChangesAsync();
                    TempData["AdminMessage"] = "Đã thêm danh mục mới thành công!";
                }
                else
                {
                    TempData["AdminError"] = "Tên danh mục này đã tồn tại!";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// POST: /Admin/DeleteCategory/5
        /// Xóa danh mục bài viết.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (await _db.Categories.FindAsync(id) is { } category)
            {
                _db.Categories.Remove(category);
                await _db.SaveChangesAsync();
                TempData["AdminMessage"] = "Đã xóa danh mục thành công!";
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// GET: /Admin/ManageUsers
        /// Quản lý danh sách thành viên và phân quyền trong hệ thống.
        /// </summary>
        public async Task<IActionResult> ManageUsers()
        {
            // TỐI ƯU: Tải danh sách Admin trước đưa vào HashSet trong bộ nhớ để tránh lỗi truy vấn N+1
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var adminIds = adminUsers.Select(u => u.Id).ToHashSet();

            var users = await _userManager.Users.ToListAsync();

            var userRolesViewModel = users.Select(user => new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                IsAdmin = adminIds.Contains(user.Id) // Kiểm tra tốc độ O(1)
            }).ToList();

            return View(userRolesViewModel);
        }

        /// <summary>
        /// POST: /Admin/ToggleAdminRole
        /// Bật/Tắt quyền quản trị (Admin) của một thành viên. Ngăn ngừa tự hạ quyền của chính mình.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // BẢO MẬT: Không cho phép Admin tự hạ quyền của bản thân tránh khóa tài khoản Admin cuối cùng
            string currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            {
                TempData["AdminError"] = "Bạn không thể tự gỡ quyền Admin của chính mình!";
                return Redirect("/Admin#manageusers");
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

            return Redirect("/Admin#manageusers");
        }

        /// <summary>
        /// POST: /Admin/CreateUser
        /// Thêm người dùng mới vào hệ thống
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string fullName, string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["AdminError"] = "Vui lòng nhập đầy đủ thông tin!";
                return Redirect("/Admin#manageusers");
            }

            // Kiểm tra định dạng email đuôi gmail/outlook/yahoo
            string cleanedEmail = email.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(cleanedEmail, @"^[a-zA-Z0-9._%+-]+@(gmail\.com|outlook\.com|yahoo\.com)$"))
            {
                TempData["AdminError"] = "Chỉ chấp nhận email đuôi @gmail.com, @outlook.com hoặc @yahoo.com!";
                return Redirect("/Admin#manageusers");
            }

            // Kiểm tra trùng email
            var existingUser = await _userManager.FindByEmailAsync(cleanedEmail);
            if (existingUser != null)
            {
                TempData["AdminError"] = "Email này đã được sử dụng!";
                return Redirect("/Admin#manageusers");
            }

            var newUser = new ApplicationUser
            {
                FullName = fullName.Trim(),
                UserName = cleanedEmail,
                Email = cleanedEmail,
                EmailConfirmed = true // Tự động xác thực email khi admin tạo
            };

            var result = await _userManager.CreateAsync(newUser, password);
            if (result.Succeeded)
            {
                // Nếu chọn quyền Admin, thêm vào Role Admin
                if (role == "Admin")
                {
                    await _userManager.AddToRoleAsync(newUser, "Admin");
                }
                TempData["AdminMessage"] = $"Đã tạo tài khoản {newUser.UserName} thành công!";
            }
            else
            {
                TempData["AdminError"] = $"Lỗi tạo tài khoản: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return Redirect("/Admin#manageusers");
        }

        /// <summary>
        /// POST: /Admin/DeleteUser
        /// Xóa người dùng khỏi hệ thống
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Không cho phép tự xóa tài khoản của chính mình
            string currentUserId = _userManager.GetUserId(User);
            if (userId == currentUserId)
            {
                TempData["AdminError"] = "Bạn không thể tự xóa tài khoản của chính mình!";
                return Redirect("/Admin#manageusers");
            }

            // Gỡ bỏ liên kết UserId của người dùng này khỏi tất cả các bài viết (chuyển thành vô danh)
            var userStories = await _db.Stories.Where(s => s.UserId == userId).ToListAsync();
            foreach (var story in userStories)
            {
                story.UserId = null;
            }
            await _db.SaveChangesAsync();

            // Xóa người dùng
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["AdminMessage"] = $"Đã xóa tài khoản {user.UserName} thành công!";
            }
            else
            {
                TempData["AdminError"] = $"Không thể xóa tài khoản: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return Redirect("/Admin#manageusers");
        }



        /// <summary>
        /// POST: /Admin/Approve
        /// Phê duyệt một câu chuyện để xuất bản công khai lên tập san.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string finalIssueNumber)
        {
            if (await _db.Stories.FindAsync(id) is not { } story)
            {
                return NotFound();
            }

            story.Status = StoryStatus.Approved;
            story.PublishDate = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(finalIssueNumber))
            {
                story.IssueNumber = finalIssueNumber.Trim();
            }

            await _db.SaveChangesAsync();
            TempData["AdminMessage"] = $"Đã duyệt và xuất bản bài viết '{story.Title}' thành công!";

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// POST: /Admin/Reject
        /// Từ chối câu chuyện: Xóa bài khỏi cơ sở dữ liệu và dọn dẹp tệp tin ảnh bìa vật lý trên Server.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            if (await _db.Stories.FindAsync(id) is not { } story)
            {
                return NotFound();
            }

            string imagePath = story.ImagePath;

            _db.Stories.Remove(story);
            await _db.SaveChangesAsync();

            // Xóa tệp vật lý sau khi DB cập nhật thành công
            DeletePhysicalFile(imagePath);

            TempData["AdminMessage"] = $"Đã xóa bài viết '{story.Title}' và dọn dẹp các tệp liên quan.";

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Hàm hỗ trợ xóa các tệp tin vật lý trong thư mục wwwroot để tránh rác server.
        /// </summary>
        private void DeletePhysicalFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            try
            {
                string absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
                if (System.IO.File.Exists(absolutePath))
                {
                    System.IO.File.Delete(absolutePath);
                }
            }
            catch (Exception)
            {
                // Ghi log lỗi nếu cần thiết
            }
        }
    }
}