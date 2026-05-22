using ArcGisTest_VueAndNetCore.Server.Model.WeatherImagery;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace ArcGisTest_VueAndNetCore.Server.Service.WeatherImagery
{
    /// <summary>
    /// 天氣內插服務
    /// </summary>
    public class WeatherInterpolationService
    {
        /// <summary>
        /// 氣象開放資料平台 API 服務
        /// </summary>
        private readonly CwaGovApi _cwaGovApi;

        /// <summary>
        /// 記憶體快取服務
        /// </summary>
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// 天氣影像選項類別
        /// </summary>
        private readonly WeatherImageryOptions _weatherImageryOptions;

        #region ◆建構子 [WeatherInterpolationService]
        /// <summary>
        /// ◆建構子
        /// </summary>
        /// <param name="cwaGovApi">氣象開放資料平台 API 服務</param>
        /// <param name="memoryCache">記憶體快取服務</param>
        /// <param name="weatherImageryOptions">天氣影像選項類別</param>
        public WeatherInterpolationService(
            CwaGovApi cwaGovApi,
            IMemoryCache memoryCache,
            IOptions<WeatherImageryOptions> weatherImageryOptions)
        {
            _cwaGovApi = cwaGovApi;
            _memoryCache = memoryCache;
            _weatherImageryOptions = weatherImageryOptions.Value;
        }
        #endregion

        #region ◆取得內插後的天氣影像資料 [OrBuildGridGetAsync]
        /// <summary>
        /// 取得內插後的天氣影像資料
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>內插後的天氣影像資料</returns>
        /// <exception cref="InvalidOperationException">當測站資料不足時拋出例外</exception>
        public async Task<double[,]> OrBuildGridGetAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = "weather:grid:latest"; // 記憶體快取鍵

            // 嘗試從快取中取得資料
            if (_memoryCache.TryGetValue(cacheKey, out double[,]? grid) && grid is not null)
            {
                return grid;
            }

            // 從氣象開放資料平台 API 取得原始資料
            var raw = await _cwaGovApi.MeteorologicalDataGetAsync();

            // 解析原始資料
            var points = ParseStations(raw);

            // 檢查測站資料是否足夠
            if (points.Count < 3)
            {
                throw new InvalidOperationException("測站資料不足，無法插值。");
            }

            // 執行內插計算
            grid = BuildIdwGrid(points,
                _weatherImageryOptions.Extent.XMin, _weatherImageryOptions.Extent.YMin, _weatherImageryOptions.Extent.XMax, _weatherImageryOptions.Extent.YMax,
                _weatherImageryOptions.GridWidth, _weatherImageryOptions.GridHeight, _weatherImageryOptions.IdwPower);

            // 將內插結果存入快取，並設定過期時間
            _memoryCache.Set(cacheKey, grid, TimeSpan.FromMinutes(_weatherImageryOptions.CacheMinutes));

            // 回傳內插後的天氣影像資料
            return grid;
        }
        #endregion

        #region ◆回傳資料測站點位解析 [ParseStations]
        /// <summary>
        ///  ◆回傳資料測站點位解析
        /// </summary>
        /// <param name="json">JSON 字串</param>
        /// <returns>解析後的測站點位資料列表</returns>
        private static List<StationPoint> ParseStations(string json)
        {
            var result = new List<StationPoint>(); // 儲存解析後的測站點位資料列表

            // 解析 JSON 字串
            using var doc = JsonDocument.Parse(json);
            // 如果 JSON 中沒有 "records" 屬性，回傳空列表
            if (!doc.RootElement.TryGetProperty("records", out var records))
            {
                return result; 
            }
            // 如果 "records" 中沒有 "Station" 屬性或 "Station" 不是陣列，回傳空列表
            if (!records.TryGetProperty("Station", out var stations) || stations.ValueKind != JsonValueKind.Array)
            {
                return result; 
            }

            // 迭代 "Station" 陣列中的每個元素
            foreach (var stationItem in stations.EnumerateArray())
            {
                var coords = stationItem.GetProperty("GeoInfo").GetProperty("Coordinates"); // 取得測站的經緯度座標
                if (coords.ValueKind != JsonValueKind.Array || coords.GetArrayLength() == 0) continue; // 如果座標不是陣列或陣列為空，跳過此測站

                // 優先用 WGS84，找不到再用第一個可用座標
                JsonElement? coord = null;
                foreach (var coordItem in coords.EnumerateArray())
                {
                    if (coordItem.TryGetProperty("CoordinateName", out var nameEl) &&
                        string.Equals(nameEl.GetString(), "WGS84", StringComparison.OrdinalIgnoreCase))
                    {
                        coord = coordItem;
                        break;
                    }
                }
                coord ??= coords[0]; // 如果沒有找到 WGS84 座標，使用第一個可用座標

                if (!TryReadDouble(coord.Value, "StationLongitude", out var stationLongitude)) continue; // 讀取經度失敗，跳過此測站
                if (!TryReadDouble(coord.Value, "StationLatitude", out var stationLatitude)) continue;  // 讀取緯度失敗，跳過此測站
                if (!TryReadDouble(stationItem, "WeatherElement.AirTemperature", out var airTemperature)) continue; // 讀取氣溫失敗，跳過此測站

                // 跳過無效值
                if (stationLongitude == -99 || stationLatitude == -99 || airTemperature == -99) continue;

                // 將解析後的測站點位資料加入結果列表
                result.Add(new StationPoint(stationLongitude, stationLatitude, airTemperature));
            }
            return result;
        }
        #endregion

        #region ◆從指定路徑的 JSON 元素中讀取雙精度值 [TryReadDouble]
        /// <summary>
        /// ◆從指定路徑的 JSON 元素中讀取雙精度值
        /// </summary>
        /// <param name="root">要讀取的 JSON</param>
        /// <param name="path">要讀取的值在 JSON 元素中的路徑。</param>
        /// <param name="value">當此方法返回時，如果成功，包含解析後的雙精度值；否則，包含預設值。</param>
        /// <returns>如果值成功讀取並解析，則為 <see langword="true"/>；否則，為 <see langword="false"/>。</returns>
        private static bool TryReadDouble(JsonElement root, string path, out double value)
        {
            value = default; // 初始化輸出參數

            // 取得JSON元素中指定路徑的字串，如果路徑不存在或值不是字串，返回 false
            if (!TryReadPath(root, path, out var resultString) || string.IsNullOrWhiteSpace(resultString))
            {
                return false;
            }

            // 嘗試將讀取到的字串解析為雙精度值
            return double.TryParse(resultString, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || double.TryParse(resultString, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
        }

        /// <summary>
        /// ◆從指定路徑的 JSON 元素中讀取雙精度值
        /// </summary>
        /// <param name="root">要讀取的 JSON</param>
        /// <param name="paths">要讀取的值在 JSON 元素中的路徑集合。</param>
        /// <returns>如果成功，包含解析後的雙精度值；否則，包含預設值。</returns>
        private static double? TryReadDouble(JsonElement root, IEnumerable<string> paths)
        {
            foreach (var pathItem in paths)
            {
                if (TryReadPath(root, pathItem, out var resultString) &&
                    double.TryParse(resultString, NumberStyles.Float, CultureInfo.InvariantCulture, out var resultDouble))
                    return resultDouble;

                if (TryReadPath(root, pathItem, out resultString) &&
                    double.TryParse(resultString, NumberStyles.Float, CultureInfo.CurrentCulture, out resultDouble))
                    return resultDouble;
            }
            return null;
        }
        #endregion

        #region ◆從指定路徑的 JSON 元素中讀取字串值 [TryReadPath]
        /// <summary>
        /// ◆從指定路徑的 JSON 元素中讀取字串值
        /// </summary>
        /// <param name="root">要讀取的 JSON</param>
        /// <param name="path">要讀取的值在 JSON 元素中的路徑。</param>
        /// <param name="value">當此方法返回時，如果成功，包含解析後的字串值；否則，包含預設值。</param>
        /// <returns>如果值成功讀取，則為 <see langword="true"/>；否則，為 <see langword="false"/>。</returns>
        private static bool TryReadPath(JsonElement root, string path, out string? value)
        {
            value = null;   // 初始化輸出參數
            var cur = root; // 從根元素開始遍歷 JSON 結構

            // 將路徑字串以 '.' 分割成多個部分，逐層尋找對應的 JSON 屬性
            foreach (var token in path.Split('.'))
            {
                // 嘗試從當前 JSON 元素中取得指定名稱的子元素，如果找不到或子元素不是物件，返回 false
                if (token.Contains('['))
                {
                    var name = token[..token.IndexOf('[')];                // 取得屬性名稱（不包含索引部分）
                    var idxText = token[(token.IndexOf('[') + 1)..token.IndexOf(']')]; // 取得索引部分的文字
                    if (!cur.TryGetProperty(name, out cur) || cur.ValueKind != JsonValueKind.Array) return false; // 如果找不到屬性或屬性不是陣列，返回 false
                    if (!int.TryParse(idxText, out var idx)) return false; // 如果索引部分無法解析為整數，返回 false
                    if (cur.GetArrayLength() <= idx) return false;         // 如果索引超出陣列範圍，返回 false
                    cur = cur[idx];
                }
                // 如果 token 不包含 '[', 直接嘗試取得對應的子元素
                else
                {
                    // 嘗試從當前 JSON 元素中取得指定名稱的子元素，如果找不到，返回 false
                    if (!cur.TryGetProperty(token, out cur)) return false;
                }
            }

            // 如果最終找到的 JSON 元素不是字串，返回 false
            value = cur.ValueKind switch
            {
                JsonValueKind.String => cur.GetString(),
                JsonValueKind.Number => cur.GetRawText(),
                _ => null
            };
            // 如果最終找到的 JSON 元素的值為 null 或空白字串，返回 false
            return !string.IsNullOrWhiteSpace(value);
        }
        #endregion

        #region ◆建立 IDW 網格 [BuildIdwGrid]
        /// <summary>
        /// ◆建立 IDW 網格 (Inverse Distance Weighting Grid，反距離加權網格)
        /// </summary>
        /// <param name="points">測站點集合</param>
        /// <param name="xmin">網格最小 X 座標</param>
        /// <param name="ymin">網格最小 Y 座標</param>
        /// <param name="xmax">網格最大 X 座標</param>
        /// <param name="ymax">網格最大 Y 座標</param>
        /// <param name="width">網格寬度（列數）</param>
        /// <param name="height">網格高度（行數）</param>
        /// <param name="power">IDW 計算的權重指數</param>
        /// <returns>返回計算後的 IDW 網格</returns>
        private static double[,] BuildIdwGrid(
            IReadOnlyList<StationPoint> points,
            double xmin, double ymin, double xmax, double ymax,
            int width, int height, double power)
        {
            // 用 Math.NET 建立向量（示範在本流程中可用於後續擴充矩陣計算）
            var xs = Vector<double>.Build.DenseOfEnumerable(points.Select(p => p.Longitude));
            var ys = Vector<double>.Build.DenseOfEnumerable(points.Select(p => p.Latitude));
            var vs = Vector<double>.Build.DenseOfEnumerable(points.Select(p => p.Value));

            var grid = new double[height, width];
            var dx = (xmax - xmin) / (width - 1);
            var dy = (ymax - ymin) / (height - 1);

            for (var r = 0; r < height; r++)
            {
                var y = ymax - r * dy;
                for (var c = 0; c < width; c++)
                {
                    var x = xmin + c * dx;
                    double wSum = 0, vSum = 0;

                    for (var i = 0; i < points.Count; i++)
                    {
                        var dist = Math.Sqrt(Math.Pow(x - xs[i], 2) + Math.Pow(y - ys[i], 2));
                        if (dist < 1e-9) { wSum = 1; vSum = vs[i]; break; }

                        var w = 1.0 / Math.Pow(dist, power);
                        wSum += w;
                        vSum += w * vs[i];
                    }

                    grid[r, c] = wSum > 0 ? vSum / wSum : double.NaN;
                }
            }
            return grid;
        }
        #endregion

    }//class end
}//namespace end
