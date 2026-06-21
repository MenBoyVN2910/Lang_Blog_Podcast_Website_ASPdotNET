# Kế hoạch tích hợp Hotwire Turbo (Turbo Drive) cho trải nghiệm SPA

Tài liệu này mô tả kế hoạch tích hợp thư viện **Hotwire Turbo (Turbo Drive)** vào ứng dụng để mang lại trải nghiệm chuyển trang mượt mà, không bị giật màn hình hoặc tải lại toàn bộ trang (giống như một ứng dụng di động/Single Page Application).

## Nguyên lý hoạt động của Turbo Drive
- Turbo Drive sẽ tự động chặn (intercept) tất cả các lượt nhấp chuột vào liên kết `<a>` và lượt gửi form `<form>`.
- Thay vì để trình duyệt tải lại trang theo cách truyền thống, Turbo sẽ gửi yêu cầu bằng Fetch API (AJAX), nhận kết quả HTML và tiến hành thay thế phần nội dung `<body>` hiện tại bằng nội dung mới.
- Đồng thời, Turbo sẽ giữ nguyên cấu trúc `<head>`, các tệp CSS, và các file JS đã tải, giúp giảm thiểu tối đa băng thông và thời gian xử lý của trình duyệt.

## Các điều chỉnh kỹ thuật cần thiết để tương thích
Khi sử dụng Turbo Drive, sự kiện `DOMContentLoaded` truyền thống của trình duyệt chỉ kích hoạt 1 lần duy nhất ở lượt tải trang đầu tiên. Do đó, các đoạn script khởi tạo giao diện ở các trang con (như AOS, TinyMCE, SweetAlert2, các event listener cho nút bấm) cần được chuyển sang lắng nghe sự kiện `turbo:load` của Turbo để đảm bảo chúng luôn chạy lại mỗi khi trang con được thay đổi nội dung.

---

## Danh sách các thay đổi chi tiết

### [Views/Shared]

#### [MODIFY] [_Layout.cshtml](file:///d:/DEV/DEV_Source_Project/2.Develoment_Project/Lang_ASPdotNET/Lang_Blog_Podcast_Website_ASPdotNET/Views/Shared/_Layout.cshtml)
- Nạp thư viện Hotwire Turbo ở phần đầu trang:
  ```html
  <script src="https://unpkg.com/@hotwired/turbo@7.8.0/dist/turbo.es2017-umd.js" defer></script>
  ```

---

### [Views/Home]

#### [MODIFY] [Index.cshtml](file:///d:/DEV/DEV_Source_Project/2.Develoment_Project/Lang_ASPdotNET/Lang_Blog_Podcast_Website_ASPdotNET/Views/Home/Index.cshtml)
- Chuyển `AOS.init` sang chạy trong sự kiện `turbo:load`.

#### [MODIFY] [About.cshtml](file:///d:/DEV/DEV_Source_Project/2.Develoment_Project/Lang_ASPdotNET/Lang_Blog_Podcast_Website_ASPdotNET/Views/Home/About.cshtml)
- Chuyển `AOS.init` sang chạy trong sự kiện `turbo:load`.

---

### [Views/Magazine]

#### [MODIFY] [Index.cshtml](file:///d:/DEV/DEV_Source_Project/2.Develoment_Project/Lang_ASPdotNET/Lang_Blog_Podcast_Website_ASPdotNET/Views/Magazine/Index.cshtml)
- Chuyển `AOS.init` sang chạy trong sự kiện `turbo:load`.

---

### [Views/Podcast]

#### [MODIFY] [Index.cshtml](file:///d:/DEV/DEV_Source_Project/2.Develoment_Project/Lang_ASPdotNET/Lang_Blog_Podcast_Website_ASPdotNET/Views/Podcast/Index.cshtml)
- Chuyển `AOS.init` sang chạy trong sự kiện `turbo:load`.

---

### [Views/Story]

#### [MODIFY] [Index.cshtml](file:///d:/DEV/DEV_Source_Project/2.Develoment_Project/Lang_ASPdotNET/Lang_Blog_Podcast_Website_ASPdotNET/Views/Story/Index.cshtml)
- Chuyển `AOS.init` sang chạy trong sự kiện `turbo:load`.

#### [MODIFY] [Submit.cshtml](file:///d:/DEV/DEV_Source_Project/2.Develoment_Project/Lang_ASPdotNET/Lang_Blog_Podcast_Website_ASPdotNET/Views/Story/Submit.cshtml)
- Trước khi khởi tạo TinyMCE, cần dọn dẹp các editor instance cũ (`tinymce.remove()`) để tránh lỗi xung đột cache của trình soạn thảo khi chuyển trang qua lại.
- Chuyển toàn bộ script khởi tạo TinyMCE và SweetAlert2 sang sự kiện `turbo:load`.

---

### [Views/Admin]

#### [MODIFY] [Index.cshtml](file:///d:/DEV/DEV_Source_Project/2.Develoment_Project/Lang_ASPdotNET/Lang_Blog_Podcast_Website_ASPdotNET/Views/Admin/Index.cshtml)
- Chuyển logic mở Modal và thiết lập tab sang sự kiện `turbo:load`.

---

## Kế hoạch kiểm thử & Xác thực

### Kiểm tra tự động
- Chạy lệnh `dotnet build` để xác nhận toàn bộ mã nguồn ứng dụng biên dịch thành công.

### Kiểm tra thủ công
1. Khởi động ứng dụng, di chuyển qua lại giữa các menu trang chủ, câu chuyện, tạp chí, podcast để kiểm tra tốc độ chuyển trang nhanh chóng và không có hiện tượng giật màn hình (trình duyệt không hiển thị icon loading vòng tròn truyền thống).
2. Kiểm tra hiệu ứng hoạt họa AOS ở các trang con xem có chạy bình thường khi cuộn chuột sau mỗi lần chuyển trang hay không.
3. Kiểm tra trình soạn thảo TinyMCE ở trang gửi câu chuyện xem có khởi chạy lại bình thường khi điều hướng tới trang đó không.
