// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Tên người dùng")]
        public string FullName { get; set; } = default!;

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@(gmail\.com|outlook\.com|yahoo\.com)$",
        ErrorMessage = "Hệ thống chỉ chấp nhận email đuôi @gmail.com, @outlook.com hoặc @yahoo.com")]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "{0} phải có độ dài tối thiểu {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật Khẩu")]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "Nhập Lại Mật Khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string? ConfirmPassword { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            var user = CreateUser();

            // 1. Lưu "Tên người dùng" vào đúng cột FullName dưới DB
            user.FullName = Input.FullName;

            // 2. DÙNG EMAIL LÀM USERNAME ĐỂ ĐĂNG NHẬP
            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);

            // 3. Lưu Email vào cột Email bình thường
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            // 4. Chỉ kiểm tra trùng Email (vì hệ thống dùng Email làm tài khoản đăng nhập)
            var existingByEmail = await _userManager.FindByEmailAsync(Input.Email);
            if (existingByEmail != null)
            {
                ModelState.AddModelError(string.Empty, "Email này đã được sử dụng.");
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                return Page();
            }

            // 5. Tiến hành tạo User xuống Database (Chỉ chạy duy nhất 1 lần)
            IdentityResult result;
            try
            {
                result = await _userManager.CreateAsync(user, Input.Password);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Tạo người dùng thất bại.");
                ModelState.AddModelError(string.Empty, "Không thể tạo tài khoản: Email có thể đã tồn tại.");
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
                return Page();
            }

            // 6. Xử lý sau khi tạo tài khoản thành công
            if (result.Succeeded)
            {
                _logger.LogInformation("Người dùng đã tạo tài khoản mới với mật khẩu.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme)!;

                await _emailSender.SendEmailAsync(Input.Email, "Xác nhận email của bạn",
                    $"Vui lòng xác nhận tài khoản của bạn bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }

            // Đưa các lỗi bảo mật của Identity (ví dụ: mật khẩu yếu) vào ModelState để hiển thị trên giao diện
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Nếu có lỗi xảy ra, render lại trang cùng dữ liệu cũ và thông báo lỗi
        return Page();
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Không thể tạo một thể hiện của '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' không phải là một lớp trừu tượng và có hàm tạo không tham số, hoặc nói cách khác là.. " +
                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("Giao diện người dùng mặc định yêu cầu một kho lưu trữ người dùng có hỗ trợ email.");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}