var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient("WmsProxy", client =>
{
    client.Timeout = TimeSpan.FromSeconds(300); // WMS 遠端服務回應較慢，設定較長的 timeout
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
