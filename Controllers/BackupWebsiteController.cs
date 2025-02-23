using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using backup_website.Models.Requests; // ✅ นำเข้า Model ที่ใช้รับค่าจาก Frontend

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

        /// ✅ ดึงข้อมูลประวัติการดาวน์โหลดจาก `get-tb-sansiri-url-log`
        public async Task<IActionResult> DownloadHistory()
        {
            var data = await _apiService.GetDataAsync(); // ✅ เรียกใช้ Service เพื่อดึงข้อมูล
            return View(data); // ✅ ส่งข้อมูลไปแสดงผลใน View
        }

        /// ✅ ดึงลิงก์ทั้งหมดจาก `get-tb-sansiri-url`
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

            return View(sansiriUrls); // ✅ ส่งข้อมูลไปแสดงผลใน View
        }

        /// ✅ ฟังก์ชันกลางที่ใช้ร่วมกันระหว่าง Update และ Delete
        private async Task<IActionResult> SendPutRequest(object requestData)
        {
            var apiUrl = $"{_BaseUrlUrud}put-tb-sansiri-url"; // ✅ รวม Base URL + Endpoint API

            var json = JsonSerializer.Serialize(requestData); // ✅ แปลง object เป็น JSON
            var content = new StringContent(json, Encoding.UTF8, "application/json"); // ✅ กำหนด Content-Type เป็น JSON

            _httpClient.DefaultRequestHeaders.Clear(); // ✅ ล้าง Header ก่อนใส่ใหม่
            _httpClient.DefaultRequestHeaders.Add("token", _apiToken); // ✅ ใส่ API Token ใน Header

            var response = await _httpClient.PutAsync(apiUrl, content); // ✅ ส่ง HTTP PUT Request ไปยัง API
            var responseText = await response.Content.ReadAsStringAsync(); // ✅ อ่าน Response จาก API

            return response.IsSuccessStatusCode
                ? Json(new { success = true }) // ✅ ส่ง Response กลับถ้าสำเร็จ
                : Json(new { success = false, error = responseText }); // ✅ ส่ง Error กลับถ้าไม่สำเร็จ
        }

        /// ✅ อัปเดตสถานะของ URL (เปิด/ปิด) โดยรับค่า `url_id` และ `is_active`
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int url_id, bool is_active)
        {
            var requestData = new
            {
                url_id = url_id,
                is_active = is_active ? 1 : 0 // ✅ แปลงเป็น 1 (เปิด) หรือ 0 (ปิด)
            };

            return await SendPutRequest(requestData); // ✅ ใช้ฟังก์ชันกลาง SendPutRequest
        }

        /// ✅ ลบ URL โดยส่ง `is_delete = 1`
        [HttpPost]
        public async Task<IActionResult> DeleteUrl(int url_id)
        {
            var requestData = new
            {
                url_id = url_id,
                is_delete = 1 // ✅ ส่งค่า is_delete เป็น 1 เพื่อบอกว่าให้ลบ
            };

            return await SendPutRequest(requestData); // ✅ ใช้ฟังก์ชันกลาง SendPutRequest
        }

        /// ✅ ดึง Categories มาแสดงใน Dropdown
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _apiService.GetTableUrlCategory(); // ✅ ดึงข้อมูล Categories
            return Json(new { success = true, data = categories }); // ✅ ส่งข้อมูล Categories กลับเป็น JSON
        }

        /// ✅ อัปเดต URL โดยรับค่าจาก Modal (ใช้ในหน้าแก้ไข)
        [HttpPost]
        public async Task<IActionResult> UpdateUrl([FromBody] UpdateUrlRequest request)
        {
            // ✅ Debug Log เพื่อตรวจสอบค่าที่ได้รับจาก Frontend
            Console.WriteLine($"📌 Debug - ค่าที่ได้รับจาก Frontend: {JsonSerializer.Serialize(request)}");

            // ✅ ตรวจสอบว่าข้อมูลที่รับเข้ามาถูกต้อง
            if (request == null || string.IsNullOrEmpty(request.url) || request.id_category_url == null)
            {
                return Json(new { success = false, error = "Invalid input data" });
            }

            // ✅ แปลง `bool?` เป็น `int?` เพื่อให้รองรับค่า `0` และ `1`
            var requestData = new
            {
                url_id = request.url_id,
                url = request.url,
                url_thankyou = request.url_thankyou ?? "",
                id_category_url = request.id_category_url,
                is_active = request.is_active.HasValue ? Convert.ToInt32(request.is_active.Value) : 1 // ✅ ใช้ Convert.ToInt32()
            };

            // ✅ Debug Log ค่าที่จะถูกส่งไป API
            Console.WriteLine($"📌 Debug - requestData ที่จะส่งไป API: {JsonSerializer.Serialize(requestData)}");

            return await SendPutRequest(requestData);
        }

        private async Task<IActionResult> SendPostRequest(object requestData)
        {
            var apiUrl = $"{_BaseUrlUrud}post-tb-sansiri-url"; // ✅ เปลี่ยนเป็น API `POST`

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("token", _apiToken);

            var response = await _httpClient.PostAsync(apiUrl, content);
            var responseText = await response.Content.ReadAsStringAsync();

            return response.IsSuccessStatusCode
                ? Json(new { success = true })
                : Json(new { success = false, error = responseText });
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
                is_active = 1, // ✅ เปิดใช้งานเริ่มต้น
            };

            return await SendPostRequest(requestData);

        }


    }
}
