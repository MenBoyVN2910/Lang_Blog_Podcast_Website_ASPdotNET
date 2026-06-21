using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Lang_Blog_Podcast_Website_ASPdotNET.Data; // Đảm bảo đúng namespace ApplicationUser của bạn

namespace Lang_Blog_Podcast_Website_ASPdotNET.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool HasAvatar { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Số Điện Thoạia")]
            // Kiểm tra 10 số, bắt đầu bằng 03, 05, 07, 08, hoặc 09.
            [RegularExpression(@"^0(3|5|7|8|9)[0-9]{8}$", ErrorMessage = "Số điện thoại không hợp lệ. Vui lòng nhập 10 chữ số bắt đầu bằng 03, 05, 07, 08 hoặc 09.")]
            public string? PhoneNumber { get; set; }

            // Khai báo kiểu IFormFile để nhận file gửi lên từ form html
            public IFormFile? ProfilePicture { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };

            HasAvatar = user.ProfilePicture != null && user.ProfilePicture.Length > 0;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải thông tin user.");
            }

            await LoadAsync(user);

            if (user.ProfilePicture != null)
            {
                ViewData["UserAvatar"] = user.ProfilePicture;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải thông tin user.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // 1. Xử lý lưu số điện thoại nếu có thay đổi
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Lỗi không mong muốn khi cập nhật số điện thoại.";
                    return RedirectToPage();
                }
            }

            // 2. Xử lý File ảnh được upload lên
            if (Input.ProfilePicture != null)
            {
                using (var dataStream = new MemoryStream())
                {
                    await Input.ProfilePicture.CopyToAsync(dataStream);
                    user.ProfilePicture = dataStream.ToArray(); // Chuyển file thành dạng byte[]
                }

                // Cập nhật thực thể User vào Database
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    StatusMessage = "Lỗi khi lưu ảnh đại diện vào hệ thống.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Hồ sơ cá nhân của bạn đã cập nhật thành công.";
            return RedirectToPage();
        }
    }
}