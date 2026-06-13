using Microsoft.AspNetCore.Mvc;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    public class StoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
