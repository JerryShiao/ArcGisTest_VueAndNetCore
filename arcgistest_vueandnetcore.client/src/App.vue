<template>
  <!-- 地圖會顯示在這個 div 裡面 -->
  <div id="viewDiv"></div>
</template>

<style>
  header {
    line-height: 1.5;
  }

  .logo {
    display: block;
    margin: 0 auto 2rem;
  }

  @media (min-width: 1024px) {
    header {
      display: flex;
      place-items: center;
      padding-right: calc(var(--section-gap) / 2);
    }

    .logo {
      margin: 0 2rem 0 0;
    }

    header .wrapper {
      display: flex;
      place-items: flex-start;
      flex-wrap: wrap;
    }
  }

  /*============================================================*/
  /*【圖台Style】*/
  html, body, #app, #viewDiv {
    padding: 0;
    margin: 0;
    height: 100%;
    width: 100%;
  }

  /* 自定義按鈕樣式 */
  .custom-button {
    background-color: #0079c1;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 4px;
    cursor: pointer;
    font-size: 14px;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
  }

    .custom-button:hover {
      background-color: #005a87;
    }

  /* 自定義選單樣式 */
  .custom-menu {
    background-color: white;
    padding: 15px;
    border-radius: 4px;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
    min-width: 200px;
  }

    .custom-menu h3 {
      margin: 0 0 10px 0;
      font-size: 16px;
      color: #323232;
    }

    .custom-menu select,
    .custom-menu button {
      width: 100%;
      padding: 8px;
      margin-bottom: 8px;
      border-radius: 3px;
      border: 1px solid #ccc;
    }

    .custom-menu button {
      background-color: #0079c1;
      color: white;
      border: none;
      cursor: pointer;
    }

      .custom-menu button:hover {
        background-color: #005a87;
      }

  /* 圖標按鈕樣式 */
  .icon-button {
    background-color: white;
    color: #323232;
    border: none;
    padding: 12px;
    border-radius: 50%;
    cursor: pointer;
    font-size: 18px;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
    width: 40px;
    height: 40px;
  }

    .icon-button:hover {
      background-color: #f3f3f3;
    }

  /* 路徑規劃面板樣式 */
  .route-panel {
    background-color: white;
    padding: 15px;
    border-radius: 4px;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
    min-width: 250px;
    max-width: 300px;
  }

    .route-panel h3 {
      margin: 0 0 10px 0;
      font-size: 16px;
      color: #323232;
      border-bottom: 2px solid #0079c1;
      padding-bottom: 5px;
    }

    .route-panel button {
      width: 100%;
      padding: 10px;
      margin: 5px 0;
      border-radius: 3px;
      border: none;
      cursor: pointer;
      font-size: 14px;
      transition: all 0.3s;
    }

    .route-panel .btn-primary {
      background-color: #0079c1;
      color: white;
    }

      .route-panel .btn-primary:hover {
        background-color: #005a87;
      }

    .route-panel .btn-success {
      background-color: #28a745;
      color: white;
    }

      .route-panel .btn-success:hover {
        background-color: #218838;
      }

    .route-panel .btn-danger {
      background-color: #dc3545;
      color: white;
    }

      .route-panel .btn-danger:hover {
        background-color: #c82333;
      }

    .route-panel .btn-secondary {
      background-color: #6c757d;
      color: white;
    }

      .route-panel .btn-secondary:hover {
        background-color: #5a6268;
      }

    .route-panel .status {
      padding: 8px;
      margin: 10px 0;
      border-radius: 3px;
      font-size: 13px;
      text-align: center;
    }

      .route-panel .status.info {
        background-color: #d1ecf1;
        color: #0c5460;
        border: 1px solid #bee5eb;
      }

      .route-panel .status.success {
        background-color: #d4edda;
        color: #155724;
        border: 1px solid #c3e6cb;
      }

      .route-panel .status.error {
        background-color: #f8d7da;
        color: #721c24;
        border: 1px solid #f5c6cb;
      }

    .route-panel .route-info {
      background-color: #f8f9fa;
      padding: 10px;
      border-radius: 3px;
      margin: 10px 0;
      font-size: 13px;
    }

      .route-panel .route-info div {
        margin: 5px 0;
      }

      .route-panel .route-info strong {
        color: #0079c1;
      }
  /*============================================================*/
  /* WMTS 圖層在 3D 模式下停用時的視覺提示 */
  label:has(input[type="checkbox"]:disabled) {
    opacity: 0.45;
    cursor: not-allowed;
  }
  /*============================================================*/
