using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lang_Blog_Podcast_Website_ASPdotNET.Data;
using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    public class MagazineController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MagazineController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var issues = await _db.MagazineIssues
                .Where(m => m.Status == StoryStatus.Approved)
                .OrderByDescending(m => m.PublishDate)
                .ToListAsync();
            return View(issues);
        }

        public async Task<IActionResult> IssueDetails(int id)
        {
            var issue = await _db.MagazineIssues
                .Include(m => m.Articles)
                .ThenInclude(a => a.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.Status == StoryStatus.Approved);

            if (issue == null) return NotFound();

            return View(issue);
        }

        public async Task<IActionResult> ArticleDetails(int id)
        {
            var article = await _db.MagazineArticles
                .Include(a => a.Category)
                .Include(a => a.Issue)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null || article.Issue?.Status != StoryStatus.Approved) return NotFound();

            // Tăng lượt xem
            article.ViewCount++;
            await _db.SaveChangesAsync();

            return View(article);
        }
    }
}
