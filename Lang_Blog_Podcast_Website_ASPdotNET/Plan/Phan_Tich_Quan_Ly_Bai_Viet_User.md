# PHÂN TÍCH & THIẾT KẾ HỆ THỐNG: MODULE "QUẢN LÝ BÀI VIẾT" DÀNH CHO USER
**Dự án:** LẶNG.
**Mục tiêu:** Cung cấp không gian làm việc cá nhân hóa cho User để sáng tạo và quản lý nội dung (Story & Podcast), đồng thời đảm bảo quy trình kiểm duyệt chặt chẽ từ Admin.

---

## 1. VỊ TRÍ & ĐIỀU HƯỚNG (NAVIGATION)
* **Điểm truy cập:** Nằm trong Dropdown List khi User click vào Avatar (chỉ hiển thị cho role `User`, ẩn với `Admin`).
* **Cấu trúc Dropdown:**
    * Cài Đặt
    * **Quản Lý Bài Viết** *(Mới)*
    * **Thông Báo** *(Mới - Phát triển tương lai)*
    * Đăng Xuất
* **Layout chung của trang Quản lý:** Khi click vào "Quản Lý Bài Viết", hệ thống chuyển sang một trang Dashboard dành riêng cho User. 
    * **Sidebar trái:** Chứa các tab: `Đăng Bài Mới`, `Câu Chuyện Của Tôi` (Quản lý bài viết), `Podcast Của Tôi` (Quản lý podcast).
    * **Main Content phải:** Khu vực làm việc tương ứng với tab được chọn. Background màu kem sữa `#FDFBF7` đặc trưng của dự án, text màu than đậm, font Serif cho tiêu đề để giữ nguyên vibe "chữa lành".

---

## 2. PHÂN TÍCH CHỨC NĂNG CHI TIẾT

### 2.1. Tab: Đăng Bài (Sáng tạo nội dung)
Khu vực này thay thế cho việc phải ra trang chủ để gửi bài.
* **Giao diện:** Chia làm 2 lựa chọn lớn (Radio button dạng thẻ hoặc Tabs): **[ Viết Câu Chuyện ]** và **[ Tải Lên Podcast ]**.
* **Form nhập liệu:**
    * **Story:** Tiêu đề, Upload ảnh cover, Khung soạn thảo văn bản Rich Text (hỗ trợ in nghiêng, bôi đậm, quote), chọn Danh mục (Chủ đề).
    * **Podcast:** Tiêu đề, Upload file audio (hiển thị player nghe thử), Ảnh cover, Đoạn mô tả ngắn (Show notes).
* **Khu vực "Trạng thái nội dung vừa gửi":**
    * Nằm ngay dưới form đăng bài. Hiển thị dưới dạng một Timeline hoặc List các thẻ (Cards).
    * **Nhãn trạng thái (Badge):** * 🟡 `Chờ Duyệt`: Khi vừa submit xong.
        * 🟢 `Đã Duyệt`: Admin đã approve. Đi kèm nút [ x ] nhỏ góc phải thẻ để User có thể tắt/ẩn thông báo này đi cho gọn màn hình.
        * 🔴 `Từ Chối`: Admin không duyệt (kèm text lý do ngắn bên dưới).

### 2.2. Tab: Quản Lý Bài Viết (Story) & 2.3. Tab: Quản Lý Podcast
* **Hiển thị:** Dạng bảng (Table) hoặc Danh sách thẻ (Grid Cards) tối giản.
* **Thông tin cột:** Ảnh thumbnail, Tiêu đề, Chủ đề, Ngày đăng, **Trạng thái** (Đang Live / Chờ Duyệt / Nháp), **Hành động**.
* **Hành động (CRUD):**
    * 👁️ **Xem:** Xem preview bài viết/podcast hiển thị trên trang chủ như thế nào.
    * ✏️ **Sửa:** Mở lại form giống lúc đăng bài với data cũ đã được fill sẵn.
    * 🗑️ **Xóa:** Xóa bài (Hiển thị popup confirm: "Bạn có chắc chắn muốn xóa kỷ niệm này không?").

---

## 3. LUỒNG XỬ LÝ RÀNG BUỘC & TRẠNG THÁI (STATE MACHINE)

Đây là phần quan trọng nhất trong logic hệ thống để đảm bảo tính nhất quán dữ liệu khi User chỉnh sửa bài đã duyệt.