</style>

<!--初始化-->
<script setup lang="ts">
  import { getFs2WmsLayer, getFs2WmtsLayer } from "@/api/wmsLayer.js";   // 引入 WMS/WMTS 圖層 API
  import { getWeatherImageryLayer } from "@/api/weatherImageryLayer.js"; // 引入天氣圖 API
  try {
    // 使用 require 載入模組
    // 加上 @ts-ignore 後，TypeScript 會忽略下一行的型別檢查，錯誤「應有 1 個引數，但得到 2 個」就不會再出現了。
    require(["esri/Map",
      "esri/views/SceneView",
      "esri/views/MapView",
      "esri/widgets/BasemapGallery",
      "esri/Basemap",
      "esri/layers/WMSLayer",
      "esri/layers/WMTSLayer",
      "esri/layers/ImageryLayer",      // 影像圖層模組
      "esri/layers/ElevationLayer",    // 高程圖層模組
      "esri/identity/IdentityManager",
      "esri/Graphic",
      "esri/layers/ImageryTileLayer"        // 影像切片圖層模組
    ],
      // @ts-ignore
      (Map, SceneView, MapView, BasemapGallery, Basemap, WMSLayer, WMTSLayer,
        ImageryLayer, ElevationLayer, esriId, Graphic, ImageryTileLayer) => {

        //#region -- 自動還原 ArcGIS 登入憑證 (避免每次重新登入)
        const CREDENTIALS_KEY = "esriCredentials";

        // 從 localStorage 還原上次儲存的登入憑證
        const savedCredentials = localStorage.getItem(CREDENTIALS_KEY);
        if (savedCredentials) {
          try {
            esriId.initialize(JSON.parse(savedCredentials));
          } catch (e) {
            // 若憑證格式有誤或已過期，清除後重新登入
            localStorage.removeItem(CREDENTIALS_KEY);
          }
        }

        // 每當新憑證建立時（登入後），自動儲存到 localStorage
        esriId.on("credential-create", () => {
          try {
            localStorage.setItem(CREDENTIALS_KEY, JSON.stringify(esriId.toJSON()));
          } catch (e) {
            console.warn("[IdentityManager] 儲存憑證失敗，已略過：", e);
          }
        });

        // 攔截 IdentityManager 內部的非同步錯誤，避免中斷系統
        window.addEventListener("unhandledrejection", (event) => {
          const reason = event.reason;
          const msg = reason?.message ?? String(reason ?? "");
          // 僅吞掉來自 IdentityManager 的錯誤（例如：登入取消、Token 過期等）
          if (
            msg.includes("identity") ||
            msg.includes("token") ||
            msg.includes("credential") ||
            msg.includes("IdentityManager") ||
            reason?.name === "identity:not-authorized" ||
            reason?.code === "identity:not-authorized"
          ) {
            console.warn("[IdentityManager] 已攔截非同步錯誤，不中斷系統：", reason);
            event.preventDefault(); // 阻止瀏覽器顯示 Uncaught error
          }
        });
        //#endregion

        //#region -- 房價圖層初始化
        const HOUSE_PRICE_IMAGE_SERVER = "/api-ltgis/server/rest/services/test/Natural_Getresidential20260518_tif/ImageServer";

        let housePriceImageryLayer: any = null;
        let housePriceElevationLayer: any = null;

        // 先透過 fetch 查詢服務統計，確認範圍後再建立圖層，避免 LayerView 建立失敗
        const initHousePriceLayers = async (retryCount = 0, maxRetries = 20, retryDelay = 3000) => {
          try {
            // 直接查詢 ImageServer 根端點取得服務資訊（minValues / maxValues）
            const res = await fetch(`${HOUSE_PRICE_IMAGE_SERVER}?f=json`);
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            const text = await res.text();
            if (!text || text.trim() === "") throw new Error("服務回傳空白內容");
            const info = JSON.parse(text);
            console.log("[房價服務] 服務資訊:", info);

            // 從服務根端點取得像素值範圍
            let minVal: number = info?.minValues?.[0] ?? info?.statistics?.[0]?.min ?? 0;
            let maxVal: number = info?.maxValues?.[0] ?? info?.statistics?.[0]?.max ?? 1;

            // 若 min/max 都為 0，嘗試從 /statistics 端點補查
            if (minVal === 0 && maxVal === 0) {
              try {
                const res2 = await fetch(`${HOUSE_PRICE_IMAGE_SERVER}/statistics?f=json`);
                const stats = await res2.json();
                console.log("[房價服務] /statistics:", stats);
                minVal = stats?.[0]?.min ?? stats?.statistics?.[0]?.min ?? 0;
                maxVal = stats?.[0]?.max ?? stats?.statistics?.[0]?.max ?? 1;
              } catch (_) { /* 補查失敗則維持原值 */ }
            }

            console.log(`[房價服務] 像素值範圍: ${minVal} ~ ${maxVal}`);
            // 建立 ImageryLayer（彩色熱力圖）
            housePriceImageryLayer = new ImageryLayer({
              url: HOUSE_PRICE_IMAGE_SERVER,
              opacity: 0.7,
              visible: false,
              renderer: {
                type: "raster-stretch",
                stretchType: "min-max",
                statistics: [{
                  min: minVal,
                  max: maxVal,
                  avg: (minVal + maxVal) / 2,
                  stddev: (maxVal - minVal) / 6
                }],
                colorRamp: {
                  type: "multipart",
                  colorRamps: [
                    { fromColor: [0, 0, 255, 255], toColor: [0, 255, 0, 255] },  // 低房價：藍 → 綠
                    { fromColor: [0, 255, 0, 255], toColor: [255, 0, 0, 255] }   // 高房價：綠 → 紅
                  ]
                }
              } as any
            });

            // 建立 ElevationLayer（地形依房價高低起伏）
            // targetMaxMeters：最高房價對應的公尺數，值越小 → 起伏越誇張
            const targetMaxMeters = 1;
            const scaleFactor = maxVal > 0 ? maxVal / targetMaxMeters : 1;
            console.log(`[房價服務] ElevationLayer scaleFactor=${scaleFactor.toFixed(2)}`);

            housePriceElevationLayer = new ElevationLayer({
              url: HOUSE_PRICE_IMAGE_SERVER,
              // noDataInterpretation: "no-data-value"：NoData 區域（無房價資料）穿透到下層 world-elevation，
              // 避免整片地形被 0 值覆蓋成平地
              noDataInterpretation: "no-data-value" as any,
              renderingRule: {
                rasterFunction: "ArithmeticFunction",
                rasterFunctionArguments: {
                  Operation: 4,         // 4 = Divide
                  Raster2: scaleFactor,
                  ExtentType: 1,
                  CellsizeType: 0
                }
              } as any
            });

            // 啟用 checkbox
            housePriceCheckbox.disabled = false;
            housePriceCheckbox.title = "";
            console.log("[房價服務] 圖層初始化完成，可勾選顯示。");
          }
          catch (err) {
            console.error("[房價服務] 初始化失敗：", err);
            if (retryCount < maxRetries) {
              console.warn(`[房價服務] ${retryDelay / 1000} 秒後自動重試 (${retryCount + 1}/${maxRetries})...`);
              housePriceCheckbox.title = `服務載入失敗，${retryDelay / 1000} 秒後自動重試 (${retryCount + 1}/${maxRetries})`;
              setTimeout(() => initHousePriceLayers(retryCount + 1, maxRetries, retryDelay), retryDelay);
            } else {
              console.error("[房價服務] 已達最大重試次數，停止重試。");
              housePriceCheckbox.title = "服務載入失敗，請檢查主控台日誌";
            }
          }
        };
        //#endregion

        //#region ◆台灣電子地圖 (WMTS)
        // 創建底圖
        const taiwanWmtsLayer = new WMTSLayer({
          url: "https://wmts.nlsc.gov.tw/wmts",
          activeLayer: {
            id: "EMAP"  // 電子地圖圖層 ID
          },
          serviceMode: "KVP",  // 或 "RESTful"
          title: "台灣電子地圖 (WMTS)"
        });

        // 創建 Basemap 並使用 WMTS 圖層
        const taiwanWmtsBasemap = new Basemap({
          baseLayers: [taiwanWmtsLayer],
          title: "台灣電子地圖 (WMTS)",
          thumbnailUrl: new URL('./assets/taiwan-wmts-thumbnail.svg', import.meta.url).href
        });
        //#endregion

        //#region ◆衛星影像底圖 (自定義繁體中文標題)
        // 先創建標準底圖，然後克隆並修改屬性
        const imageryBasemap = Basemap.fromId("arcgis-imagery").clone();
        imageryBasemap.title = "衛星影像";
        //#endregion

        //#region ◆街道圖底圖 (自定義繁體中文標題)
        // 先創建標準底圖，然後克隆並修改屬性
        const streetsBasemap = Basemap.fromId("arcgis-streets").clone();
        streetsBasemap.title = "街道圖";
        //#endregion

        //#region ◆創建地圖實例
        const map = new Map({
          basemap: taiwanWmtsBasemap,
          ground: "world-elevation" // 3D 地形
        });
        //#endregion

        //#region ◆創建 3D 視圖並關聯到 HTML 元素
        const view = new SceneView({
          container: "viewDiv", // 對應 HTML 標籤的 ID
          map: map,
          camera: {
            position: { x: 120.68, y: 24.15, z: 50000 },
            tilt: 45,  // 傾斜角度 (0-90°，0=俯視，90=平視)
            heading: 0 // 方向角度 (0-360°，0=正北)
          },
          // 垂直誇張：放大地形高低差異的視覺效果
          // 若起伏還是不明顯，可進一步加大此值（最高 8）
          environment: {
            verticalExaggeration: 8
          }
        });

        // 2D MapView（共用同一 Map 實例，初始不掛載 container）
        const mapView = new MapView({
          map: map,
          zoom: 8,
          center: [120.68, 24.15]
        });

        let isSceneView = true;        // 目前是否為 3D 模式
        let currentView: any = view;   // 目前作用中的 view
        //#endregion

        //#region --圖層選單
        const layerMenu = document.createElement("div");
        layerMenu.className = "custom-menu";

        const menuTitle = document.createElement("h3");
        menuTitle.textContent = "圖層選單";
        layerMenu.appendChild(menuTitle);

        //#region ◆2015年全臺福衛二號影像 (WMS)
        const fs2WmsLabel = document.createElement("label");
        fs2WmsLabel.style.cssText = "display: flex; align-items: center; cursor: pointer; padding: 5px 0;";

        const fs2WmsCheckbox = document.createElement("input");
        fs2WmsCheckbox.type = "checkbox";
        fs2WmsCheckbox.id = "toggleWmsLayer";
        fs2WmsCheckbox.style.cssText = "margin-right: 8px; cursor: pointer;";

        let fs2WmsLayer: any = null;

        // 帶重試的預先載入：最多重試 20 次，每次失敗延遲 3 秒後再試
        const preloadFs2WmsLayer = async (maxRetries = 20, delayMs = 3000) => {
          for (let attempt = 1; attempt <= maxRetries; attempt++) {
            try {
              const layerInfo = await getFs2WmsLayer();
              fs2WmsLayer = new WMSLayer({
                url: layerInfo.url,
                sublayers: [{ name: layerInfo.layerName, visible: true }],
                visible: false
              });
              // 先明確等待 GetCapabilities 載入完成，確保 load 失敗時能被 catch 捕捉並重試
              await fs2WmsLayer.load();
              map.add(fs2WmsLayer);

              console.log("2015年全臺福衛二號影像 (WMS) 預先載入成功", layerInfo);
              return; // 成功即結束
            } catch (err) {
              // 確保損壞的 layer 物件不會殘留，讓下次重試重新建立
              if (fs2WmsLayer) {
                try { map.remove(fs2WmsLayer); } catch (_) { }
                fs2WmsLayer = null;
              }
              console.warn(`WMS 圖層預先載入失敗（第 ${attempt}/${maxRetries} 次）：`, err);
              if (attempt < maxRetries) {
                await new Promise(resolve => setTimeout(resolve, delayMs));
              } else {
                console.error("WMS 圖層預先載入已達重試上限，放棄。");
              }
            }
          }
        };
        preloadFs2WmsLayer();

        fs2WmsCheckbox.addEventListener("click", () => {
          const visible = fs2WmsCheckbox.checked;

          if (fs2WmsLayer) {
            fs2WmsLayer.visible = visible;
          } else {
            console.warn("WMS 圖層尚未載入完成，請稍後再試");
            fs2WmsCheckbox.checked = false;
          }
        });

        const fs2WmsSpan = document.createElement("span");
        fs2WmsSpan.textContent = "🗺️ 2015年全臺福衛二號影像 (WMS)";

        fs2WmsLabel.appendChild(fs2WmsCheckbox);
        fs2WmsLabel.appendChild(fs2WmsSpan);
        layerMenu.appendChild(fs2WmsLabel);
        //#endregion

        //#region ◆2015年全臺福衛二號影像 (WMTS)
        const fs2WmtsLabel = document.createElement("label");
        fs2WmtsLabel.style.cssText = "display: flex; align-items: center; cursor: pointer; padding: 5px 0;";

        const fs2WmtsCheckbox = document.createElement("input");
        fs2WmtsCheckbox.type = "checkbox";
        fs2WmtsCheckbox.id = "toggleWmtsLayer";
        fs2WmtsCheckbox.style.cssText = "margin-right: 8px; cursor: pointer;";
        fs2WmtsCheckbox.disabled = true; // 3D 模式不支援，切換至 2D 後才啟用

        let fs2WmtsLayer: any = null;

        // 帶重試的預先載入：最多重試 20 次，每次失敗延遲 3 秒後再試
        const preloadFs2WmtsLayer = async (maxRetries = 20, delayMs = 3000) => {
          for (let attempt = 1; attempt <= maxRetries; attempt++) {
            try {
              const layerInfo = await getFs2WmtsLayer();
              fs2WmtsLayer = new WMTSLayer({
                url: layerInfo.url,
                activeLayer: {
                  id: layerInfo.layerName,
                  tileMatrixSetId: "EPSG:900913" // 直接在建構時指定 Web Mercator，與 SceneView 相容
                },
                visible: false
              });
              // 先明確等待 GetCapabilities 載入完成，確保 load 失敗時能被 catch 捕捉並重試
              await fs2WmtsLayer.load();

              console.log("2015年全臺福衛二號影像 (WMTS) 預先載入成功", layerInfo);
              return; // 成功即結束
            } catch (err) {
              // 確保損壞的 layer 物件不會殘留，讓下次重試重新建立
              if (fs2WmtsLayer) {
                try { map.remove(fs2WmtsLayer); } catch (_) { }
                fs2WmtsLayer = null;
              }
              console.warn(`WMTS 圖層預先載入失敗（第 ${attempt}/${maxRetries} 次）：`, err);
              if (attempt < maxRetries) {
                await new Promise(resolve => setTimeout(resolve, delayMs));
              } else {
                console.error("WMTS 圖層預先載入已達重試上限，放棄。");
              }
            }
          }
        };
        preloadFs2WmtsLayer();

        fs2WmtsCheckbox.addEventListener("click", () => {
          const visible = fs2WmtsCheckbox.checked;

          if (fs2WmtsLayer) {
            fs2WmtsLayer.visible = visible;
          } else {
            console.warn("WMTS 圖層尚未載入完成，請稍後再試");
            fs2WmtsCheckbox.checked = false;
          }
        });

        const fs2WmtsSpan = document.createElement("span");
        fs2WmtsSpan.textContent = "🗺️ 2015年全臺福衛二號影像 (WMTS，僅2D)";

        fs2WmtsLabel.appendChild(fs2WmtsCheckbox);
        fs2WmtsLabel.appendChild(fs2WmtsSpan);
        layerMenu.appendChild(fs2WmtsLabel);
        //#endregion

        //#region ◆時價登錄房價 (ImageryLayer + ElevationLayer)
        const housePriceLabel = document.createElement("label");
        housePriceLabel.style.cssText = "display: flex; align-items: center; cursor: pointer; padding: 5px 0;";

        const housePriceCheckbox = document.createElement("input");
        housePriceCheckbox.type = "checkbox";
        housePriceCheckbox.style.cssText = "margin-right: 8px; cursor: pointer;";
        housePriceCheckbox.disabled = true; // 統計載入完成前停用
        housePriceCheckbox.title = "載入中，請稍候...";

        housePriceCheckbox.addEventListener("click", () => {
          if (!housePriceImageryLayer || !housePriceElevationLayer) {
            console.warn("[房價圖層] 尚未初始化完成，請稍後再試");
            housePriceCheckbox.checked = false;
            return;
          }

          const visible = housePriceCheckbox.checked;

          if (visible) {
            // 加入彩色房價影像圖層
            if (!map.layers.includes(housePriceImageryLayer)) {
              map.add(housePriceImageryLayer);
            }
            housePriceImageryLayer.visible = true;
            // 加入房價高程圖層（地形依房價高低起伏）
            if (!map.ground.layers.includes(housePriceElevationLayer)) {
              map.ground.layers.add(housePriceElevationLayer);
            }
          } else {
            housePriceImageryLayer.visible = false;
            if (map.layers.includes(housePriceImageryLayer)) {
              map.remove(housePriceImageryLayer);
            }
            if (map.ground.layers.includes(housePriceElevationLayer)) {
              map.ground.layers.remove(housePriceElevationLayer);
            }
          }
        });

        const housePriceSpan = document.createElement("span");
        housePriceSpan.textContent = "🏠 時價登錄房價 (地形3D視覺化)";

        housePriceLabel.appendChild(housePriceCheckbox);
        housePriceLabel.appendChild(housePriceSpan);
        layerMenu.appendChild(housePriceLabel);
        //#endregion

        //#region ◆氣象內插圖層
        let weatherImageryLayer = null;
        // 在地圖建立後加到 map
        async function createWeatherImageryLayer() {
          try {
            weatherImageryLayer = null; // 確保不會有殘留的圖層物件
            const layerUrl = await getWeatherImageryLayer();

            //測試
            console.log("氣象內插圖層資訊：", layerUrl);

            weatherImageryLayer = new ImageryTileLayer({
              url: layerUrl
            });
            // 加入地圖
            map.add(weatherImageryLayer);
          }
          catch (err) {
            console.error("氣象內插圖層載入失敗：", err);
          }
        }
        if (!weatherImageryLayer) {
          console.warn("氣象內插圖層尚未建立成功，相關功能將無法使用");
        }
        else {
          //顯示URL
          console.info("氣象內插圖層 URL:", weatherImageryLayer.url);
          console.info("氣象內插圖層已成功建立，正在加入地圖...");
          // 監聽圖層載入完成事件
          weatherImageryLayer.when(() => {
            console.log("氣象圖層載入完成", weatherImageryLayer);
          }, (error) => {
            console.error("氣象圖層載入失敗", error);
            console.warn("氣象圖層尚未建立成功，無法切換顯示");
          });

          // 監聽圖層可見性變化
          weatherImageryLayer.watch("visible", (newValue) => {
            console.log("氣象圖層可見性變化:", newValue);
          });
        }

        // 加入 checkbox 控制顯示
        const weatherLabel = document.createElement("label");
        weatherLabel.style.cssText = "display: flex; align-items: center; cursor: pointer; padding: 5px 0;";
        const weatherCheckbox = document.createElement("input");
        weatherCheckbox.type = "checkbox";
        weatherCheckbox.style.cssText = "margin-right: 8px; cursor: pointer;";
        weatherCheckbox.addEventListener("click", () => {
          if (!weatherImageryLayer) {            
            weatherCheckbox.checked = false;
            return;
          }
          const visible = weatherCheckbox.checked;
          if (visible) {
            map.add(weatherImageryLayer);
          }
          else {
            map.remove(weatherImageryLayer);
          }
        });
        const weatherSpan = document.createElement("span");
        weatherSpan.textContent = "🌦️ 氣象內插圖層 (僅2D)";
        weatherLabel.appendChild(weatherCheckbox);
        weatherLabel.appendChild(weatherSpan);

        // 加入圖層選單
        layerMenu.appendChild(weatherLabel);
        //#endregion

        // 將選單添加到右上角
        view.ui.add(layerMenu, "top-right");

        // 非同步初始化房價圖層（統計取得後才啟用 checkbox）
        initHousePriceLayers();
        //#endregion

        //#region ◆底圖切換功能
        const basemapGallery = new BasemapGallery({
          view: view,
          source: [
            taiwanWmtsBasemap, // 台灣電子地圖 (WMTS)
            imageryBasemap,    // 衛星影像
            streetsBasemap     // 街道圖
          ]
        });
        //#endregion

        //#region ◆創建可展開的底圖選擇器
        const bgExpand = view.ui.add({
          component: basemapGallery,
          position: "bottom-left",
          expanded: false
        });
        //#endregion

        //#region ◆MapView 底圖選擇器（2D 模式用）
        const mapBasemapGallery = new BasemapGallery({
          view: mapView,
          source: [
            taiwanWmtsBasemap,
            imageryBasemap,
            streetsBasemap
          ]
        });
        //#endregion

        //#region ◆2D/3D 切換按鈕
        const switchViewBtn = document.createElement("button");
        switchViewBtn.className = "custom-button";
        switchViewBtn.textContent = "🗺️ 切換至 2D";

        switchViewBtn.addEventListener("click", () => {
          if (isSceneView) {
            // ── 切換至 2D ──
            // 1. WMTS 加入 map（僅 2D 相容）
            if (fs2WmtsLayer && !map.layers.includes(fs2WmtsLayer)) {
              map.add(fs2WmtsLayer);
            }
            // 2. 若 WMTS checkbox 勾選，確保圖層可見
            if (fs2WmtsLayer) {
              fs2WmtsLayer.visible = fs2WmtsCheckbox.checked;
            }
            // 3. 啟用 WMTS checkbox
            fs2WmtsCheckbox.disabled = !fs2WmtsLayer;

            // 4. 容器交換
            const center = (view as any).center ?? { longitude: 120.68, latitude: 24.15 };
            view.container = null as any;
            mapView.center = center;
            mapView.container = "viewDiv";

            // 5. 將底圖 gallery 與按鈕移至 mapView
            mapView.ui.add({ component: mapBasemapGallery, position: "bottom-left", expanded: false });
            mapView.ui.add(customButton, "top-left");
            mapView.ui.add(switchViewBtn, "top-left");
            mapView.ui.add(layerMenu, "top-right");

            currentView = mapView;
            isSceneView = false;
            switchViewBtn.textContent = "🌐 切換至 3D";

            createWeatherImageryLayer(); // 重新建立氣象圖層以確保在 2D 中可用
          } else {
            // ── 切換至 3D ──
            // 1. 停用並取消勾選 WMTS checkbox
            fs2WmtsCheckbox.disabled = true;
            fs2WmtsCheckbox.checked = false;
            // 2. 從 map 移除 WMTS（SceneView 不支援）
            if (fs2WmtsLayer) {
              fs2WmtsLayer.visible = false;
              try { map.remove(fs2WmtsLayer); } catch (_) { }
            }

            // 3. 容器交換
            const center = (mapView as any).center ?? { longitude: 120.68, latitude: 24.15 };
            mapView.container = null as any;
            view.container = "viewDiv";

            // 4. 將底圖 gallery 與按鈕移至 view（SceneView）
            view.ui.add({ component: basemapGallery, position: "bottom-left", expanded: false });
            view.ui.add(customButton, "top-left");
            view.ui.add(switchViewBtn, "top-left");
            view.ui.add(layerMenu, "top-right");

            currentView = view;
            isSceneView = true;
            switchViewBtn.textContent = "🗺️ 切換至 2D";
          }
        });
        //#endregion

        //#region ◆自定義按鈕：我的位置
        // [我的位置]按鈕
        const customButton = document.createElement("button");
        customButton.className = "custom-button";
        customButton.textContent = "📍 我的位置";
        customButton.addEventListener("click", () => {
          // 檢查瀏覽器是否支援 Geolocation API
          if (navigator.geolocation) {
            customButton.textContent = "📍 定位中...";
            customButton.disabled = true;

            navigator.geolocation.getCurrentPosition(
              (position) => {
                // 成功獲取位置
                const userLongitude = position.coords.longitude;
                const userLatitude = position.coords.latitude;

                console.log(`使用者位置：經度 ${userLongitude}, 緯度 ${userLatitude}`);

                // 移動相機到使用者位置（依視圖模式使用不同參數）
                const goToTarget = isSceneView
                  ? { position: { longitude: userLongitude, latitude: userLatitude, z: 3000 }, tilt: 0, heading: 0 }
                  : { center: [userLongitude, userLatitude], zoom: 16 };
                currentView.goTo(goToTarget).then(() => {
                  // 移除舊的使用者位置標記（先收集再移除，避免修改集合時出錯）
                  const toRemove: any[] = [];
                  currentView.graphics.forEach((graphic: any) => {
                    if (
                      (graphic.popupTemplate && graphic.popupTemplate.title === "📍 您的位置") ||
                      (graphic.attributes && graphic.attributes.isUserLocationPulse)
                    ) {
                      toRemove.push(graphic);
                    }
                  });
                  toRemove.forEach((g: any) => currentView.graphics.remove(g));

                  // 清除舊的脈衝動畫
                  if ((window as any)._userLocationAnimInterval) {
                    clearInterval((window as any)._userLocationAnimInterval);
                    (window as any)._userLocationAnimInterval = null;
                  }

                  const pointGeometry = {
                    type: "point",
                    longitude: userLongitude,
                    latitude: userLatitude
                  };

                  if (isSceneView) {
                    // ── SceneView（3D）：使用 point-3d 符號 ──
                    const pulseGraphic = new Graphic({
                      geometry: pointGeometry,
                      symbol: {
                        type: "point-3d",
                        symbolLayers: [{
                          type: "icon",
                          size: "18px",
                          resource: { primitive: "circle" },
                          material: { color: [0, 122, 255, 0] },
                          outline: { color: [0, 122, 255, 0.9], size: 2 }
                        }]
                      },
                      attributes: { isUserLocationPulse: true }
                    });

                    const userLocationGraphic = new Graphic({
                      geometry: pointGeometry,
                      symbol: {
                        type: "point-3d",
                        symbolLayers: [{
                          type: "icon",
                          size: "20px",
                          resource: { primitive: "circle" },
                          material: { color: [0, 122, 255] },
                          outline: { color: [255, 255, 255], size: 3 }
                        }]
                      },
                      popupTemplate: {
                        title: "📍 您的位置",
                        content: `經度: ${userLongitude.toFixed(5)}<br/>緯度: ${userLatitude.toFixed(5)}<br/>精確度: ±${position.coords.accuracy.toFixed(0)} 公尺`
                      }
                    });

                    currentView.graphics.add(pulseGraphic);
                    currentView.graphics.add(userLocationGraphic);

                    // 脈衝動畫
                    let pulseStep = 0;
                    (window as any)._userLocationAnimInterval = setInterval(() => {
                      pulseStep = (pulseStep + 1) % 40;
                      const progress = pulseStep / 40;
                      const size = 20 + progress * 40;
                      const opacity = (1 - progress) * 0.9;
                      pulseGraphic.symbol = {
                        type: "point-3d",
                        symbolLayers: [{
                          type: "icon",
                          size: `${size}px`,
                          resource: { primitive: "circle" },
                          material: { color: [0, 122, 255, 0] },
                          outline: { color: [0, 122, 255, opacity], size: 2 }
                        }]
                      } as any;
                    }, 50);
                  } else {
                    // ── MapView（2D）：使用 simple-marker 符號 ──
                    const pulseGraphic = new Graphic({
                      geometry: pointGeometry,
                      symbol: {
                        type: "simple-marker",
                        style: "circle",
                        color: [0, 122, 255, 0],
                        size: "18px",
                        outline: { color: [0, 122, 255, 0.9], width: 2 }
                      },
                      attributes: { isUserLocationPulse: true }
                    });

                    const userLocationGraphic = new Graphic({
                      geometry: pointGeometry,
                      symbol: {
                        type: "simple-marker",
                        style: "circle",
                        color: [0, 122, 255],
                        size: "20px",
                        outline: { color: [255, 255, 255], width: 3 }
                      },
                      popupTemplate: {
                        title: "📍 您的位置",
                        content: `經度: ${userLongitude.toFixed(5)}<br/>緯度: ${userLatitude.toFixed(5)}<br/>精確度: ±${position.coords.accuracy.toFixed(0)} 公尺`
                      }
                    });

                    currentView.graphics.add(pulseGraphic);
                    currentView.graphics.add(userLocationGraphic);

                    // 脈衝動畫（2D）
                    let pulseStep = 0;
                    (window as any)._userLocationAnimInterval = setInterval(() => {
                      pulseStep = (pulseStep + 1) % 40;
                      const progress = pulseStep / 40;
                      const size = 20 + progress * 40;
                      const opacity = (1 - progress) * 0.9;
                      pulseGraphic.symbol = {
                        type: "simple-marker",
                        style: "circle",
                        color: [0, 122, 255, 0],
                        size: `${size}px`,
                        outline: { color: [0, 122, 255, opacity], width: 2 }
                      } as any;
                    }, 50);
                  }

                  customButton.textContent = "📍 我的位置";
                  customButton.disabled = false;
                });
              },
              (error) => {
                // 定位失敗處理
                let errorMessage = "";
                switch (error.code) {
                  case error.PERMISSION_DENIED:
                    errorMessage = "使用者拒絕提供位置資訊";
                    break;
                  case error.POSITION_UNAVAILABLE:
                    errorMessage = "無法取得位置資訊";
                    break;
                  case error.TIMEOUT:
                    errorMessage = "定位請求逾時";
                    break;
                  default:
                    errorMessage = "發生未知錯誤";
                }
                console.error("定位錯誤：", errorMessage);
                alert(`無法取得您的位置：${errorMessage}\n\n請確認已允許瀏覽器存取您的位置。`);

                customButton.textContent = "📍 我的位置";
                customButton.disabled = false;
              },
              {
                enableHighAccuracy: true,  // 使用高精度定位
                timeout: 10000,            // 10 秒逾時
                maximumAge: 0              // 不使用快取位置
              }
            );
          } else {
            alert("您的瀏覽器不支援地理定位功能！");
          }
        });

        // 將按鈕添加到地圖的左上角
        view.ui.add(customButton, "top-left");
        view.ui.add(switchViewBtn, "top-left");
        //#endregion
      });
  }
  catch (error) {
    console.error("載入 ArcGIS API for JavaScript 時發生錯誤：", error);
  }
</script>
