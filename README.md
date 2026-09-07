# 🍃 LẶNG. — Nền Tảng Blog, Podcast & Tạp Chí Nghệ Thuật Trực Tuyến

<p align="center">
  <img src="Lang_Blog_Podcast_Website_ASPdotNET/wwwroot/images/Logo/logo_website.png" alt="LẶNG. Logo" width="120" />
</p>

<p align="center">
  <em>"Nơi những tâm hồn tìm thấy nhau trong sự tĩnh lặng của ngôn từ."</em>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C# 13" />
  <img src="https://img.shields.io/badge/ASP.NET%20Core-MVC-blue?style=for-the-badge&logo=aspnetcore&logoColor=white" alt="ASP.NET Core MVC" />
  <img src="https://img.shields.io/badge/EF%20Core-10.0-purple?style=for-the-badge" alt="EF Core 10" />
  <img src="https://img.shields.io/badge/SQL%20Server-Database-CC292B?style=for-the-badge&logo=microsoftsqlserver&logoColor=white" alt="SQL Server" />
  <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="MIT License" />
</p>

---

## 🌟 Giới Thiệu Tổng Quan

**LẶNG.** là một nền tảng trực tuyến kết hợp giữa **Tự sự (Blog/Story)**, **Podcast nghệ thuật** và **Tạp chí số định kỳ (Digital Magazine)**. Website được xây dựng theo phong cách thiết kế tối giản, thanh lịch và giàu tính nghệ thuật (Minimalist Editorial Design), hướng tới trải nghiệm đọc sâu lắng và thưởng thức âm thanh liền mạch.

Dự án phát triển trên nền tảng **ASP.NET Core 10 (.NET 10)** tiên tiến nhất, tuân thủ mô hình kiến trúc MVC, bảo mật định danh bằng ASP.NET Core Identity và tích hợp trình phát nhạc toàn cục không gián đoạn (Global Audio Player) thông qua kiến trúc SPA điều hướng mượt mà.

---

## ✨ Tính Năng Nổi Bật

### 📖 1. Câu Chuyện & Tự Sự (Story / Blog)
- **Soạn thảo & Đăng tải**: Đăng tải câu chuyện với ảnh bìa, danh mục, số phát hành và nội dung tự sự.
- **Quy trình duyệt bài chuẩn toà soạn**:
  - `Pending`: Chờ ban biên tập xem xét.
  - `Approved`: Xuất bản chính thức ra trang chủ và chuyên mục.
  - `Rejected`: Từ chối kèm theo lý do cụ thể gửi về cho tác giả.
- **Cơ chế Duyệt bản sửa đổi (`PostRevision`)**:
  - Khi tác giả chỉnh sửa một bài đã xuất bản, hệ thống lưu bản chỉnh sửa vào kho chờ duyệt riêng (`PostRevision`).
  - Bài viết đang phát hành vẫn giữ nguyên nội dung hiển thị cho bạn đọc cho đến khi Admin duyệt bản sửa mới.

### 🎙️ 2. Podcast & Trình Phát Âm Thanh Toàn Cục (Global Audio Player)
- **Tải lên & Trích xuất Metadata tự động**: Tích hợp thư viện `TagLibSharp` tự động phân tích tệp âm thanh tải lên để trích xuất thời lượng (Duration), dung lượng và định dạng.
- **Trình phát toàn cục (`GlobalPlayer.js`)**:
  - Thanh phát nhạc gắn cố định dưới chân trang với đầy đủ chức năng: Play/Pause, tua thời gian, điều chỉnh âm lượng, hiển thị ảnh bìa và tác giả.
- **Điều hướng mượt mà không ngắt nhạc (`spa-router.js`)**:
  - Khi người dùng chuyển trang giữa Trang chủ, Bài viết, Tạp chí hay Cá nhân, trang web nạp nội dung ngầm (SPA transition) giúp **âm thanh podcast tiếp tục phát liên tục mà không bị gián đoạn hay tải lại trang**.

### 📰 3. Tạp Chí Nghệ Thuật Số (Digital Magazine)
- **Quản lý ấn phẩm (Issues)**: Xuất bản các số tạp chí theo mùa (Xuân, Hạ, Thu, Đông) kèm lời tựa và ảnh bìa ấn phẩm.
- **Bố cục trình bày đa dạng**: Hỗ trợ nhiều kiểu lưới hiển thị nghệ thuật: `tall`, `wide`, `minimal`, `short`, `tall-offset`.
- **Thống kê lượt đọc**: Tự động ghi nhận lượt xem cho từng bài viết tạp chí.

### 👤 4. Trang Cá Nhân & Hệ Thống Thành Tựu (Profile & Gamification)
- **Tùy biến diện mạo**: Tải lên ảnh bìa (Cover) và ảnh đại diện (Avatar lưu trữ dưới dạng nhị phân/base64 an toàn).
- **Tab quản lý nội dung**: Phân tách rõ ràng giữa câu chuyện, podcast đã phát hành và mục bài viết yêu thích (Favorites).
- **Thành tựu vinh danh (Gamification Badges)**: Mở khóa các danh hiệu ("Người Kể Chuyện", "Thanh Âm Đêm", "Tâm Hồn Lan Tỏa"...) dựa trên hoạt động thực tế của thành viên.

