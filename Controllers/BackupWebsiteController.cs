using Microsoft.AspNetCore.Mvc;

namespace backup_website.Controllers
{
    public class BackupWebsiteController : Controller
    {
        public IActionResult DownloadHistory()
        {
            return View();
        }
    }
}
