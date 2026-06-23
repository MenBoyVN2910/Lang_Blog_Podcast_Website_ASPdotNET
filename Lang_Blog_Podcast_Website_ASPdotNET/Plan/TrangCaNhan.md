# Prompt Thiết Kế & Lập Trình Trang Cá Nhân (Blog & Podcast)

**Role (Vai trò):**
Đóng vai một Chuyên gia Phân tích Hệ thống (System Analyst), Kỹ sư Frontend (sử dụng Next.js và Tailwind CSS), và Chuyên gia UI/UX.

**Context (Ngữ cảnh):**
Tôi đang phát triển một hệ thống nền tảng Blog & Podcast truyền cảm hứng. Tôi cần bạn thiết kế cấu trúc giao diện và viết mã nguồn cho tính năng "Trang Cá Nhân" (Profile Page) dành cho cả 2 đối tượng: User (Người dùng/Nhà sáng tạo) và Admin (Quản trị viên).

**Task (Yêu cầu công việc):**
Dựa vào các thành phần dưới đây, hãy tạo ra một bản thiết kế UI/UX hiện đại, responsive và viết mã code Frontend tương ứng (ưu tiên cấu trúc Component của Next.js và style bằng Tailwind CSS).

---

## 1. Yêu Cầu Component: Dropdown Menu (Navbar)
Dropdown menu xuất hiện ở góc phải Navbar khi người dùng click vào Ảnh đại diện. Cần hiển thị logic dựa theo `Role`:

* **Nếu Role = User:**
    * Trang cá nhân (Profile).
    * Viết bài / Đăng Podcast (Action button).
    * Nội dung yêu thích (Đã lưu / Đã thích).
    * Cài đặt tài khoản.
    * Đăng xuất.
* **Nếu Role = Admin:**
    * Trang cá nhân (Có kèm Badge/Huy hiệu "Admin").
    * Bảng điều khiển (Dashboard - Quản trị hệ thống).
    * Quản lý nội dung tổng.
    * Cài đặt hệ thống.
    * Đăng xuất.

## 2. Cấu Trúc Trang Cá Nhân (Profile Page)

### 2.1. Khu vực Header (Hero Profile) & Giới thiệu
* **Ảnh bìa (Cover Image):** Tỷ lệ 16:9, bao phủ phần trên cùng.
* **Ảnh đại diện (Avatar):** Bo tròn, đặt đè lên góc dưới của ảnh bìa.
* **Thông tin định danh:**
    * Tên hiển thị & Tên tài khoản (`@username`).
    * Huy hiệu phân quyền (Admin / Creator / User).
* **Khu vực "Câu Chuyện Của Tôi":**
    * Câu slogan cá nhân nổi bật.
    * Đoạn Bio (Giới thiệu ngắn) để chia sẻ hành trình và suy nghĩ.
* **Nút Hành động:** "Theo dõi" (nếu là khách) hoặc "Chỉnh sửa hồ sơ" (nếu là chủ sở hữu).

### 2.2. Khu Vực Thống Kê & Thành Tựu (Gamification)
Thiết kế một dải băng (ribbon) hoặc thẻ (card) trực quan ngay dưới phần Header:
* **Thống kê:** Bài viết đã đăng, Podcast đã phát hành, Lượt thích nhận được, Người theo dõi.
* **Thành tựu (Badges):** Các icon sáng lên khi người dùng đạt cột mốc: Podcast đầu tiên, 100 lượt nghe đầu tiên, 1000 lượt nghe, 10 bài viết được xuất bản.

### 2.3. Khu Vực Hiển Thị Nội Dung (Tab Navigation)
Sử dụng UI dạng Tabs để người dùng chuyển đổi nội dung mượt mà, tránh trang quá dài:

* **Tab 1: Podcast Của Tôi**
    * Danh sách dạng Grid/List hiển thị: Ảnh bìa podcast, Tiêu đề, Ngày đăng, Lượt nghe, Trạng thái (Public/Draft). Cần có nút "Play" trực tiếp.
* **Tab 2: Bài Viết Của Tôi**
    * Hiển thị dạng Card: Ảnh đại diện bài viết, Tiêu đề, Danh mục, Lượt xem, Ngày đăng.
* **Tab 3: Nội Dung Yêu Thích (Chỉ chủ sở hữu mới thấy)**
    * Chứa danh sách: Podcast đã thích, Bài viết đã lưu, Danh sách phát cá nhân.

### 2.4. Khu Vực Cài Đặt Tài Khoản
Thiết kế như một Modal Popup hoặc một trang con (`/settings`) bao gồm các form:
* Chỉnh sửa hồ sơ (Bio, Slogan).
* Đổi ảnh đại diện / Ảnh bìa.
* Đổi mật khẩu.
* Quản lý thông báo.

---

**Output Constraints (Yêu cầu đầu ra):**
1. Trình bày cấu trúc Layout chi tiết bằng chữ trước khi code.
2. Viết mã nguồn React/Next.js sử dụng Tailwind CSS cho trang này.
3. Code cần chú ý đến Mobile Responsive (hiển thị tốt trên điện thoại) và có các hiệu ứng hover, transition mượt mà.