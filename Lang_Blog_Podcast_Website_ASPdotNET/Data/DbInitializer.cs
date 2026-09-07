using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Data
{
    /// <summary>
    /// Lớp khởi tạo dữ liệu mặc định (Seed Data) cho hệ thống LẶNG.
    /// Tự động tạo Roles cơ bản và các Danh mục mẫu ban đầu khi khởi chạy ứng dụng.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                // 1. Tự động tạo các vai trò (Roles) mặc định nếu chưa tồn tại
                string[] roles = { "Admin", "Member", "Creator" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                        logger.LogInformation("Đã khởi tạo vai trò mặc định: {Role}", role);
                    }
                }

                // 2. Khởi tạo danh mục mặc định nếu hệ thống chưa có danh mục nào
                if (!await context.Categories.AnyAsync())
                {
                    context.Categories.AddRange(
                        new Category { Name = "Tâm Sự & Ký Ức" },
                        new Category { Name = "Nghệ Thuật & Thơ Ca" },
                        new Category { Name = "Âm Nhạc & Thanh Âm" },
                        new Category { Name = "Góc Nhìn Cuộc Sống" },
                        new Category { Name = "Tạp Chí & Văn Hoá" }
                    );
                    await context.SaveChangesAsync();
                    logger.LogInformation("Đã khởi tạo các danh mục mặc định cho hệ thống LẶNG.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Đã xảy ra lỗi khi khởi tạo dữ liệu ban đầu (Seeding Database).");
            }
        }
    }
}
