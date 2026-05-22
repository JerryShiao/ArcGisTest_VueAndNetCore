using ArcGisTest_VueAndNetCore.Server.Model.WeatherImagery;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArcGisTest_VueAndNetCore.Server.Service.WeatherImagery
{
    /// <summary>
    /// 天氣圖磚服務，負責處理與天氣圖磚相關的業務邏輯
    /// </summary>
    public class WeatherTileService
    {
        /// <summary>
        /// 天氣內插服務
        /// </summary>
        private readonly WeatherInterpolationService _weatherInterpolationService;
        /// <summary>
        /// 天氣影像選項類別
        /// </summary>
        private readonly WeatherImageryOptions _weatherImageryOptions;

        #region ◆建構子 [WeatherTileService]
        /// <summary>
        /// ◆建構子
        /// </summary>
        /// <param name="weatherInterpolationService">天氣內插服務</param>
        /// <param name="weatherImageryOptions">天氣影像選項類別</param>
        public WeatherTileService(WeatherInterpolationService weatherInterpolationService, IOptions<WeatherImageryOptions> weatherImageryOptions)
        {
            _weatherInterpolationService = weatherInterpolationService;
            _weatherImageryOptions = weatherImageryOptions.Value;
        }
        #endregion

        #region ◆獲取指定圖磚的天氣圖像 [RenderTileAsync]
        /// <summary>
        /// ◆獲取指定圖磚的天氣圖像
        /// </summary>
        /// <param name="z">圖層的縮放級別</param>
        /// <param name="y">圖磚的 Y 座標</param>
        /// <param name="x">圖磚的 X 座標</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>圖磚的 PNG 圖像資料</returns>
        public async Task<byte[]> RenderTileAsync(int z, int y, int x, CancellationToken cancellationToken = default)
        {
            const int tileSize = 256; // 圖磚的大小為 256x256 像素
            var grid = await _weatherInterpolationService.OrBuildGridGetAsync(cancellationToken); // 獲取天氣內插網格數據            
            using var img = new Image<Rgba32>(tileSize, tileSize); // 創建圖像對象

            // 計算圖磚的地理範圍
            for (var py = 0; py < tileSize; py++)
            {
                for (var px = 0; px < tileSize; px++)
                {
                    // 計算圖像中每個像素對應的地理坐標
                    var (lon, lat) = TilePixelToLonLat(z, x, y, px, py, tileSize);

                    // 根據地理坐標從內插網格中獲取天氣數據
                    if (lon < _weatherImageryOptions.Extent.XMin || lon > _weatherImageryOptions.Extent.XMax || lat < _weatherImageryOptions.Extent.YMin || lat > _weatherImageryOptions.Extent.YMax)
                    {
                        img[px, py] = new Rgba32(0, 0, 0, 0);
                        continue;
                    }

                    // 根據內插網格數據取得天氣數值
                    var resultValue = SampleNearest(grid, lon, lat);
                    // 根據天氣數據值映射到顏色（這裡使用簡單的灰度映射，實際應用中可以使用更複雜的配色方案）
                    img[px, py] = ColorRamp(resultValue);
                }
            }

            // 將圖像保存為 PNG 格式並返回圖像數據
            using var memoryStream = new MemoryStream();
            await img.SaveAsPngAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }
        #endregion

        #region ◆根據經緯度從內插網格中獲取最近的天氣數值 [SampleNearest]
        /// <summary>
        /// ◆根據經緯度從內插網格中獲取最近的天氣數值
        /// </summary>
        /// <param name="grid">內插網格數據</param>
        /// <param name="longitude">經度</param>
        /// <param name="latitude">緯度</param>
        /// <returns>最近的天氣數值</returns>
        private double SampleNearest(double[,] grid, double longitude, double latitude)
        {
            var w = grid.GetLength(1); // 網格的寬度
            var h = grid.GetLength(0); // 網格的高度

            // 計算經緯度對應的網格索引
            var u = (longitude - _weatherImageryOptions.Extent.XMin) / (_weatherImageryOptions.Extent.XMax - _weatherImageryOptions.Extent.XMin);
            var v = (_weatherImageryOptions.Extent.YMax - latitude) / (_weatherImageryOptions.Extent.YMax - _weatherImageryOptions.Extent.YMin);

            // 計算最近的網格索引
            var c = Math.Clamp((int)Math.Round(u * (w - 1)), 0, w - 1);
            var r = Math.Clamp((int)Math.Round(v * (h - 1)), 0, h - 1);

            // 返回最近的天氣數值
            return grid[r, c];
        }
        #endregion

        #region ◆圖磚像素轉經緯度 [TilePixelToLonLat]
        /// <summary>
        ///  ◆圖磚像素轉經緯度
        /// </summary>
        /// <param name="z">縮放級別</param>
        /// <param name="x">圖磚的 X 座標</param>
        /// <param name="y">圖磚的 Y 座標</param>
        /// <param name="px">圖像中的像素 X 座標</param>
        /// <param name="py">圖像中的像素 Y 座標</param>
        /// <param name="tileSize">圖磚的大小</param>
        /// <returns>經緯度座標</returns>
        private static (double longitude, double latitude) TilePixelToLonLat(int z, int x, int y, int px, int py, int tileSize)
        {
            var n = Math.Pow(2, z); // 縮放級別對應的圖磚數量
            var xf = (x + (px / (double)tileSize)) / n; // 圖磚 X 座標加上像素偏移量，然後除以圖磚數量得到經度比例
            var yf = (y + (py / (double)tileSize)) / n; // 圖磚 Y 座標加上像素偏移量，然後除以圖磚數量得到緯度比例

            var longitude = xf * 360.0 - 180.0; // 將經度比例轉換為經度值
            var latitude = 90.0 - yf * 180.0;   // 將緯度比例轉換為緯度值
            return (longitude, latitude);       // 返回經緯度座標
        }
        #endregion

        #region ◆顏色映射 [ColorRamp]
        /// <summary>
        /// ◆顏色映射
        /// </summary>
        /// <param name="weatherValue">天氣數值</param>
        /// <returns>對應的顏色</returns>
        private static Rgba32 ColorRamp(double weatherValue)
        {
            // 簡易溫度色帶：0~40
            var t = (float)Math.Clamp((weatherValue - 0) / 40.0, 0, 1);
            byte r = (byte)(255 * t);
            byte b = (byte)(255 * (1 - t));
            return new Rgba32(r, 80, b, 180);
        }
        #endregion

        /// <summary>
        /// 依指定地理範圍與尺寸產生天氣影像（用於 ImageServer /exportImage）
        /// </summary>
        /// <param name="xmin">最小經度</param>
        /// <param name="ymin">最小緯度</param>
        /// <param name="xmax">最大經度</param>
        /// <param name="ymax">最大緯度</param>
        /// <param name="width">輸出影像寬度（像素）</param>
        /// <param name="height">輸出影像高度（像素）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>PNG 影像位元組陣列</returns>
        public async Task<byte[]> RenderImageAsync(
            double xmin, double ymin, double xmax, double ymax,
            int width, int height, CancellationToken cancellationToken = default)
        {
            var grid = await _weatherInterpolationService.OrBuildGridGetAsync(cancellationToken);
            using var img = new Image<Rgba32>(width, height);

            for (var py = 0; py < height; py++)
            {
                for (var px = 0; px < width; px++)
                {
                    // 線性插值計算該像素對應的經緯度
                    var lon = xmin + (xmax - xmin) * (px / (double)(width - 1));
                    var lat = ymax - (ymax - ymin) * (py / (double)(height - 1));

                    if (lon < _weatherImageryOptions.Extent.XMin || lon > _weatherImageryOptions.Extent.XMax ||
                        lat < _weatherImageryOptions.Extent.YMin || lat > _weatherImageryOptions.Extent.YMax)
                    {
                        img[px, py] = new Rgba32(0, 0, 0, 0);
                        continue;
                    }

                    var value = SampleNearest(grid, lon, lat);
                    img[px, py] = ColorRamp(value);
                }
            }

            using var ms = new MemoryStream();
            await img.SaveAsPngAsync(ms, cancellationToken);
            return ms.ToArray();
        }

    }//class end
}//namespace end
