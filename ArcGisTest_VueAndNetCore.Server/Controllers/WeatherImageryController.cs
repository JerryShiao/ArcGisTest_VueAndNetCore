using ArcGisTest_VueAndNetCore.Server.Model.WeatherImagery;
using ArcGisTest_VueAndNetCore.Server.Service.WeatherImagery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ArcGisTest_VueAndNetCore.Server.Controllers
{
    /// <summary>
    /// 天氣圖像控制器
    /// </summary>
    [ApiController]
    [Route("weather-imagery")]
    public class WeatherImageryController : ControllerBase
    {
        /// <summary>
        /// 天氣圖磚服務
        /// </summary>
        private readonly WeatherTileService _weatherTileService;
        /// <summary>
        /// 天氣影像選項類別
        /// </summary>
        private readonly WeatherImageryOptions _weatherImageryOptions;

        #region ◆建構子 [WeatherImageryController]
        /// <summary>
        /// ◆建構子
        /// </summary>
        /// <param name="weatherTileService">天氣圖磚服務</param>
        /// <param name="weatherImageryOptions">天氣影像選項類別</param>
        public WeatherImageryController(WeatherTileService weatherTileService, IOptions<WeatherImageryOptions> weatherImageryOptions)
        {
            _weatherTileService = weatherTileService;
            _weatherImageryOptions = weatherImageryOptions.Value;
        }
        #endregion

        #region ◆服務狀態檢查 [ServiceInfoGet]
        /// <summary>
        /// ◆服務狀態檢查
        /// </summary>
        /// <param name="f">回應格式，僅支援 pjson 或 json</param>
        /// <returns>服務資訊的 JSON 結果</returns>
        [HttpGet]

        public IActionResult ServiceInfoGet([FromQuery] string f = "pjson")
        {
            // 檢查回應格式是否為 pjson 或 json
            if (!string.Equals(f, "pjson", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(f, "json", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only f=pjson or f=json is supported.");

            // 取得服務資訊
            var info = new
            {
                currentVersion = 11.3,   // 服務版本
                serviceDescription = "Weather Interpolation Imagery Service", // 服務描述
                name = "WeatherImagery", // 服務名稱
                type = "ImageServer",    // 服務類型
                capabilities = "Image,Metadata", // 服務功能
                copyrightText = "Copyright © Your Organization", // 版權信息
                minScale = 0, // 最小比例尺
                maxScale = 0, // 最大比例尺
                pixelType = "F32",       // 圖像像素類型
                bandCount = 2,           // 圖像波段數
                defaultMosaicMethod = "None", // 預設拼接方法
                defaultResamplingMethod = "Nearest", // 預設重採樣方法
                maxImageHeight = 4096, // 最大圖像高度
                maxImageWidth = 4096,  // 最大圖像寬度
                spatialReference = new { wkid = 4326 }, // 空間參考系統
                fullExtent = new   // 服務完整範圍              
                {
                    xmin = _weatherImageryOptions.Extent.XMin, // 最小 X 座標
                    ymin = _weatherImageryOptions.Extent.YMin, // 最小 Y 座標
                    xmax = _weatherImageryOptions.Extent.XMax, // 最大 X 座標
                    ymax = _weatherImageryOptions.Extent.YMax, // 最大 Y 座標
                    spatialReference = new { wkid = 4326 }     // 空間參考系統
                },
                extent = new // 服務顯示範圍
                {
                    xmin = _weatherImageryOptions.Extent.XMin,
                    ymin = _weatherImageryOptions.Extent.YMin,
                    xmax = _weatherImageryOptions.Extent.XMax,
                    ymax = _weatherImageryOptions.Extent.YMax,
                    spatialReference = new { wkid = 4326 }
                },
                tileInfo = new // 磚片資訊
                {
                    rows = 256, // 磚片行數
                    cols = 256, // 磚片列數
                    dpi = 96,   // 磚片解析度
                    format = "PNG32", // 磚片格式
                    origin = new { x = -180.0, y = 90.0 },         // 磚片原點
                    spatialReference = new { wkid = 4326 },        // 空間參考系統
                    lods = Enumerable.Range(0, 11).Select(i => new // 磚片金字塔層級資訊
                    {
                        level = i,                                   // 層級
                        resolution = 180.0 / (256 * Math.Pow(2, i)), // 層級解析度
                        scale = 295829355.45 / Math.Pow(2, i)        // 層級比例尺
                    }).ToArray()
                },
                rasterTypeInfos = Array.Empty<object>(), // 光柵類型資訊
                // 服務欄位資訊
                fields = new object[]
                {
                    new { name = "OBJECTID", type = "esriFieldTypeOID", alias = "OBJECTID" }
                },
                drawingInfo = new // 繪製資訊
                {
                    renderer = new
                    {
                        type = "flow", // 標註為流向渲染器
                        visualVariables = new[]
                        {
                            new {
                                type = "sizeVariable",
                                field = "Magnitude", // 由 U/V 自動計算
                                minSize = 0.5,
                                maxSize = 10
                            }
                        },
                        flowType = "u_v", // 關鍵：告知波段代表 U 與 V
                        density = 1,
                        velocityScale = 1
                    }
                },
                // 修改 rasterFunctionInfos 讓前端知道如何處理
                rasterFunctionInfos = new[]
                {
                    new { name = "None", description = "Make a 2-band export for Wind.", help = "" }
                },
                serviceDataType = "esriImageServiceDataTypeVector-UV", // 服務資料類型為 Vector-UV
                bandNames = new[] { "U", "V" } // 兩個波段名稱
            };

            return Ok(info);
        }
        #endregion

        [HttpGet("keyProperties")]
        public IActionResult KeyProperties([FromQuery] string f = "json")
        {
            if (!string.Equals(f, "pjson", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(f, "json", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only f=pjson or f=json is supported.");

            // 範例回傳內容，可依實際需求調整
            var result = new
            {
                keyProperties = new string[0]
            };
            return new JsonResult(result);
        }

        #region ◆圖像請求 [TileGet]
        /// <summary>
        /// ◆圖像請求
        /// </summary>
        /// <param name="z">層級</param>
        /// <param name="y">行</param>
        /// <param name="x">列</param>
        /// <param name="ct">取消令牌</param>
        /// <returns></returns>
        [HttpGet("tile/{z:int}/{y:int}/{x:int}")]
        public async Task<IActionResult> TileGet(int z, int y, int x, CancellationToken ct)
        {
            var png = await _weatherTileService.RenderTileAsync(z, y, x, ct); // 取得圖像磚
            return File(png, "image/png"); // 回傳圖像磚作為 PNG 格式的檔案
        }
        #endregion

        [HttpGet("exportImage")]
        public async Task<IActionResult> ExportImage(
    [FromQuery] string bbox,
    [FromQuery] string size,
    [FromQuery] string format = "tiff", // 建議改用 tiff 或 lerc 以攜帶浮點數
    [FromQuery] string f = "image",
    CancellationToken ct = default)
        {
            // 解析 bbox 與 size
            var bboxParts = bbox.Split(',').Select(double.Parse).ToArray();
            var sizeParts = size.Split(',').Select(int.Parse).ToArray();
            if (bboxParts.Length != 4 || sizeParts.Length != 2)
                return BadRequest("Invalid bbox or size.");

            // 產生影像

            // 1. 取得原始 U, V 資料矩陣 (而非繪製好的圖片)
            // 這裡假設您的 Service 有一個方法可以產出兩層 float 矩陣
            var png = await _weatherTileService.RenderImageAsync(
               bboxParts[0], bboxParts[1], bboxParts[2], bboxParts[3],
                sizeParts[0], sizeParts[1], ct);       

            return File(png, "image/png");
        }


        // 2. /identify
        [HttpGet("identify")]
        public IActionResult Identify()
        {
            return StatusCode(501, "Not Implemented");
        }

        // 3. /info
        [HttpGet("info")]
        public IActionResult Info()
        {
            return StatusCode(501, "Not Implemented");
        }


    }//class end
}//namespace end
