using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace backup_website.Controllers
{
    public class BackupWebsiteController : Controller
    {
        private readonly ApiService _apiService;

        public BackupWebsiteController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> DownloadHistory()
        {
            // ✅ ดึงข้อมูลจาก API
            var data = await _apiService.GetDataAsync();

            // ✅ ส่งข้อมูลไปที่ View
            return View(data);
        }
    }
}