### 📊 5. Bảng Điều Khiển Thành Viên (User Dashboard)
- Theo dõi trạng thái toàn bộ nội dung đã gửi (Đang chờ, Đã duyệt, Bị từ chối kèm phản hồi).
- Quản lý các bản sửa đổi đang chờ duyệt.
- Thống kê nhanh tổng số bài viết, podcast và tương tác cá nhân.

### 🛡️ 6. Bảng Quản Trị Ban Biên Tập (Admin Dashboard)
- **Phân quyền người dùng (Role Management)**: Cấp / Thu hồi quyền `Admin`, `Member`, `Creator` trực quan. Chặn cơ chế tự hạ quyền tránh khóa tài khoản Admin cuối cùng.
- **Duyệt nội dung tập trung**: Xem trước (Modal Preview), duyệt hoặc từ chối bài viết, podcast và bản sửa đổi.
- **Quản lý danh mục & Tạp chí**: Tạo mới danh mục, xuất bản số báo mới và biên tập bài viết tạp chí.
- **Dọn rác tự động (`DeletePhysicalFile`)**: Khi bài viết/podcast bị xóa hoặc người dùng đổi ảnh/audio mới, tệp tin vật lý cũ trong `wwwroot` được dọn dẹp triệt để, chống phình dung lượng lưu trữ máy chủ.

### 🔔 7. Hệ Thống Thông Báo Nội Bộ (In-App Notifications)
- Tự động thông báo tới tác giả ngay khi bài được duyệt hoặc bị từ chối.
- Thông báo tới toàn bộ ban biên tập khi có bài gửi mới.
- Menu chuông thông báo trực quan trên thanh Header, hỗ trợ đánh dấu đã đọc và xem lịch sử.

---

## 🛠️ Ngăn Xếp Công Nghệ (Tech Stack)

| Thành phần | Công nghệ / Thư viện sử dụng |
|---|---|
| **Framework Chính** | ASP.NET Core 10 (Target Framework: `net10.0`) |
| **Ngôn ngữ** | C# 13 (Kích hoạt `Nullable` enable, `ImplicitUsings`) |
| **Xác thực & Phân quyền** | ASP.NET Core Identity (Hỗ trợ Cookie Auth, Roles, Extensible OAuth) |
| **ORM & Database** | Entity Framework Core 10, Microsoft SQL Server |
| **Xử lý Audio** | `TagLibSharp` (Phân tích metadata và thời lượng âm thanh) |
| **Phân trang** | `X.PagedList.Mvc.Core` |
| **Giao diện & Styling** | Vanilla CSS3 (CSS Variables, Flexbox, CSS Grid, Glassmorphism, BEM), Bootstrap Icons / FontAwesome 6 |
| **Hiệu ứng & Hoạt cảnh** | AOS (Animate On Scroll), SweetAlert2 |
| **Frontend Scripting** | Vanilla JavaScript ES6+ (`GlobalPlayer.js`, `spa-router.js`) |

---

## 📁 Cấu Trúc Thư Mục Dự Án

```text
Lang_Blog_Podcast_Website_ASPdotNET/
├── .gitignore                                      # Quy tắc loại trừ tệp tạm, cache build
├── README.md                                       # Tài liệu hướng dẫn dự án
├── Lang_Blog_Podcast_Website_ASPdotNET.slnx        # Solution file .NET hiện đại
└── Lang_Blog_Podcast_Website_ASPdotNET/            # Thư mục mã nguồn chính
    ├── Areas/Identity/                             # Giao diện Đăng nhập, Đăng ký, Quản lý tài khoản
    ├── Controllers/                                # Bộ điều hướng nghiệp vụ
    │   ├── AdminController.cs                      # Ban biên tập & Quản trị hệ thống
    │   ├── HomeController.cs                       # Trang chủ, Giới thiệu, Bắt lỗi
    │   ├── MagazineController.cs                   # Khám phá Tạp chí số
    │   ├── NotificationController.cs               # API thông báo nội bộ
    │   ├── PodcastController.cs                    # Chuyên mục Podcast
    │   ├── ProfileController.cs                    # Trang cá nhân thành viên
    │   ├── StoryController.cs                      # Chuyên mục Câu chuyện & Tự sự
    │   └── UserDashboardController.cs              # Bảng điều khiển người dùng
    ├── Data/                                       # Tầng dữ liệu & DbContext
    │   ├── ApplicationDbContext.cs                 # Entity Framework DbContext
    │   ├── ApplicationUser.cs                      # Mở rộng Identity User (Avatar, Bio, FullName)
    │   └── DbInitializer.cs                        # Tự động Seed Roles & Categories mặc định
    ├── Migrations/                                 # Lịch sử EF Core Migrations (Code-First)
    ├── Models/                                     # Entities & ViewModels
    ├── Services/                                   # Các dịch vụ dùng chung (NotificationService)
    ├── Views/                                      # Giao diện Razor Views
    ├── wwwroot/                                    # Tài nguyên tĩnh
    │   ├── audio/                                  # Tệp âm thanh Podcast mẫu
    │   ├── css/                                    # Mã nguồn CSS tách theo module
    │   ├── images/                                 # Logo, banner, ảnh bìa, fallback mặc định
    │   ├── js/                                     # GlobalPlayer.js, spa-router.js
    │   └── uploads/                                # Thư mục lưu trữ tệp người dùng tải lên
    ├── appsettings.json                            # Cấu hình ứng dụng & Chuỗi kết nối
    └── appsettings.Example.json                    # Tệp cấu hình mẫu cho nhà phát triển mới
```

