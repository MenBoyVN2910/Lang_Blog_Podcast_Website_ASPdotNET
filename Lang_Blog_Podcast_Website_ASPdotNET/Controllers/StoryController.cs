using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    /// <summary>
    /// Controller quản lý luồng hiển thị câu chuyện của cộng đồng, gửi bài mới và xem chi tiết câu chuyện.
    /// </summary>
    public class StoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

        public StoryController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        /// <summary>
        /// GET: /Story/
        /// Hiển thị danh sách các câu chuyện đã được duyệt công khai kèm bộ lọc danh mục và tìm kiếm.
        /// </summary>
        /// <param name="categoryId">ID danh mục để lọc bài viết (tùy chọn)</param>
        /// <param name="searchString">Từ khóa tìm kiếm tiêu đề bài viết (tùy chọn)</param>
        public async Task<IActionResult> Index(int? categoryId, string searchString)
        {
            // 1. Tải danh sách danh mục đổ vào Dropdown chọn lọc trên giao diện
            ViewBag.Categories = await _db.Categories.ToListAsync();
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentSearch = searchString;

            // 2. Khởi tạo Query chỉ truy vấn những câu chuyện đã được duyệt (Approved) để tối ưu hiệu năng
            var storiesQuery = _db.Stories
                .Include(s => s.Category)
                .Include(s => s.User)
                .Where(s => s.Status == StoryStatus.Approved);

            // 3. Lọc theo danh mục nếu người dùng yêu cầu
            if (categoryId.HasValue)
            {
                storiesQuery = storiesQuery.Where(s => s.CategoryId == categoryId.Value);
            }

            // 4. Lọc theo từ khóa tìm kiếm tiêu đề
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                storiesQuery = storiesQuery.Where(s => s.Title.Contains(searchString));
            }

            // Sắp xếp bài viết mới nhất lên đầu tiên
            storiesQuery = storiesQuery.OrderByDescending(s => s.CreatedAt);

            return View(await storiesQuery.ToListAsync());
        }

        /// <summary>
        /// GET: /Story/Submit
        /// Hiển thị trang để người dùng soạn thảo và gửi câu chuyện mới lên ban biên tập.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Submit()
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View();
        }

        /// <summary>
        /// POST: /Story/Submit
        /// Xử lý dữ liệu form người dùng gửi câu chuyện mới lên (kèm tải ảnh bìa lên server).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(StoryUploadViewModel model)
        {
            // Loại bỏ kiểm tra độ hợp lệ của IssueNumber vì thuộc tính này do Admin gán khi duyệt bài
            ModelState.Remove("IssueNumber");

            // 1. Kiểm tra bắt buộc phải chọn ảnh bìa
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn ảnh bìa cho câu chuyện của bạn!");
            }

            // 2. Kiểm tra trùng lặp tiêu đề bài viết trong cơ sở dữ liệu
            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                bool isTitleExist = await _db.Stories.AnyAsync(s => s.Title.Trim().ToLower() == model.Title.Trim().ToLower());
                if (isTitleExist)
                {
                    ModelState.AddModelError("Title", "Tiêu đề này đã tồn tại, vui lòng chọn tiêu đề khác!");
                }
            }

            // 3. Nếu dữ liệu đầu vào hợp lệ, tiến hành lưu trữ
            if (ModelState.IsValid)
            {
                try
                {
                    string uploadedFileName = "#";
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        uploadedFileName = UploadFile(model.ImageFile, "uploads/images");
                    }

                    var newStory = new Story
                    {
                        Title = model.Title,
                        Content = model.Content,
                        Author = model.Author?.Trim() ?? "Ẩn danh",
                        UserId = _userManager.GetUserId(User), // Gán UserId của người đang đăng nhập
                        IssueNumber = string.IsNullOrWhiteSpace(model.IssueNumber) ? "None" : model.IssueNumber.Trim(),
                        CategoryId = model.CategoryId,
                        ImagePath = uploadedFileName,
                        Status = StoryStatus.Pending, // Mặc định ở trạng thái chờ duyệt
                        CreatedAt = DateTime.Now
                    };

                    _db.Stories.Add(newStory);
                    await _db.SaveChangesAsync();

                    ViewBag.SuccessMessage = "Mảnh ghép của bạn đã được gửi thành công và đang chờ ban biên tập LẶNG. đón nhận.";
                    
                    // Xóa trắng form sau khi gửi thành công để tránh gửi lặp dữ liệu
                    ModelState.Clear();
                    model = new StoryUploadViewModel();
                }
                catch (Exception)
                {
                    ViewBag.ErrorMessage = "Đã xảy ra sự cố hệ thống. Lời tự sự chưa thể gửi đi lúc này, vui lòng thử lại sau.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Vui lòng kiểm tra lại. Một vài thông tin quan trọng bị trống hoặc không hợp lệ.";
            }

            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View(model);
        }

        /// <summary>
        /// GET: /Story/Details/5
        /// Xem chi tiết nội dung của một câu chuyện. Chỉ cho phép xem nếu câu chuyện đã được phê duyệt.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 🛡️ BẢO MẬT: Chỉ cho phép công chúng xem bài viết có trạng thái Approved
            var story = await _db.Stories
                .Include(s => s.Category)
                .Include(s => s.User)
                .FirstOrDefaultAsync(m => m.Id == id && m.Status == StoryStatus.Approved);
                
            if (story == null)
            {
                return NotFound();
            }

            return View(story);
        }

        /// <summary>
        /// Phương thức nội bộ hỗ trợ tải tệp tin (Ảnh) lên server vật lý.
        /// </summary>
        private string UploadFile(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0) return null;

            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, folderPath);
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            string uniqueFileName = $"{Guid.NewGuid()}-{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return $"/{folderPath}/{uniqueFileName}";
        }
    }
}