**Giải pháp đề xuất (Kiến trúc "Shadow Copy" - Bản nháp tạm):**
Khi một bài viết đã `Public` (Đã duyệt), nếu User ấn Edit, hệ thống **không** sửa trực tiếp vào dòng dữ liệu đang Live. Thay vào đó, nó tạo ra một bản `Draft_Edit` (Bản nháp chỉnh sửa).

* **Góc nhìn của User:**
    * Bài gốc vẫn đang hiển thị bình thường trên trang chủ "LẶNG".
    * Trong trang "Quản Lý Bài Viết", tại thẻ bài viết đó xuất hiện nhãn 🟡 `Đang Chờ Duyệt (Bản Sửa)`.
    * Nếu Admin duyệt -> Hệ thống lấy nội dung bản `Draft_Edit` ghi đè lên bản `Live`, nhãn đổi thành 🟢 `Đã Sửa`.
    * Nếu Admin từ chối -> Bản `Live` giữ nguyên, bản `Draft_Edit` bị hủy, hiện thông báo 🔴 `Sửa Không Được Duyệt`.

* **Góc nhìn của Admin (Trang Ban Biên Tập):**
    * Admin nhận được request ở tab "Câu chuyện chờ duyệt".
    * Thẻ bài này sẽ có một Badge nổi bật: 🔵 `Bản Chỉnh Sửa` (Có thể thiết kế UI cho phép Admin bấm vào nút `[So sánh]` để xem đoạn text nào đã bị thay đổi so với bản cũ - một tính năng rất xịn cho hệ thống CMS).

---

## 4. HƯỚNG PHÁT TRIỂN TƯƠNG LAI: TRANG "THÔNG BÁO" (NOTIFICATIONS)

Việc thêm Dropdown "Thông báo" là một bước đi chuẩn xác để tăng tương tác (Retention Rate) cho hệ thống.

**Ý tưởng cho Hệ thống Thông báo (Notification System):**

1.  **Phân loại thông báo (Notification Types):**
    * 🔔 **System (Hệ thống):** "Bản cập nhật mới của LẶNG", "Bảo trì server", "Chào mừng bạn gia nhập LẶNG".
    * ✅ **Approval (Kiểm duyệt):** "Câu chuyện 'Cơn mưa đầu mùa' của bạn đã được duyệt và lên sóng", "Podcast của bạn cần chỉnh sửa lại chất lượng âm thanh (Từ chối)".
    * ❤️ **Interaction (Tương tác - Nếu có chức năng này):** "Có 5 người đã nghe và cảm thấy bình yên với Podcast của bạn", "Ai đó đã lưu lại bài viết của bạn".

2.  **Logic & Giao diện:**
    * **Dropdown mini:** Click vào chữ "Thông Báo" trên Header sẽ xổ xuống một bảng nhỏ (không cần chuyển trang) hiển thị 5 thông báo mới nhất.
    * **Chưa đọc / Đã đọc:** Thông báo chưa đọc có background hơi sẫm hoặc có chấm đỏ `dot` góc phải.
    * **Trang "Tất cả thông báo":** Dành cho ai muốn xem lịch sử (Pagination).
    * **Thông báo cho Admin:** "Tác giả [Minh Nhật] vừa gửi một trạm dừng chân (Podcast) mới", "Có 3 bài viết đang chờ bạn duyệt".

---

## 5. GỢI Ý DÀNH CHO DEVELOPER CHUẨN BỊ TRIỂN KHAI

* **Về Database (Backend):** Ở bảng `Posts` và `Podcasts`, cần thêm cột `Status` (enum: `PENDING`, `APPROVED`, `REJECTED`, `EDIT_PENDING`). Có thể cần một bảng phụ `Post_Revisions` để lưu các bản nháp chỉnh sửa đang chờ duyệt.
* **Về Frontend (React/Next/Vue):** * Sử dụng **Optimistic UI** cho thao tác tắt/ẩn thẻ thông báo (ấn tắt là ẩn luôn trên UI, gọi API chạy ngầm phía sau).
    * Hiệu ứng: Chuyển tab giữa Quản lý Story và Podcast nên dùng hiệu ứng fade in nhẹ nhàng (`transition-opacity duration-300`).
    * Màu sắc Badge: Nên dùng tone màu pastel (VD: Chờ duyệt = vàng nhạt, Đã duyệt = xanh mint nhạt) thay vì màu rực rỡ, để match với concept "LẶNG".
