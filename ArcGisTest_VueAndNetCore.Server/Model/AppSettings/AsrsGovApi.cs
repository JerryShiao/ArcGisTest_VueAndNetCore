namespace ArcGisTest_VueAndNetCore.Server.Model.AppSettings
{
    /// <summary>
    /// 航遙測圖資 API 設定檔配置類別
    /// </summary>
    public class AsrsGovApi
    {
        /// <summary>
        /// 連線帳號
        /// </summary>
        private string connectAccount = string.Empty;
        /// <summary>
        /// 連線帳號
        /// </summary>
        public string ConnectAccount
        {
            get => connectAccount;
            set => connectAccount = value;
        }

        /// <summary>
        /// 認證密碼
        /// </summary>
        private string connectPassword = string.Empty;
        /// <summary>
        /// 認證密碼
        /// </summary>
        public string ConnectPassword
        {
            get => connectPassword;
            set => connectPassword = value;
        }

        /// <summary>
        /// 航照-全臺正射影像(WMS) 服務的 URL
        /// </summary>
        private string aerialPhotographyWmsUrl = string.Empty;
        /// <summary>
        /// 航照-全臺正射影像(WMS) 服務的 URL
        /// </summary>
        public string AerialPhotographyWmsUrl
        {
            get => aerialPhotographyWmsUrl;
            set => aerialPhotographyWmsUrl = value;
        }

        /// <summary>
        /// 無人機鑲嵌正射影像(WMS) 服務的 URL
        /// </summary>
        private string uavWmsUrl = string.Empty;
        /// <summary>
        /// 無人機鑲嵌正射影像(WMS) 服務的 URL
        /// </summary>
        public string UavWmsUrl
        {
            get => uavWmsUrl;
            set => uavWmsUrl = value;
        }

        /// <summary>
        /// 福衛影像(WMS) 服務的 URL
        /// </summary>
        private string fs2WmsUrl = string.Empty;
        /// <summary>
        /// 福衛影像(WMS) 服務的 URL
        /// </summary>
        public string Fs2WmsUrl
        {
            get => fs2WmsUrl;
            set => fs2WmsUrl = value;
        }

        /// <summary>
        /// 福衛影像(WMTS) 服務的 URL
        /// </summary>
        private string fs2WmtsUrl = string.Empty;
        /// <summary>
        /// 福衛影像(WMTS) 服務的 URL
        /// </summary>
        public string Fs2WmtsUrl
        {
            get => fs2WmtsUrl;
            set => fs2WmtsUrl = value;
        }

    }//class end
}//namespace end
