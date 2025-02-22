using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace backup_website.Controllers
{
    public class BackupWebsiteController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IConfiguration _configuration;
        private readonly string _BaseUrlUrud;

        public BackupWebsiteController(ApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
            _BaseUrlUrud = _configuration["ApiSettings:BaseUrlUrud"] ?? throw new ArgumentNullException(nameof(_BaseUrlUrud), "API URL not found in appsettings.json");
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
                    item.category_name = "Unknown";
                }
            }

            return View(sansiriUrls);
        }


        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int url_id, bool is_active)
        {
            using (var httpClient = new HttpClient())
            {
                var apiUrl = $"{_BaseUrlUrud}put-tb-sansiri-url";
                var token = _configuration["ApiSettings:SansiriApiToken"];

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

        [HttpPost]
        public async Task<IActionResult> DeleteUrl(int url_id)
        {

            using (var httpClient = new HttpClient())
            {
                var apiUrl = $"{_BaseUrlUrud}put-tb-sansiri-url";
                var token = _configuration["ApiSettings:SansiriApiToken"];

                var requestData = new
                {
                    url_id = url_id,
                    is_delete = 1
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                httpClient.DefaultRequestHeaders.Add("token", token);

                var response = await httpClient.PutAsync(apiUrl, content);
                var responseText = await response.Content.ReadAsStringAsync();


                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, error = responseText });
                }
            }
        }

    }
}
