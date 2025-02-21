using System.Text.Json;
public class ApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Result>> GetDataAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "http://prd-apigateway.sansiri.com/crud/get-tb-sansiri-url-log");
        request.Headers.Add("token", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjQ5MzEwODZjLWY0NjMtNGQ4Ni04ZTVjLTU2NmVmODhlMjVkZiIsImlhdCI6MTU0OTk1MTA4N30.ypF3f7RwVbTJ1_0UWCDszf0DJd1upvssZ5ecXgjzqPU"); // ✅ ใช้ Header "token" ตามที่ Postman ใช้

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"API Error {response.StatusCode}: {errorContent}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<SansiriUrlLog>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return data?.Result ?? new List<Result>();
    }

}
