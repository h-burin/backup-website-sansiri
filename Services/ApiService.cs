using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using backup_website.Models.SansiriUrlLog;
using backup_website.Models.TableSansiriUrl;
using backup_website.Models.TableUrlCategory;

public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<backup_website.Models.SansiriUrlLog.Result>> GetDataAsync()
    {
        return await FetchApiData<SansiriUrlLog, backup_website.Models.SansiriUrlLog.Result>(
            "http://prd-apigateway.sansiri.com/crud/get-tb-sansiri-url-log"
        );
    }
    public async Task<List<backup_website.Models.TableSansiriUrl.Result>> GetSansiriUrlsAsync()
    {
        return await FetchApiData<TableSansiriUrl, backup_website.Models.TableSansiriUrl.Result>(
            "http://prd-apigateway.sansiri.com/crud/get-tb-sansiri-url?is_delete=0"
        );
    }

    public async Task<List<backup_website.Models.TableUrlCategory.Result>> GetTableUrlCategory()
    {
        return await FetchApiData<TableUrlCategory, backup_website.Models.TableUrlCategory.Result>(
            "http://prd-apigateway.sansiri.com/crud/get-tb-sansiri-url-category"
        );
    }


    /// 🔄 **ฟังก์ชันกลาง** ใช้สำหรับดึงข้อมูลจาก API (ช่วยลดโค้ดซ้ำ)
    private async Task<List<TItem>> FetchApiData<TResponse, TItem>(string url)
        where TResponse : class
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("token", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjQ5MzEwODZjLWY0NjMtNGQ4Ni04ZTVjLTU2NmVmODhlMjVkZiIsImlhdCI6MTU0OTk1MTA4N30.ypF3f7RwVbTJ1_0UWCDszf0DJd1upvssZ5ecXgjzqPU"); // ✅ ใช้ Header "token"

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
