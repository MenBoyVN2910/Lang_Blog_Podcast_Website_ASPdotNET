using Microsoft.AspNetCore.Mvc;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    public class MagazineController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
