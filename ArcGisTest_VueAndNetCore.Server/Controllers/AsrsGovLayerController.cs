using ArcGisTest_VueAndNetCore.Server.Model.AppSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ArcGisTest_VueAndNetCore.Server.Controllers
{
    /// <summary>
    /// 「航遙測圖資」圖層代理控制器，負責轉發前端對圖層服務的請求，並添加必要的認證資訊
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class AsrsGovLayerController : ControllerBase
    {
        /// <summary>
        /// 航遙測圖資 API 配置
        /// </summary>
        private readonly AsrsGovApi _asrsGovApi;

        #region ◆建構子 [AsrsGovLayerController]
        /// <summary>
        /// ◆建構子
        /// </summary>
        /// <param name="asrsGovApiOptions">航遙測圖資 API 配置選項</param>
        public AsrsGovLayerController(IOptions<AsrsGovApi> asrsGovApiOptions)
        {
            _asrsGovApi = asrsGovApiOptions.Value;
        }
        #endregion

        #region ◆福衛二號影像(WMS) [GetFs2WmsLayer]
        /// <summary>
        /// 取得 2015 年全臺福衛二號影像的 WMS 圖層配置。
        /// </summary>
        /// <returns>包含圖層名稱、代理 URL、格式設定及版本資訊的 Ok 結果。</returns>
        [HttpGet("fs2-wms")]
        public IActionResult GetFs2WmsLayer()
        {
            var layerInfo = new
            {
                name = "2015年全臺福衛二號影像", // 圖層名稱
                url = "/api-fs2" + _asrsGovApi.Fs2WmsUrl, // 代理 URL
                layerName = _asrsGovApi.Fs2WmsLayerName, // WMS 圖層名稱
                format = "image/gif", // 圖像格式
                transparent = false,  // 是否透明
                version = "1.1.0"     // WMS 版本
            };

            return Ok(layerInfo);
        }
        #endregion

        #region ◆福衛二號影像(WMTS) [GetFs2WmtsLayer]
        /// <summary>
        /// 取得 2015 年全臺福衛二號影像的 WMTS 圖層配置。
        /// </summary>
        /// <returns>包含圖層名稱、代理 URL、格式設定及版本資訊的 Ok 結果。</returns>
        [HttpGet("fs2-wmts")]
        public IActionResult GetFs2WmtsLayer()
        {
            var layerInfo = new
            {
                name = "2015年全臺福衛二號影像", // 圖層名稱
                url = "/api-fs2" + _asrsGovApi.Fs2WmtsUrl, // 代理 URL
                layerName = _asrsGovApi.Fs2WmtsLayerName, // WMTS 圖層名稱
                format = "image/gif", // 圖像格式
                transparent = false,  // 是否透明
                version = "1.1.0"     // WMTS 版本
            };

            return Ok(layerInfo);
        }
        #endregion
    }
}
