namespace ArcGisTest_VueAndNetCore.Server.Model.WeatherImagery
{
    /// <summary>
    ///  天氣測站點位資料
    /// </summary>
    /// <param name="Longitude">經度</param>
    /// <param name="Latitude">緯度</param>
    /// <param name="Value">測站值</param>
    public sealed record StationPoint(double Longitude, double Latitude, double Value);
}//namespace end
