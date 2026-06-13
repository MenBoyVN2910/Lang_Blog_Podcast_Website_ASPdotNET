using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;

// Khởi tạo WebApplicationBuilder để thiết lập ứng dụng và các dịch vụ (Dependency Injection)
var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------------
// 1. CẤU HÌNH CƠ SỞ DỮ LIỆU (DATABASE)
// --------------------------------------------------------
// Lấy chuỗi kết nối (Connection String) từ file cấu hình (thường là appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Đăng ký ApplicationDbContext vào hệ thống Service, sử dụng SQL Server làm cơ sở dữ liệu
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// --------------------------------------------------------
// 2. CẤU HÌNH TÀI KHOẢN & PHÂN QUYỀN (IDENTITY)
// --------------------------------------------------------
// Đăng ký Identity để quản lý user (ApplicationUser) và quyền (IdentityRole)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Không bắt buộc người dùng phải xác thực email mới được đăng nhập
    options.SignIn.RequireConfirmedAccount = false;

    // Cấu hình độ phức tạp của mật khẩu
    options.Password.RequireDigit = true;           // Phải có ít nhất 1 chữ số
    options.Password.RequiredLength = 6;            // Độ dài tối thiểu là 6 ký tự
    options.Password.RequireUppercase = false;      // Không bắt buộc có chữ in hoa
    options.Password.RequireLowercase = false;      // Không bắt buộc có chữ thường
    options.Password.RequireNonAlphanumeric = false;// Không bắt buộc ký tự đặc biệt (VD: @, #, !)
})
.AddRoles<IdentityRole>()                           // Kích hoạt tính năng phân quyền (Role)
.AddEntityFrameworkStores<ApplicationDbContext>()   // Chỉ định nơi lưu trữ dữ liệu Identity (DbContext)
.AddDefaultTokenProviders()                         // Cung cấp token cho việc reset mật khẩu, xác nhận email...
.AddDefaultUI();                                    // Sử dụng giao diện mặc định của Identity (Login, Register...)


// --------------------------------------------------------
// 3. CẤU HÌNH MVC & RAZOR PAGES
// --------------------------------------------------------
// Đăng ký các dịch vụ cần thiết để sử dụng mô hình MVC (Model-View-Controller)
builder.Services.AddControllersWithViews();
// Đăng ký dịch vụ cho Razor Pages (Identity UI mặc định dùng Razor Pages)
builder.Services.AddRazorPages();

// Xây dựng ứng dụng từ các cấu hình bên trên
var app = builder.Build();


// --------------------------------------------------------
// 4. CẤU HÌNH MIDDLEWARE (PIPELINE XỬ LÝ REQUEST)
// --------------------------------------------------------
// Nếu môi trường không phải là Development (tức là Production hoặc Staging)
if (!app.Environment.IsDevelopment())
{
    // Bắt lỗi toàn cục và chuyển hướng đến trang /Home/Error
    app.UseExceptionHandler("/Home/Error");
    // Sử dụng HTTP Strict Transport Security (HSTS) để ép trình duyệt dùng HTTPS
    app.UseHsts();
}

// Tự động chuyển hướng các request HTTP sang HTTPS
app.UseHttpsRedirection();

// Thiết lập định tuyến (Routing) để biết request nên được xử lý bởi endpoint nào
app.UseRouting();

// Kiểm tra người dùng là ai (Xác thực) - BẮT BUỘC phải đặt TRƯỚC Authorization
app.UseAuthentication();
// Kiểm tra người dùng có quyền truy cập tài nguyên hay không (Phân quyền)
app.UseAuthorization();

// Phục vụ các file tĩnh (CSS, JS, Hình ảnh...) và tối ưu hóa chúng
app.MapStaticAssets();


// --------------------------------------------------------
// 5. ĐỊNH TUYẾN CUỐI (ENDPOINTS) & CHẠY ỨNG DỤNG
// --------------------------------------------------------
// Định nghĩa Route mặc định cho Controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}") // Nếu không gõ URL cụ thể, mặc định vào HomeController và Index Action
    .WithStaticAssets();

// Ánh xạ các Razor Pages (cần thiết để các trang Login/Register của Identity hoạt động)
app.MapRazorPages();

// Khởi chạy ứng dụng web và lắng nghe các request tới
app.Run();