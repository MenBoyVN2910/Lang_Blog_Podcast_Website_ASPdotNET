using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using Lang_Blog_Podcast_Website_ASPdotNET.Data; // Đảm bảo đúng namespace ApplicationUser của bạn

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    [Authorize(Roles = "Admin")] // Bắt buộc phải là Admin mới vào được Controller này
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        // Cấu hình Dependency Injection
        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // Cũ: Các Action Index hiện tại của bạn giữ nguyên...
        public IActionResult Index()
        {
            return View();
        }

        // MỚI: 1. Lấy danh sách tài khoản
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var viewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    // Kiểm tra xem user này có quyền Admin không
                    IsAdmin = await _userManager.IsInRoleAsync(user, "Admin")
                };
                userRolesViewModel.Add(viewModel);
            }

            return View(userRolesViewModel);
        }

        // MỚI: 2. Xử lý nút bấm Cấp/Hủy quyền
        [HttpPost]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Nếu đang là Admin -> Rút quyền
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            // Nếu chưa là Admin -> Cấp quyền
            else
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            // Xử lý xong thì load lại trang danh sách
            return RedirectToAction(nameof(ManageUsers));
        }
    }
}