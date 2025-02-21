using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace backup_website.Controllers
{
    public class BackupWebsiteController : Controller
    {
        private readonly ApiService _apiService;

        public BackupWebsiteController(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// ✅ โหลดข้อมูลจาก `get-tb-sansiri-url-log`
        public async Task<IActionResult> DownloadHistory()
        {
            var data = await _apiService.GetDataAsync();
            return View(data);
        }

        /// ✅ โหลดข้อมูลจาก `get-tb-sansiri-url`
        public async Task<IActionResult> DownloadLinks()
        {
            var sansiriUrls = await _apiService.GetSansiriUrlsAsync();
            var categories = await _apiService.GetTableUrlCategory();

            // ทำการ Map id_category_url -> name
            var categoryDict = categories.ToDictionary(c => c.id_category_url, c => c.name ?? "Unknown");

            // ใส่ category_name เข้าไปใน sansiriUrls
            foreach (var item in sansiriUrls)
            {
                if (item.id_category_url.HasValue && categoryDict.ContainsKey(item.id_category_url.Value))
                {
                    item.category_name = categoryDict[item.id_category_url.Value];
                }
                else
                {
                    item.category_name = "Unknown"; // เผื่อกรณีหาไม่เจอ
                }
            }

            return View(sansiriUrls);
        }


        /// ✅ อัปเดตสถานะ `is_active`
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int url_id, bool is_active)
        {
            using (var httpClient = new HttpClient())
            {
                var apiUrl = "http://prd-apigateway.sansiri.com/crud/put-tb-sansiri-url";
                var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjQ5MzEwODZjLWY0NjMtNGQ4Ni04ZTVjLTU2NmVmODhlMjVkZiIsImlhdCI6MTU0OTk1MTA4N30.ypF3f7RwVbTJ1_0UWCDszf0DJd1upvssZ5ecXgjzqPU";

                var requestData = new
                {
                    url_id = url_id,
                    is_active = is_active ? 1 : 0
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Add("token", token);

                var response = await httpClient.PutAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return Json(new { success = false, error = errorResponse });
                }
            }
        }
    }
}
