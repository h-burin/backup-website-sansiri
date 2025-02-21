var builder = WebApplication.CreateBuilder(args);

// ✅ เพิ่ม ApiService ให้ใช้งานได้ใน Controller
builder.Services.AddHttpClient<ApiService>();

// ✅ ลงทะเบียน MVC Controller
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BackupWebsite}/{action=DownloadHistory}/{id?}"); // ✅ เปลี่ยนให้ชี้ไปที่ `BackupWebsiteController`

app.Run();
