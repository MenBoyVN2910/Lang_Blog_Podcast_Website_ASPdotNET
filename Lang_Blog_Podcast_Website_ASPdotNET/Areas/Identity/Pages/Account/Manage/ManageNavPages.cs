// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// API này hỗ trợ cơ sở hạ tầng giao diện người dùng mặc định của ASP.NET Core Identity 
    /// và không nhằm mục đích sử dụng trực tiếp từ mã của bạn. 
    /// API này có thể thay đổi hoặc bị xóa trong các bản phát hành tương lai.
    /// </summary>
    public static class ManageNavPages
    {
        // Các thuộc tính định danh cho các trang quản lý tài khoản
        public static string Index => "Index";
        public static string Email => "Email";
        public static string ChangePassword => "ChangePassword";
        public static string DownloadPersonalData => "DownloadPersonalData";
        public static string DeletePersonalData => "DeletePersonalData";
        public static string ExternalLogins => "ExternalLogins";
        public static string PersonalData => "PersonalData";
        public static string TwoFactorAuthentication => "TwoFactorAuthentication";

        // Các phương thức hỗ trợ xác định lớp CSS "active" cho menu điều hướng dựa trên ngữ cảnh view hiện tại

        /// <summary> Trả về lớp CSS 'active' nếu trang hiện tại là trang Index </summary>
        public static string? IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        /// <summary> Trả về lớp CSS 'active' nếu trang hiện tại là trang Email </summary>
        public static string? EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);

        /// <summary> Trả về lớp CSS 'active' nếu trang hiện tại là trang Thay đổi mật khẩu </summary>
        public static string? ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        /// <summary> Trả về lớp CSS 'active' nếu trang hiện tại là trang Tải dữ liệu cá nhân </summary>
        public static string? DownloadPersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DownloadPersonalData);

        /// <summary> Trả về lớp CSS 'active' nếu trang hiện tại là trang Xóa dữ liệu cá nhân </summary>
        public static string? DeletePersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeletePersonalData);

        /// <summary> Trả về lớp CSS 'active' nếu trang hiện tại là trang Đăng nhập bên ngoài </summary>
        public static string? ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);

        /// <summary> Trả về lớp CSS 'active' nếu trang hiện tại là trang Dữ liệu cá nhân </summary>
        public static string? PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);

        /// <summary> Trả về lớp CSS 'active' nếu trang hiện tại là trang Xác thực 2 yếu tố </summary>
        public static string? TwoFactorAuthenticationNavClass(ViewContext viewContext) => PageNavClass(viewContext, TwoFactorAuthentication);

        /// <summary>
        /// Phương thức dùng chung để so sánh trang hiện tại với trang mục tiêu 
        /// và trả về chuỗi "active" nếu khớp để làm nổi bật menu.
        /// </summary>
        public static string? PageNavClass(ViewContext viewContext, string page)
        {
            // Lấy tên trang đang hoạt động từ ViewData hoặc từ tên tệp tin action
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);

            // So sánh không phân biệt hoa thường, trả về "active" nếu trùng khớp
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }
    }
}