---

## 🚀 Hướng Dẫn Cài Đặt & Khởi Chạy

### 1. Yêu Cầu Môi Trường
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (hoặc phiên bản mới nhất tương thích).
- [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (SQL Server Express hoặc SQL Server LocalDB).
- [Visual Studio 2022 / 2025](https://visualstudio.microsoft.com/) hoặc [VS Code](https://code.visualstudio.com/) / [JetBrains Rider].

### 2. Tải Mã Nguồn
```bash
git clone https://github.com/MenBoyVN2910/Lang_Blog_Podcast_Website_ASPdotNET.git
cd Lang_Blog_Podcast_Website_ASPdotNET
```

### 3. Thiết Lập Chuỗi Kết Nối Cơ Sở Dữ Liệu
Mở tệp `Lang_Blog_Podcast_Website_ASPdotNET/appsettings.json` và điều chỉnh chuỗi kết nối `DefaultConnection` phù hợp với máy chủ SQL Server của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=Lang_BlogPodcastDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```
*(Nếu sử dụng SQL LocalDB: `Server=(localdb)\\mssqllocaldb;Database=Lang_BlogPodcastDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True`)*

### 4. Khởi Tạo Cơ Sở Dữ Liệu (Migration)
Mở cửa sổ dòng lệnh tại thư mục chứa file dự án `.csproj` và chạy:
```bash
dotnet ef database update
```
> **Lưu ý**: Lệnh này sẽ tự động tạo cấu trúc toàn bộ các bảng trong CSDL `Lang_BlogPodcastDb`.

### 5. Khởi Chạy Ứng Dụng
```bash
dotnet run
```
Sau khi chạy thành công, mở trình duyệt và truy cập:
- `https://localhost:7227` hoặc cổng hiển thị trên cửa sổ dòng lệnh.

Khi ứng dụng khởi chạy lần đầu tiên, lớp `DbInitializer` sẽ **tự động khởi tạo sẵn các nhóm quyền (`Admin`, `Member`, `Creator`)** và **các danh mục bài viết mẫu** để hệ thống hoạt động ngay lập tức mà không gặp bất kỳ lỗi thiếu dữ liệu nào.

---

## 🔑 Thiết Lập Quyền Quản Trị Viên (Admin)

1. Truy cập vào trang web và nhấn **Tạo tài khoản mới**.
2. Điền thông tin đăng ký một tài khoản bất kỳ (ví dụ: `admin@lang.vn`).
3. Mở **SQL Server Management Studio (SSMS)** hoặc **Azure Data Studio**, thực thi câu truy vấn để cấp quyền Admin cho tài khoản vừa tạo:
```sql
USE Lang_BlogPodcastDb;

-- Gán quyền Admin cho tài khoản dựa vào Email
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id 
FROM AspNetUsers u, AspNetRoles r
WHERE u.Email = 'admin@lang.vn' AND r.Name = 'Admin';
```
4. Đăng xuất và đăng nhập lại. Menu **BAN BIÊN TẬP** sẽ xuất hiện trên thanh điều hướng để bạn truy cập trang quản trị `/Admin`.
5. Từ thời điểm này, bạn có thể dễ dàng cấp/thu hồi quyền Admin cho các tài khoản khác trực tiếp từ giao diện trang quản trị mà không cần can thiệp SQL nữa.

---

## 🤝 Đóng Góp (Contributing)

Mọi ý kiến đóng góp, báo lỗi hoặc yêu cầu tính năng mới đều được hoan nghênh:
1. Fork dự án.
2. Tạo nhánh tính năng mới (`git checkout -b feature/TinhNangMoi`).
3. Commit các thay đổi (`git commit -m 'Thêm tính năng mới'`).
4. Đẩy lên nhánh (`git push origin feature/TinhNangMoi`).
5. Mở một **Pull Request**.

---

## 📄 Giấy Phép (License)

Dự án được phát hành theo giấy phép [MIT License](LICENSE). Tự do sử dụng cho mục đích học tập và phát triển cá nhân.

---

<p align="center">
  Được phát triển với 💖 bởi <strong>MenBoyVN2910</strong>
</p>