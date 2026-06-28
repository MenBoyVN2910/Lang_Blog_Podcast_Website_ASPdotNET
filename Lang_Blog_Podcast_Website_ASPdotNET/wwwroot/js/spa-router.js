document.addEventListener("DOMContentLoaded", () => {
    // 1. Khởi tạo AOS cho lần tải trang đầu tiên (Cold Load)
    if (typeof AOS !== 'undefined') {
        AOS.init({
            once: true,
            offset: 50,
            easing: 'ease-out-cubic'
        });
    }

    // Hàm cốt lõi: Tải HTML mới và thay thế nội dung mượt mà
    async function navigateTo(url) {
        try {
            const response = await fetch(url);
            const htmlString = await response.text();

            // Phân tích cú pháp HTML vừa lấy về
            const parser = new DOMParser();
            const newDocument = parser.parseFromString(htmlString, "text/html");

            const currentMain = document.querySelector("main.main-content");
            const newMain = newDocument.querySelector("main.main-content");

            if (currentMain && newMain) {
                // Xoá các style của trang cũ
                document.querySelectorAll('.spa-page-style').forEach(el => el.remove());

                // Lấy và thêm các style của trang mới
                const newStyles = newDocument.querySelectorAll('link[rel="stylesheet"]:not([data-layout-style="true"])');
                newStyles.forEach(style => {
                    const clone = document.createElement('link');
                    Array.from(style.attributes).forEach(attr => {
                        clone.setAttribute(attr.name, attr.value);
                    });
                    clone.classList.add('spa-page-style');
                    document.head.appendChild(clone);
                });

                // Thay thế nội dung vùng chính
                currentMain.innerHTML = newMain.innerHTML;

                // Cập nhật Title trang trên tab trình duyệt
                document.title = newDocument.title;

                // Cập nhật Class của Header & Footer để giữ nguyên logic đổi màu giao diện
                const currentHeader = document.querySelector("header.main-header");
                const newHeader = newDocument.querySelector("header.main-header");
                if (currentHeader && newHeader) currentHeader.className = newHeader.className;

                const currentFooter = document.querySelector("footer.site-footer");
                const newFooter = newDocument.querySelector("footer.site-footer");
                if (currentFooter && newFooter) currentFooter.className = newFooter.className;

                // Cập nhật thanh URL trình duyệt
                window.history.pushState({ path: url }, "", url);
                window.scrollTo(0, 0);

                // Lấy và chạy tuần tự các script đặc thù của trang mới
                const newScripts = newDocument.querySelectorAll('script:not([data-layout-script="true"])');
                async function loadScriptsSequentially(scripts) {
                    for (const script of scripts) {
                        await new Promise((resolve) => {
                            const clone = document.createElement('script');
                            Array.from(script.attributes).forEach(attr => {
                                clone.setAttribute(attr.name, attr.value);
                            });
                            
                            if (script.src) {
                                clone.onload = () => resolve();
                                clone.onerror = () => {
                                    console.error(`Failed to load script: ${script.src}`);
                                    resolve();
                                };
                            } else {
                                // Thay thế DOMContentLoaded bằng sự kiện đặc chế cho SPA Router
                                clone.textContent = script.textContent.replace(/['"]DOMContentLoaded['"]/g, "'spa-page-loaded'");
                            }
                            
                            document.body.appendChild(clone);
                            
                            if (!script.src) {
                                resolve();
                            }
                        });
                    }
                }
                
                // Khởi chạy script tuần tự rồi mới refresh AOS
                loadScriptsSequentially(newScripts).then(() => {
                    // Kích hoạt sự kiện spa-page-loaded để các Script cũ vốn lắng nghe DOMContentLoaded chạy bình thường
                    document.dispatchEvent(new Event('spa-page-loaded'));
                    window.dispatchEvent(new Event('spa-page-loaded'));

                    // KHẮC PHỤC LỖI MẤT BÀI VIẾT: Ép thư viện AOS quét lại và tính toán tọa độ cho HTML mới
                    if (typeof AOS !== 'undefined') {
                        setTimeout(() => {
                            AOS.refreshHard();
                        }, 100);
                    }
                });
            } else {
                window.location.href = url; // Fallback: Nếu cấu trúc trang mới lỗi, tải lại trang kiểu cũ
            }
        } catch (error) {
            window.location.href = url;
        }
    }

    // Bắt mọi sự kiện Click thẻ <a> trên toàn trang
    document.body.addEventListener("click", (e) => {
        const link = e.target.closest("a");
        if (link && link.href) {
            const currentHost = window.location.host;
            const linkHost = link.host;

            // Loại trừ các đường dẫn không áp dụng chuyển trang bằng JS
            if (link.target === "_blank" ||
                linkHost !== currentHost ||
                link.hasAttribute("download") ||
                link.getAttribute("href").startsWith("#") ||
                link.href.includes("/Identity/") ||
                link.href.includes("Logout")) {
                return;
            }

            // Ngăn chặn trình duyệt reload lại trang gây giật và tắt nhạc
            e.preventDefault();
            navigateTo(link.href);
        }
    });

    // Xử lý khi người dùng bấm nút Back / Forward trên trình duyệt
    window.addEventListener("popstate", () => {
        navigateTo(window.location.href);
    });
});

// ==================================================================================
// CÁC HÀM TOÀN CỤC (GLOBAL FUNCTIONS)
// Giữ nguyên ở ngoài phạm vi DOMContentLoaded để các thuộc tính onclick="..." ở HTML gọi được bình thường
// ==================================================================================

function showLoginAlert() {
    if (typeof Swal === 'undefined') return;
    Swal.fire({
        title: 'Một chút nhịp ngưng...',
        text: 'Bạn cần đăng nhập để có thể gửi gắm câu chuyện của mình. Hãy đăng nhập và trở lại đây nhé!',
        icon: 'info',
        showCancelButton: true,
        confirmButtonColor: '#B29B84',
        cancelButtonColor: '#2A2825',
        confirmButtonText: 'Đăng nhập',
        cancelButtonText: 'Lúc khác',
        background: '#1A1817',
        color: '#F5F1EE',
        customClass: {
            popup: 'dark-swal-popup'
        }
    }).then((result) => {
        if (result.isConfirmed) {
            window.location.href = '/Identity/Account/Login';
        }
    });
}

function playFromCard(btnEl) {
    var card = btnEl.closest('.premium-podcast-card');
    if (!card) return;
    var audioUrl = card.getAttribute('data-audio-url');
    var title = card.getAttribute('data-audio-title');
    var author = card.getAttribute('data-audio-author');
    var image = card.getAttribute('data-audio-image');
    var podcastId = card.getAttribute('data-podcast-id');

    // Hiệu ứng nút bấm phản hồi trực quan
    btnEl.classList.add('is-clicked');
    setTimeout(() => btnEl.classList.remove('is-clicked'), 300);

    if (typeof LangPlayer !== 'undefined') {
        LangPlayer.play(audioUrl, title, author, image, podcastId);
    }
}