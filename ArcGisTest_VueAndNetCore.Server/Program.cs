using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// 註冊 IMemoryCache，供 Proxy 快取回應使用
builder.Services.AddMemoryCache();

// 註冊 YARP 服務，並指定讀取 appsettings.json 中的 "ReverseProxy" 區塊
builder.Services.AddReverseProxy()
.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(ctx =>
    {
        // 只針對特定路由注入 Basic Auth 邏輯
        var routesNeedingAuth = new HashSet<string> { "fs2-route", "asrs-route" };
        if (!routesNeedingAuth.Contains(ctx.Route.RouteId)) return;

        var account = builder.Configuration["AsrsGovApi:ConnectAccount"] ?? "";
        var password = builder.Configuration["AsrsGovApi:ConnectPassword"] ?? "";
        var credentials = Convert.ToBase64String(
            System.Text.Encoding.ASCII.GetBytes($"{account}:{password}"));

        // 1. 注入 Basic Auth 標頭
        ctx.AddRequestTransform(reqCtx =>
        {
            reqCtx.ProxyRequest.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            return ValueTask.CompletedTask;
        });

        // 2. XML 回應：將真實 origin 替換成 proxy 前綴，讓 ArcGIS SDK 後續請求繼續走 YARP
        ctx.AddResponseTransform(async resCtx =>
        {
            var contentType = resCtx.ProxyResponse?.Content.Headers.ContentType?.ToString() ?? "";
            if (!contentType.Contains("xml", StringComparison.OrdinalIgnoreCase)) return;

            resCtx.SuppressResponseBody = true;

            // 手動偵測並解壓縮，因為 YARP 的 ProxyResponse 不會自動解壓縮
            var contentEncoding = resCtx.ProxyResponse!.Content.Headers.ContentEncoding
                                         .FirstOrDefault()?.ToLowerInvariant();
            Stream rawStream = await resCtx.ProxyResponse.Content.ReadAsStreamAsync();
            Stream bodyStream = contentEncoding switch
            {
                "gzip"    => new System.IO.Compression.GZipStream(rawStream,
                                 System.IO.Compression.CompressionMode.Decompress),
                "br"      => new System.IO.Compression.BrotliStream(rawStream,
                                 System.IO.Compression.CompressionMode.Decompress),
                "deflate" => new System.IO.Compression.DeflateStream(rawStream,
                                 System.IO.Compression.CompressionMode.Decompress),
                _         => rawStream
            };
            using var reader = new System.IO.StreamReader(bodyStream);
            var xml = await reader.ReadToEndAsync();

            // 在請求時從 YARP IReverseProxyFeature 取得實際使用的目的地位址，確保資料正確
            var proxyFeature = resCtx.HttpContext.Features
                .Get<Yarp.ReverseProxy.Model.IReverseProxyFeature>();
            var realAddress = proxyFeature?.ProxiedDestination?.Model.Config.Address.TrimEnd('/') ?? "";

            // 從路由 Path 取得代理前綴，例如 "/api-fs2/{**catchall}" → "/api-fs2"
            var pathPattern = ctx.Route.Match.Path ?? "";
            var proxyPrefix = pathPattern.Split("/{**", 2)[0];

            // 將 XML 中的上游基底路徑整段替換成代理前綴，讓 ArcGIS SDK 後續請求走 YARP
            var requestOrigin = $"{resCtx.HttpContext.Request.Scheme}://{resCtx.HttpContext.Request.Host}";

            // WMS 伺服器依據轉發的 Host 標頭動態生成 URL，因此 XML 中的 origin 已是 requestOrigin，
            // 只需將路徑部分從上游路徑（如 /asofb）替換成代理前綴（如 /api-fs2）
            var realPath = new Uri(realAddress).AbsolutePath.TrimEnd('/'); // e.g. "/asofb"
            var rewritten = xml.Replace($"{requestOrigin}{realPath}", $"{requestOrigin}{proxyPrefix}");
            var bytes = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(rewritten);

            // 移除壓縮相關標頭，避免瀏覽器嘗試解壓縮已展開的純文字
            resCtx.HttpContext.Response.Headers.Remove("Content-Encoding");
            resCtx.HttpContext.Response.Headers.Remove("Transfer-Encoding");

            resCtx.HttpContext.Response.ContentType = contentType;
            resCtx.HttpContext.Response.ContentLength = bytes.Length;
            await resCtx.HttpContext.Response.Body.WriteAsync(bytes);
        });
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region 註冊 appsettings.json 的參數設定
// 航遙測圖資 API 配置
builder.Services.Configure<ArcGisTest_VueAndNetCore.Server.Model.AppSettings.AsrsGovApi>(
    builder.Configuration.GetSection("AsrsGovApi"));
#endregion

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();   // ← 原本[app.MapStaticAssets();]作廢，改用這個，可正確服務 Vite 動態命名的檔案

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 開發環境才啟用 HTTPS 重定向，避免生產環境因為沒有正確設定 SSL 證書而無法運行
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

// 啟用 YARP 路由對應 (建議放在 UseAuthorization 之後)
app.MapReverseProxy();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
