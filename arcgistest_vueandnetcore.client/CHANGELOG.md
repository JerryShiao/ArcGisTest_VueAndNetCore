此檔案說明 Visual Studio 建立專案的方式。

下列工具用來產生此專案:
- create-vite

下列步驟用來產生此專案:
- 使用 create-vite 建立 vue 專案: `npm init --yes vue@latest arcgistest_vueandnetcore.client -- --eslint `。
- 更新 `vite.config.js` 以設定 Proxy 處理和憑證。
- 更新 `HelloWorld` 元件以擷取和顯示天氣資訊。
- 建立專案檔 (`arcgistest_vueandnetcore.client.esproj`)。
- 建立 `launch.json` 以啟用偵錯功能。
- 將專案新增至解決方案。
- 將 Proxy 端點更新為後端伺服器端點。
- 將專案新增至啟動專案清單。
- 寫入此檔案。
