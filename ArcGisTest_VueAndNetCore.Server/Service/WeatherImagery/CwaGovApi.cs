namespace ArcGisTest_VueAndNetCore.Server.Service.WeatherImagery
{
    /// <summary>
    /// 氣象開放資料平台 API 服務
    /// </summary>
    public class CwaGovApi
    {
        /// <summary>
        /// HTTP client used for sending HTTP requests.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Application configuration instance.
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// ◆建構子
        /// </summary>
        public CwaGovApi(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        /// <summary>
        /// 【API】氣象觀測站-10分鐘綜觀氣象資料取得
        /// </summary>
        public async Task<string> MeteorologicalDataGetAsync()
        {
            var authorizationId = _config["CwaGovApi:AuthorizationId"]; // 讀取：[氣象開放資料平台會員授權碼]
            var apiUrl = _config["CwaGovApi:O-A0003-001_Url"];          // 讀取 API URL

            // 組合查詢參數
            var url = $"{apiUrl}?Authorization={authorizationId}";

            // 發送 GET 請求
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // 取得回應
            var response = await _httpClient.SendAsync(request);

            // 確保回應成功
            response.EnsureSuccessStatusCode();

            // 讀取回應內容
            return await response.Content.ReadAsStringAsync();
        }
    }//class end
}//namespace end
