using System.Text.Json;
using backup_website.Models.SansiriUrlLog;
using backup_website.Models.TableSansiriUrl;
using backup_website.Models.TableUrlCategory;
using backup_website.Models.TableSansiriUrlLogDetail;
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _BaseUrlUrud;

    public ApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _BaseUrlUrud = _configuration["ApiSettings:BaseUrlUrud"] ?? throw new ArgumentNullException(nameof(_BaseUrlUrud), "API URL not found in appsettings.json");
    }
    public async Task<List<backup_website.Models.SansiriUrlLog.Result>> GetDataAsync()
    {
        return await FetchApiData<SansiriUrlLog, backup_website.Models.SansiriUrlLog.Result>(
            $"{_BaseUrlUrud}get-tb-sansiri-url-log"
        );
    }
    public async Task<List<backup_website.Models.TableSansiriUrl.Result>> GetSansiriUrlsAsync()
    {
        return await FetchApiData<TableSansiriUrl, backup_website.Models.TableSansiriUrl.Result>(
            $"{_BaseUrlUrud}get-tb-sansiri-url"
        );
    }

    public async Task<List<backup_website.Models.TableUrlCategory.Result>> GetTableUrlCategory()
    {
        return await FetchApiData<TableUrlCategory, backup_website.Models.TableUrlCategory.Result>(
            $"{_BaseUrlUrud}get-tb-sansiri-url-category"
        );
    }

    public async Task<List<backup_website.Models.TableSansiriUrlLogDetail.Result>> GetTableSansiriUrlLogDetail(int? id_log = null)
    {
        string apiUrl = $"{_BaseUrlUrud}get-tb-sansiri-url-log-detail?size=9999&page=1";

        if (id_log.HasValue)
        {
            apiUrl += $"&id_log={id_log.Value}"; // ✅ เพิ่ม id_log เข้าไปใน URL ถ้ามีค่า
        }

        return await FetchApiData<TableSansiriUrlLogDetail, backup_website.Models.TableSansiriUrlLogDetail.Result>(apiUrl);
    }




    /// 🔄 **ฟังก์ชันกลาง** ใช้สำหรับดึงข้อมูลจาก API (ช่วยลดโค้ดซ้ำ)
    private async Task<List<TItem>> FetchApiData<TResponse, TItem>(string url)
        where TResponse : class
    {
        var token = _configuration["ApiSettings:SansiriApiToken"];
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("token", token);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error {response.StatusCode}: {errorContent}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // ✅ คืนค่า List<TItem> จาก Model
        return (data?.GetType().GetProperty("Result")?.GetValue(data) as List<TItem>) ?? new List<TItem>();
    }
}
