namespace ArcGisTest_VueAndNetCore.Server.Model.WeatherImagery
{
    /// <summary>
    /// 天氣影像選項類別
    /// </summary>
    public class WeatherImageryOptions
    {
        /// <summary>
        /// 範圍選項
        /// </summary>
        public ExtentOptions Extent { get; set; } = new();

        /// <summary>
        /// 網格寬度
        /// </summary>
        public int GridWidth { get; set; } = 512;
        /// <summary>
        /// 網格高度
        /// </summary>
        public int GridHeight { get; set; } = 512;
        /// <summary>
        /// 反距離加權（IDW）內插的冪參數
        /// </summary>
        public double IdwPower { get; set; } = 2.0;
        /// <summary>
        /// 快取時間（分鐘）
        /// </summary>
        public int CacheMinutes { get; set; } = 10;

    }//class end
}//namespace end
