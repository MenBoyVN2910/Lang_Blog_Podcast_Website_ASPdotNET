using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore; // SỬA LỖI: Thiếu thư viện này sẽ không gọi được ToListAsync()
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;


namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    public class StoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StoryController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        // =================================================================
        // 1. GET: /Story/Submit (SỬA LỖI ĐÃ THÊM ASYNC TASK)
        // =================================================================
        [HttpGet]
        public async Task<IActionResult> Submit()
        {
            // Đổ danh sách danh mục từ DB vào ViewBag để bên View có dữ liệu render Dropdown
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View();
        }

        // =================================================================
        // GET: /Story/ (HIỂN THỊ DANH SÁCH BÀI VIẾT + LỌC DANH MỤC)
        // =================================================================
        public async Task<IActionResult> Index(int? categoryId, string searchString)
        {
            // 1. Lấy danh sách danh mục truyền sang View để nạp vào Dropdown list
            ViewBag.Categories = await _db.Categories.ToListAsync();

            // Lưu lại giá trị đã chọn để hiển thị lại trên giao diện sau khi tải lại trang
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentSearch = searchString;

            // 2. Query danh sách bài viết (bạn có thể thêm điều kiện trạng thái đã duyệt tùy logic của bạn)
            var storiesQuery = _db.Stories.Include(s => s.Category).AsQueryable();

            // 3. Lọc theo Danh mục (nếu người dùng có chọn)
            if (categoryId.HasValue)
            {
                storiesQuery = storiesQuery.Where(s => s.CategoryId == categoryId.Value);
            }

            // 4. Lọc theo Tên bài viết (nếu người dùng có nhập từ khóa)
            if (!string.IsNullOrEmpty(searchString))
            {
                // Lọc không phân biệt chữ hoa chữ thường
                storiesQuery = storiesQuery.Where(s => s.Title.Contains(searchString));
            }

            // Sắp xếp bài viết mới nhất lên đầu (tùy chọn)
            storiesQuery = storiesQuery.OrderByDescending(s => s.CreatedAt);

            return View(await storiesQuery.ToListAsync());
        }

        // =================================================================
        // 3. POST: /Story/Submit (XỬ LÝ LƯU CÂU CHUYỆN)
        // =================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(StoryUploadViewModel model)
        {
            ModelState.Remove("IssueNumber");
            // 💡 ĐÃ XÓA: ModelState.Remove("ImageFile"); (Không được xóa lỗi ảnh nữa vì bây giờ ảnh là bắt buộc)

            // 1. RÀNG BUỘC BẮT BUỘC CÓ ẢNH BÌA
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn ảnh bìa cho câu chuyện của bạn!");
            }

            // 2. RÀNG BUỘC KIỂM TRA TRÙNG TÊN TIÊU ĐỀ
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                // Kiểm tra xem trong DB đã có tiêu đề này chưa (so sánh không phân biệt hoa thường và bỏ khoảng trắng thừa)
                bool isTitleExist = await _db.Stories.AnyAsync(s => s.Title.Trim().ToLower() == model.Title.Trim().ToLower());

                if (isTitleExist)
                {
                    ModelState.AddModelError("Title", "Tiêu đề này đã tồn tại, vui lòng đặt một tên khác!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string stringFileName = "#";

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        stringFileName = UploadFile(model.ImageFile, "uploads/images");
                    }

                    Story newStory = new Story
                    {
                        Title = model.Title,
                        Content = model.Content,
                        Author = model.Author.Trim(),
                        CategoryId = model.CategoryId,
                        ImagePath = stringFileName,
                        Status = StoryStatus.Pending,
                        CreatedAt = DateTime.Now
                    };

                    _db.Stories.Add(newStory);
                    await _db.SaveChangesAsync();

                    // THÀNH CÔNG: Gán thông báo vào ViewBag để hiển thị trên trang này
                    ViewBag.SuccessMessage = "Mảnh ghép của bạn đã được gửi thành công và đang chờ ban biên tập LẶNG. đón nhận.";

                    // Nạp lại danh mục và trả về View để hiển thị Popup (JS sẽ lo phần chuyển trang sau)
                    ViewBag.Categories = await _db.Categories.ToListAsync();
                    return View(model);
                }
                catch (Exception)
                {
                    // LỖI HỆ THỐNG (Rớt mạng, lỗi DB...): Giữ nguyên trang và thông báo
                    ViewBag.ErrorMessage = "Đã xảy ra sự cố. Lời tự sự của bạn chưa thể gửi đi lúc này, xin hãy thử lại sau.";
                }
            }
            else
            {
                // LỖI NHẬP LIỆU (Thiếu trường bắt buộc hoặc trùng tên): Giữ nguyên trang và thông báo
                // 💡 Cập nhật nhẹ câu chữ báo lỗi để bao quát được cả lỗi trùng tên
                ViewBag.ErrorMessage = "Vui lòng kiểm tra lại. Một vài thông tin quan trọng đang bị bỏ trống hoặc không hợp lệ.";
            }

            // BẮT BUỘC: Khi trả về lại Form do lỗi, nạp lại Categories
            ViewBag.Categories = await _db.Categories.ToListAsync();

            // Trả về View cùng với 'model' hiện tại -> Toàn bộ text người dùng gõ sẽ ĐƯỢC GIỮ NGUYÊN
            return View(model);
        }

        // =================================================================
        // GET: /Story/Details/5 (HIỂN THỊ CHI TIẾT BÀI VIẾT)
        // =================================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var story = await _db.Stories
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (story == null)
            {
                return NotFound();
            }

            return View(story);
        }

        // =================================================================
        // HÀM HELPER: XỬ LÝ UPLOAD FILE (ẢNH)
        // =================================================================
        private string UploadFile(IFormFile file, string folderPath)
        {
            string fileName = null;
            if (file != null)
            {
                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, folderPath);
                if (!Directory.Exists(uploadDir))
                {
                    Directory.CreateDirectory(uploadDir);
                }
                fileName = Guid.NewGuid().ToString() + "-" + file.FileName;
                string filePath = Path.Combine(uploadDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                fileName = "/" + folderPath + "/" + fileName; // Trả về đường dẫn tương đối chuẩn chỉnh để lưu DB
            }
            return fileName;
        }
    }
}