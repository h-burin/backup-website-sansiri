using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using backup_website.Models.Requests;
using backup_website.Models.TableUrlCategory; // ✅ นำเข้า Model ที่ใช้รับค่าจาก Frontend

namespace backup_website.Controllers
{
    public class BackupWebsiteController : Controller
    {
        private readonly ApiService _apiService;  // ✅ ใช้เรียก API Service ที่ติดต่อกับ Database
        private readonly IConfiguration _configuration;  // ✅ โหลดค่าตั้งจาก `appsettings.json`
        private readonly HttpClient _httpClient;  // ✅ ใช้สำหรับทำ HTTP Request
        private readonly string _BaseUrlUrud;  // ✅ เก็บ Base URL ของ API
        private readonly string _apiToken;  // ✅ เก็บ API Token

        public BackupWebsiteController(ApiService apiService, IConfiguration configuration, HttpClient httpClient)
        {
            _apiService = apiService;
            _configuration = configuration;
            _httpClient = httpClient;

            // ✅ โหลดค่า API URL และ Token จาก `appsettings.json`
            _BaseUrlUrud = _configuration["ApiSettings:BaseUrlUrud"] ?? throw new ArgumentNullException(nameof(_BaseUrlUrud), "API URL not found in appsettings.json");
            _apiToken = _configuration["ApiSettings:SansiriApiToken"] ?? throw new ArgumentNullException(nameof(_apiToken), "API Token not found in appsettings.json");
        }

        [Route("download-category", Name = "DownloadCategoryRoute")]
        [HttpGet]
        public async Task<IActionResult> DownloadCategory()
        {
            var data = await _apiService.GetTableUrlCategory();
            return View(data);
        }


        [Route("download-history", Name = "DownloadHistoryRoute")]
        [HttpGet]
        public async Task<IActionResult> DownloadHistory()
        {
            var data = await _apiService.GetDataAsync(); // ✅ เรียกใช้ Service เพื่อดึงข้อมูล
            return View(data); // ✅ ส่งข้อมูลไปแสดงผลใน View
        }


        [Route("download-history/{id_log}", Name = "DownloadDetailRoute")]
        [HttpGet]
        public async Task<IActionResult> DownloadDetail(int id_log)
        {
            var data = await _apiService.GetTableSansiriUrlLogDetail(id_log);
            ViewData["Title"] = $"Detail Log ID: {id_log}";
            return View(data);
        }

        [Route("download-links", Name = "DownloadLinksRoute")]
        [HttpGet]

        public async Task<IActionResult> DownloadLinks()
        {
            var sansiriUrls = await _apiService.GetSansiriUrlsAsync(); // ✅ ดึงข้อมูล URL ทั้งหมด
            var categories = await _apiService.GetTableUrlCategory(); // ✅ ดึงข้อมูลประเภทของ URL

            // ✅ สร้าง Dictionary สำหรับ mapping id_category_url -> name
            var categoryDict = categories.ToDictionary(c => c.id_category_url, c => c.name ?? "Unknown");

            // ✅ ใส่ category_name เข้าไปใน sansiriUrls โดยดูจาก id_category_url
            foreach (var item in sansiriUrls)
            {
                item.category_name = item.id_category_url.HasValue && categoryDict.ContainsKey(item.id_category_url.Value)
                    ? categoryDict[item.id_category_url.Value]
                    : "Unknown";
            }

            return View(sansiriUrls);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int url_id, bool is_active)
        {
            var requestData = new
            {
                url_id = url_id,
                is_active = is_active
            };

            return await SendHttpRequest(requestData, "put-tb-sansiri-url", HttpMethod.Put);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUrl(int url_id)
        {
            var requestData = new
            {
                url_id = url_id,
                is_delete = 1 // ✅ ส่งค่า is_delete เป็น 1 เพื่อบอกว่าให้ลบ
            };

            return await SendHttpRequest(requestData, "put-tb-sansiri-url", HttpMethod.Put);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id_category_url)
        {
            var requestData = new
            {
                id_category_url = id_category_url,
                is_delete = 1 // ✅ ส่งค่า is_delete เป็น 1 เพื่อบอกว่าให้ลบ
            };

            return await SendHttpRequest(requestData, "put-tb-sansiri-url-category", HttpMethod.Put);

        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _apiService.GetTableUrlCategory(); // ✅ ดึงข้อมูล Categories
            return Json(new { success = true, data = categories }); // ✅ ส่งข้อมูล Categories กลับเป็น JSON
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUrl([FromBody] UpdateUrlRequest request)
        {

            // ✅ ตรวจสอบว่าข้อมูลที่รับเข้ามาถูกต้อง
            if (request == null || string.IsNullOrEmpty(request.url) || request.id_category_url == null)
            {
                return Json(new { success = false, error = "Invalid input data" });
            }

            var requestData = new
            {
                url_id = request.url_id,
                url = request.url,
                url_thankyou = request.url_thankyou ?? "",
                id_category_url = request.id_category_url,
            };

            return await SendHttpRequest(requestData, "put-tb-sansiri-url", HttpMethod.Put);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory([FromBody] TableUrlCategory request)
        {

            // ✅ ตรวจสอบว่าข้อมูลที่รับเข้ามาถูกต้อง
            if (request == null || string.IsNullOrEmpty(request.Result[0].name) || request.Result[0].id_category_url == null)
            {
                return Json(new { success = false, error = "Invalid input data" });
            }

            var requestData = new
            {
                id_category_url = request.Result[0].id_category_url,
                name = request.Result[0].name,
            };

            return await SendHttpRequest(requestData, "put-tb-sansiri-url-category", HttpMethod.Put);
        }

        public async Task<IActionResult> AddNewLink([FromBody] AddUrlRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.url) || request.id_category_url == null)
            {
                return Json(new { success = false, error = "Invalid input data" });
            }

            var requestData = new
            {
                url = request.url,
                url_thankyou = request.url_thankyou ?? "",
                id_category_url = request.id_category_url,
                is_active = 1,
            };

            return await SendHttpRequest(requestData, "post-tb-sansiri-url", HttpMethod.Post);
        }

        public async Task<IActionResult> AddCategory([FromBody] TableUrlCategory request)
        {
            if (request == null || string.IsNullOrEmpty(request.Result[0].name))
            {
                return Json(new { success = false, error = "Invalid input data" });
            }

            var requestData = new
            {
                name = request.Result[0].name,
            };

            return await SendHttpRequest(requestData, "post-tb-sansiri-url-category", HttpMethod.Post);
        }

        private async Task<IActionResult> SendHttpRequest(object requestData, string endpoint, HttpMethod httpMethod)
        {
            var apiUrl = $"{_BaseUrlUrud}{endpoint}"; // ✅ ต่อ Base URL + Endpoint อัตโนมัติ
            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("token", _apiToken);

            HttpResponseMessage response;

            if (httpMethod == HttpMethod.Post)
            {
                response = await _httpClient.PostAsync(apiUrl, content);
            }
            else if (httpMethod == HttpMethod.Put)
            {
                response = await _httpClient.PutAsync(apiUrl, content);
            }
            else
            {
                return Json(new { success = false, error = "Unsupported HTTP method" });
            }

            var responseText = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? Json(new { success = true })
                : Json(new { success = false, error = responseText });
        }

    }
}
