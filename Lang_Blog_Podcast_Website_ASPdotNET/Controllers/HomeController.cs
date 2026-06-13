using Lang_Blog_Podcast_Website_ASPdotNET.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Lang_Blog_Podcast_Website_ASPdotNET.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


        public IActionResult About()
        {
            return View();
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
