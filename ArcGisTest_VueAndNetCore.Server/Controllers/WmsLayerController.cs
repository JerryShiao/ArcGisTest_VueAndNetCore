using ArcGisTest_VueAndNetCore.Server.Model.AppSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace ArcGisTest_VueAndNetCore.Server.Controllers
{
    /// <summary>
    /// WMS 圖層代理控制器，負責轉發前端對 WMS 服務的請求，並添加必要的認證資訊
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class WmsLayerController : ControllerBase
    {
        /// <summary>
        /// HTTP客戶端實例
        /// </summary>
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// WMS API 配置
        /// </summary>
        private readonly WmsApiClass _wmsApi;

        /// <summary>
        /// 記憶體快取，用於儲存 WMS 回應
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// 快取 WMS 回應的資料結構
        /// </summary>
        private record WmsCacheEntry(byte[] Data, string ContentType);

        /// <summary>
        /// ◆建構子
        /// </summary>
        /// <param name="httpClientFactory">HTTP客戶端實例</param>
        /// <param name="wmsApiOptions">WMS API 配置選項</param>
        /// <param name="cache">記憶體快取</param>
        public WmsLayerController(IHttpClientFactory httpClientFactory, IOptions<WmsApiClass> wmsApiOptions, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _wmsApi = wmsApiOptions.Value;
            _cache = cache;
        }

        #region ◆私有方法：重試 HTTP 請求
        /// <summary>
        /// 對 WMS 服務執行帶重試的 GET 請求，遇到 502/503/504 時自動重試
        /// </summary>
        private static async Task<HttpResponseMessage> GetWithRetryAsync(
            HttpClient client, string url, int maxRetries = 3, int delayMs = 1000)
        {
            HttpResponseMessage? response = null;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // ResponseHeadersRead：只等標頭，避免讀取內容時連線中斷拋出例外
                    response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    if (response.IsSuccessStatusCode ||
                        ((int)response.StatusCode != 502 &&
                         (int)response.StatusCode != 503 &&
                         (int)response.StatusCode != 504))
                    {
                        return response;
                    }
                }
                catch (HttpRequestException) when (attempt < maxRetries)
                {
                    // 連線被切斷或串流複製失敗，視為暫時性錯誤，繼續重試
                }
                catch (TaskCanceledException)
                {
                    // HttpClient 逾時：直接往外拋，不重試，避免累計等待時間超過前端 SDK timeout
                    throw;
                }

                if (attempt < maxRetries)
                    await Task.Delay(delayMs);
            }
            return response!;
        }
        #endregion

        #region ◆福衛二號影像
        /// <summary>
        /// 取得 2015 年全臺福衛二號影像的 WMS 圖層配置。
        /// </summary>
        /// <returns>包含圖層名稱、代理 URL、格式設定及 WMS 版本資訊的 Ok 結果。</returns>
        [HttpGet("fs2")]
        public IActionResult GetFs2WmsLayer()
        {
            // url 指向後端 proxy，前端不直接接觸 WMS 帳密
            var layerInfo = new
            {
                name = "2015年全臺福衛二號影像", // 圖層名稱
                url = $"{Request.Scheme}://{Request.Host}/wmslayer/fs2/proxy", // 代理 URL
                layerName = "FS2:fs2015-1_masked_2m_enhance", // WMS 圖層名稱
                format = "image/gif", // 圖像格式
                transparent = false,  // 是否透明
                version = "1.1.0"     // WMS 版本
            };

            return Ok(layerInfo);
        }

        /// <summary>
        /// 全臺福衛二號影像：代理 WMS 請求，後端附加帳號密碼後轉發至真實 WMS 服務
        /// 若回應為 GetCapabilities XML，會將其中的真實伺服器來源替換成 proxy URL，
        /// 讓 ArcGIS SDK 後續所有請求（GetMap 等）都繼續走 proxy，避免 CORS 問題。
        /// 使用 catch-all 路由 {**path} 接住 ArcGIS SDK 因 XML 替換而帶入的額外路徑段。
        /// </summary>
        [HttpGet("fs2/proxy")]
        [HttpGet("fs2/proxy/{**path}")]
        public async Task<IActionResult> ProxyFs2WmsRequest(string? path = null)
        {
            // 從 appsettings.json 讀取 WMS API 配置
            var baseUrl = _wmsApi.Fs2WmsUrl;        // 真實 WMS 服務的 URL
            var username = _wmsApi.ConnectAccount;  // 連線帳號
            var password = _wmsApi.ConnectPassword; // 認證密碼

            var queryString = Request.QueryString.Value ?? string.Empty;

            string targetUrl;
            if (string.IsNullOrEmpty(path))
            {
                // 前端初始呼叫（GetCapabilities）：直接轉發至設定的 baseUrl
                targetUrl = $"{baseUrl}{queryString}";
            }
            else
            {
                // ArcGIS SDK 後續請求（GetMap 等）：path 為 XML 中替換出來的路徑段
                // 還原為真實伺服器 URL：realOrigin + "/" + path + queryString
                var realUri = new Uri(baseUrl);
                var realOrigin = $"{realUri.Scheme}://{realUri.Host}";
                targetUrl = $"{realOrigin}/{path}{queryString}";
            }

            // 先查快取，命中則直接回傳，省去外部網路往返
            if (_cache.TryGetValue(targetUrl, out WmsCacheEntry? cached) && cached is not null)
                return File(cached.Data, cached.ContentType);

            // 建立 HttpClient 實例並添加基本認證頭
            var client = _httpClientFactory.CreateClient("WmsProxy");
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // 轉發請求至真實 WMS 服務（遇到 502/503/504 或連線中斷自動重試最多 3 次）
            HttpResponseMessage response;
            try
            {
                response = await GetWithRetryAsync(client, targetUrl);
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, "WMS 服務請求遂時，請稍後再試");
            }

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "WMS 服務回應錯誤");

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/gif";

            try
            {
                // 若為 GetCapabilities XML 回應，將真實伺服器來源替換成 proxy URL
                // 使 ArcGIS SDK 解析後的所有端點都指向 proxy
                if (contentType.Contains("xml", StringComparison.OrdinalIgnoreCase))
                {
                    var xml = await response.Content.ReadAsStringAsync();
                    var proxyUrl = $"{Request.Scheme}://{Request.Host}/wmslayer/fs2/proxy";
                    var realUri = new Uri(baseUrl);
                    var realOrigin = $"{realUri.Scheme}://{realUri.Host}";
                    var rewrittenXml = xml.Replace(realOrigin, proxyUrl);
                    var xmlBytes = Encoding.UTF8.GetBytes(rewrittenXml);
                    // XML（GetCapabilities）很少變動，快取 1 小時
                    _cache.Set(targetUrl, new WmsCacheEntry(xmlBytes, contentType), TimeSpan.FromHours(1));
                    return Content(rewrittenXml, contentType);
                }

                // 非 XML（GetMap 影像）快取 30 分鐘
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                _cache.Set(targetUrl, new WmsCacheEntry(imageBytes, contentType), TimeSpan.FromMinutes(30));
                return File(imageBytes, contentType);
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException)
            {
                return StatusCode(502, "讀取 WMS 回應內容時連線中斷，請稍後再試");
            }
        }

        /// <summary>
        /// ArcGIS SDK 專用 Proxy 端點
        /// ArcGIS SDK 的 proxyRules 會將請求格式化為：GET /wmslayer/fs2/arcgisproxy?https://realserver.com/path?param=value
        /// 本端點解析該格式，附加認證後轉發至真實 WMS 服務
        /// </summary>
        [HttpGet("fs2/arcgisproxy")]
        public async Task<IActionResult> ArcGisProxyFs2WmsRequest()
        {
            var username = _wmsApi.ConnectAccount;
            var password = _wmsApi.ConnectPassword;

            // ArcGIS SDK proxy 格式：query string 的原始值即為完整目標 URL（不含 '?' 前綴）
            var rawQuery = Request.QueryString.Value ?? string.Empty;
            if (rawQuery.StartsWith("?"))
                rawQuery = rawQuery[1..];

            if (string.IsNullOrEmpty(rawQuery))
                return BadRequest("缺少目標 URL");

            // 還原被 URL encode 的目標網址
            var targetUrl = Uri.UnescapeDataString(rawQuery);

            // 先查快取，命中則直接回傳
            if (_cache.TryGetValue(targetUrl, out WmsCacheEntry? cachedEntry) && cachedEntry is not null)
                return File(cachedEntry.Data, cachedEntry.ContentType);

            var client = _httpClientFactory.CreateClient("WmsProxy");

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            HttpResponseMessage response;
            try
            {
                response = await GetWithRetryAsync(client, targetUrl);
            }
            catch (TaskCanceledException)
            {
                return StatusCode(504, "WMS 服務請求遒時，請稍後再試");
            }

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "WMS 服務回應錯誤");

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

            try
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                // 影像快取 30 分鐘
                _cache.Set(targetUrl, new WmsCacheEntry(bytes, contentType), TimeSpan.FromMinutes(30));
                return File(bytes, contentType);
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException)
            {
                return StatusCode(502, "讀取 WMS 回應內容時連線中斷，請稍後再試");
            }
        }
        #endregion
    }
}
