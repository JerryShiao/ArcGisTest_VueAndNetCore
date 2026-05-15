var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddMemoryCache(); // 註冊 IMemoryCache，供 WMS Proxy 快取回應使用
builder.Services.AddHttpClient("WmsProxy", client =>
{
    // 設為 90 秒：確保在前端 SDK timeout（120 秒）之前能回傳結果或錯誤
    client.Timeout = TimeSpan.FromSeconds(90);
}); // 註冊 HttpClient，供 WMS 代理使用
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region 註冊 appsettings.json 的參數設定
// WMS API 配置
builder.Services.Configure<ArcGisTest_VueAndNetCore.Server.Model.AppSettings.WmsApiClass>(
    builder.Configuration.GetSection("WmsApi"));
#endregion

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
