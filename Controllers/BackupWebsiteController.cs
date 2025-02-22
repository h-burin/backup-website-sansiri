using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using backup_website.Models.Requests; // ✅ Import Model ใหม่

namespace backup_website.Controllers
{
    public class BackupWebsiteController : Controller
    {
        private readonly ApiService _apiService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _BaseUrlUrud;
        private readonly string _apiToken;

        public BackupWebsiteController(ApiService apiService, IConfiguration configuration, HttpClient httpClient)
        {
            _apiService = apiService;
            _configuration = configuration;
            _httpClient = httpClient;

            // ✅ โหลดค่าจาก `appsettings.json`
            _BaseUrlUrud = _configuration["ApiSettings:BaseUrlUrud"] ?? throw new ArgumentNullException(nameof(_BaseUrlUrud), "API URL not found in appsettings.json");
            _apiToken = _configuration["ApiSettings:SansiriApiToken"] ?? throw new ArgumentNullException(nameof(_apiToken), "API Token not found in appsettings.json");
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

            // ✅ Map id_category_url -> name
            var categoryDict = categories.ToDictionary(c => c.id_category_url, c => c.name ?? "Unknown");

            // ✅ ใส่ category_name เข้าไปใน sansiriUrls
            foreach (var item in sansiriUrls)
            {
                item.category_name = item.id_category_url.HasValue && categoryDict.ContainsKey(item.id_category_url.Value)
                    ? categoryDict[item.id_category_url.Value]
                    : "Unknown";
            }

            return View(sansiriUrls);
        }

        /// ✅ ฟังก์ชันกลาง ใช้ร่วมกันระหว่าง Update และ Delete
        private async Task<IActionResult> SendPutRequest(object requestData)
        {
            var apiUrl = $"{_BaseUrlUrud}put-tb-sansiri-url"; // ✅ ใช้ Base URL + Endpoint

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear(); // ✅ ล้าง Header ก่อนใส่ใหม่
            _httpClient.DefaultRequestHeaders.Add("token", _apiToken); // ✅ ใช้ Token ที่โหลดจาก `appsettings.json`

            var response = await _httpClient.PutAsync(apiUrl, content);
            var responseText = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? Json(new { success = true })
                : Json(new { success = false, error = responseText });
        }


        /// ✅ อัปเดตสถานะของ URL (เปิด/ปิด)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int url_id, bool is_active)
        {
            var requestData = new
            {
                url_id = url_id,
                is_active = is_active ? 1 : 0
            };

            return await SendPutRequest(requestData);
        }

        /// ✅ ลบ URL โดยส่ง `is_delete = 1`
        [HttpPost]
        public async Task<IActionResult> DeleteUrl(int url_id)
        {
            var requestData = new
            {
                url_id = url_id,
                is_delete = 1
            };

            return await SendPutRequest(requestData);
        }

        /// ✅ ดึง Categories มาแสดงใน Dropdown
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _apiService.GetTableUrlCategory();
            return Json(new { success = true, data = categories });
        }

        /// ✅ อัปเดต URL จาก Modal (ใช้ใน Edit)
        [HttpPost]
        public async Task<IActionResult> UpdateUrl([FromBody] UpdateUrlRequest request)
        {
            Console.WriteLine($"📌 Debug - ข้อมูลที่ได้รับจาก Frontend: {JsonSerializer.Serialize(request)}");

            return await SendPutRequest(request);
        }


    }
}
