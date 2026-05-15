<template>
  <header>
    <!-- 引入 ArcGIS 的 CSS 與 JS (CDN) -->
    <link rel="stylesheet" href="https://js.arcgis.com/4.29/esri/themes/light/main.css" />

    <!--載入 ArcGIS Maps SDK for JavaScript 的核心函式庫。-->
    <script src="https://js.arcgis.com/4.29/"></script>

  </header>

  <main>
    <!-- 地圖會顯示在這個 div 裡面 -->
    <div id="viewDiv"></div>
  </main>

  <!-- 在 HTML 中定義範本 -->
  <template id="customMenuTemplate">
    <div class="custom-menu">
      <h3>地圖控制</h3>

      <!-- 底圖切換 -->
      <label style="font-size: 13px; color: #666; margin-bottom: 5px; display: block;">切換底圖</label>
      <select id="basemapSelect" style="margin-bottom: 12px;">
        <option value="taiwanWmts">台灣電子地圖 (WMTS)</option>
        <option value="imagery">衛星影像</option>
        <option value="streets">街道圖</option>
      </select>

      <!-- 縮放控制 -->
      <label style="font-size: 13px; color: #666; margin-bottom: 5px; display: block;">縮放等級</label>
      <select id="zoomSelect">
        <option value="10">縮放等級 10</option>
        <option value="12" selected>縮放等級 12</option>
        <option value="14">縮放等級 14</option>
        <option value="16">縮放等級 16</option>
      </select>
      <button id="applyZoom">套用縮放</button>
      <button id="showInfo">顯示資訊</button>
      <hr style="margin: 10px 0; border: none; border-top: 1px solid #ddd;">

      <!-- 圖層顯示控制區 -->
      <label style="font-size: 13px; color: #666; margin-bottom: 8px; display: block; font-weight: bold;">圖層控制</label>

      <label style="display: flex; align-items: center; cursor: pointer; padding: 5px 0;">
        <input type="checkbox" id="toggleWmsLayer" style="margin-right: 8px; cursor: pointer;">
        <span>🗺️ WMS 疊加圖層 (ASRS)</span>
      </label>

      <label style="display: flex; align-items: center; cursor: pointer; padding: 5px 0;">
        <input type="checkbox" id="toggleConsumerLayer" style="margin-right: 8px; cursor: pointer;">
        <span>📍 台中市消費資訊</span>
      </label>
    </div>
  </template>

  <template id="routePanelTemplate">
    <div class="route-panel">
      <h3>🚗 路徑規劃</h3>
      <button id="selectStartBtn" class="btn-primary">選擇起點</button>
      <button id="selectEndBtn" class="btn-primary">選擇終點</button>
      <button id="calculateRouteBtn" class="btn-success">計算路徑</button>
      <button id="clearRouteBtn" class="btn-secondary">清除路徑</button>
      <div id="routeStatus" class="status info" style="display:none;"></div>
      <div id="routeInfo" class="route-info" style="display:none;"></div>
    </div>
  </template>

</template>

<style scoped>
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
  html, body, #viewDiv {
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
</style>

<!--初始化-->
<script setup>
  // 使用 require 載入模組
  // 加上 @ts-ignore 後，TypeScript 會忽略下一行的型別檢查，錯誤「應有 1 個引數，但得到 2 個」就不會再出現了。
  require(["esri/Map",
    "esri/views/SceneView",
    "esri/widgets/BasemapToggle",
    "esri/widgets/BasemapGallery",
    "esri/Graphic",
    "esri/layers/FeatureLayer",
    "esri/Basemap",
    "esri/layers/VectorTileLayer",
    "esri/layers/WMSLayer",
    "esri/layers/WMTSLayer",
    "esri/rest/route",
    "esri/rest/support/RouteParameters",
    "esri/rest/support/FeatureSet",
    "esri/layers/GraphicsLayer",
    "esri/widgets/NavigationToggle",
    "esri/widgets/Compass",
    "esri/widgets/Search"],
    // @ts-ignore
    (Map, SceneView, BasemapToggle, BasemapGallery, Graphic, FeatureLayer, Basemap, VectorTileLayer, WMSLayer, WMTSLayer, route, RouteParameters, FeatureSet, GraphicsLayer, NavigationToggle, Compass, Search) => {

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
        // 使用內嵌 SVG 作為縮圖
        thumbnailUrl: "data:image/svg+xml;charset=utf-8," + encodeURIComponent(`
                            <svg width="150" height="150" xmlns="http://www.w3.org/2000/svg">
                                <rect width="150" height="150" fill="#4a9fff"/>
                                <text x="75" y="50" font-family="Arial" font-size="30" fill="white" text-anchor="middle">🇹🇼</text>
                                <text x="75" y="90" font-family="Arial" font-size="14" fill="white" text-anchor="middle">台灣電子地圖</text>
                                <text x="75" y="110" font-family="Arial" font-size="12" fill="#eee" text-anchor="middle">WMTS</text>
                            </svg>
                        `)
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

      //#region ◆WMS 疊加圖層（改為普通圖層，不作為底圖）

      // 使用航遥测中心的 WMS 服务（福卫二号影像）
      const wmsLayer = new WMSLayer({
        url: "https://wms.asrs.gov.tw/asofb/FS2/wms",
        sublayers: [{
          name: "FS2:fs2015-1_masked_2m_enhance"  // 福卫二号 2015 影像图层
        }],
        version: "1.1.0",
        customParameters: {
          TRANSPARENT: "TRUE",  // 透明背景，適合作為疊加層
          FORMAT: "image/png"   // PNG 格式支持透明度
        },
        // 明確設置空間參考為 Web Mercator (3857)
        spatialReference: {
          wkid: 3857
        },
        title: "WMS 疊加圖層 (ASRS)",
        visible: false,  // 預設隱藏，由使用者控制顯示
        opacity: 0.7     // 設定透明度，方便與底圖疊加
      });

      // 详细的加载状态监听
      wmsLayer.when(() => {
        console.log("✅ WMS 疊加圖層加載成功");
        console.log("📋 WMS 圖層信息：", {
          url: wmsLayer.url,
          版本: wmsLayer.version,
          圖層名稱: wmsLayer.sublayers.items.map(s => s.name),
          空間參考: wmsLayer.spatialReference,
          範圍: wmsLayer.fullExtent,
          透明度: wmsLayer.opacity
        });
      }, (error) => {
        console.error("❌ WMS 圖層加載失敗：", error);

        // 詳細的錯誤分析
        if (error.message && (error.message.includes("CORS") || error.message.includes("Access-Control"))) {
          console.log("🔴 CORS 跨域錯誤檢測到！");
          console.log("⚠️ 問題原因：");
          console.log("   WMS 服務器 (wmstest.asrs.gov.tw) 不允許跨域訪問");
          console.log("");
          console.log("🔧 解決方案（按推薦順序）：");
          console.log("");
          console.log("【方案 1】自建代理服務器（最可靠）");
          console.log("   1. 安裝 Node.js");
          console.log("   2. 創建 proxy.js 文件：");
          console.log("      const express = require('express');");
          console.log("      const { createProxyMiddleware } = require('http-proxy-middleware');");
          console.log("      const app = express();");
          console.log("      app.use('/proxy', createProxyMiddleware({");
          console.log("        target: 'https://wmstest.asrs.gov.tw',");
          console.log("        changeOrigin: true,");
          console.log("        pathRewrite: { '^/proxy': '' }");
          console.log("      }));");
          console.log("      app.listen(8080);");
          console.log("   3. 執行: node proxy.js");
          console.log("   4. 修改代碼中的 proxyUrl = 'http://localhost:8080/proxy'");
          console.log("");
          console.log("【方案 2】使用 CORS Anywhere（開發測試用）");
          console.log("   1. 訪問：https://cors-anywhere.herokuapp.com/corsdemo");
          console.log("   2. 點擊 'Request temporary access'");
          console.log("   3. 修改代碼啟用 CORS Anywhere 代理");
          console.log("   ⚠️ 注意：有請求限制，僅供測試");
          console.log("");
          console.log("【方案 3】使用替代圖層");
          console.log("   使用其他支持 CORS 的台灣圖層服務，例如：");
          console.log("   - 國土測繪中心 WMTS（已在代碼中）");
          console.log("   - 政府開放資料平台的 FeatureServer");
          console.log("");
          console.log("【方案 4】聯繫服務提供商");
          console.log("   向 wmstest.asrs.gov.tw 申請啟用 CORS 支援");

          // 顯示用戶友好的彈窗提示
          setTimeout(() => {
            alert("⚠️ WMS 圖層加載失敗\n\n" +
              "問題：WMS 服務器不允許跨域訪問（CORS 限制）\n\n" +
              "✅ 建議解決方案：\n" +
              "1. 自建代理服務器（詳見控制台）\n" +
              "2. 使用 CORS Anywhere（開發測試）\n" +
              "3. 使用其他支持 CORS 的圖層\n\n" +
              "💡 地圖其他功能仍可正常使用");
          }, 3000);
        } else if (error.message && error.message.includes("network")) {
          console.log("🔴 網絡錯誤檢測到！");
          console.log("⚠️ 請檢查網絡連接和 WMS 服務可用性");
        } else {
          console.log("🔴 未知錯誤");
          console.log("📋 錯誤詳情：", error.message);
        }
      });
      //#endregion

      // 創建地圖實例
      const map = new Map({
        basemap: taiwanWmtsBasemap,
        ground: "world-elevation" // 3D 地形
      });

      // 將 WMS 圖層作為疊加層添加到地圖（不作為底圖）
      map.add(wmsLayer);

      // 創建 GraphicsLayer 用於顯示路徑規劃的圖形
      const routeLayer = new GraphicsLayer({
        title: "路徑規劃圖層"
      });
      map.add(routeLayer);

      // 創建 3D 視圖並關聯到 HTML 元素
      const view = new SceneView({
        container: "viewDiv", // 對應 HTML 標籤的 ID
        map: map,
        camera: {
          position: { x: 120.68, y: 24.15, z: 50000 },
          tilt: 45,  // 傾斜角度 (0-90°，0=俯視，90=平視)
          heading: 0 // 方向角度 (0-360°，0=正北)
        }
      });

      // 使用 BasemapGallery 替代 BasemapToggle，支援多個底圖切換
      const basemapGallery = new BasemapGallery({
        view: view,
        source: [
          taiwanWmtsBasemap, // 台灣電子地圖 (WMTS)
          imageryBasemap,    // 衛星影像
          streetsBasemap     // 街道圖
          // 移除 wmsBasemap，因為 WMS 現在是疊加層
        ]
      });

      // 創建可展開的底圖選擇器
      const bgExpand = view.ui.add({
        component: basemapGallery,
        position: "bottom-left",
        expanded: false
      });

      //#region ◆搜索功能
      const searchWidget = new Search({
        view: view,
        allPlaceholder: "搜尋地址或地點",
        includeDefaultSources: true, // 使用預設的 ArcGIS World Geocoding Service
        locationEnabled: false, // 如果需要使用者當前位置，設為 true
        popupEnabled: true,
        resultGraphicEnabled: true,
        goToOverride: function (view, goToParams) {
          // 自定義搜尋結果的視角（3D）
          goToParams.target.scale = 5000; // 設定縮放層級
          return view.goTo({
            target: goToParams.target.target,
            tilt: 60,  // 傾斜角度
            heading: 0,
            zoom: 16
          });
        }
      });

      // 將搜索框添加到地圖右上角
      view.ui.add(searchWidget, {
        position: "top-right",
        index: 0 // 放在最上面
      });

      // 監聽搜索結果
      searchWidget.on("select-result", (event) => {
        console.log("搜尋結果：", event.result);

        // 顯示搜尋結果資訊
        const result = event.result;
        if (result.feature) {
          const location = result.feature.geometry;
          console.log(`找到位置：${result.name}`);
          console.log(`座標：${location.longitude.toFixed(5)}, ${location.latitude.toFixed(5)}`);

          // 移除之前的搜索結果標記
          view.graphics.forEach(graphic => {
            if (graphic.attributes && graphic.attributes.isSearchResult) {
              view.graphics.remove(graphic);
            }
          });

          // 在搜索結果位置添加標記
          const searchResultGraphic = new Graphic({
            geometry: location,
            symbol: {
              type: "simple-marker",
              color: [255, 140, 0],  // 橙色
              size: "16px",
              outline: {
                color: [255, 255, 255],
                width: 3
              }
            },
            attributes: {
              isSearchResult: true,
              name: result.name
            },
            popupTemplate: {
              title: "🔍 搜尋結果",
              content: `<strong>${result.name}</strong><br/>經度: ${location.longitude.toFixed(5)}<br/>緯度: ${location.latitude.toFixed(5)}`
            }
          });

          view.graphics.add(searchResultGraphic);
        }
      });

      // 監聽搜索錯誤
      searchWidget.on("search-complete", (event) => {
        if (event.numResults === 0) {
          console.log("未找到結果");
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

              // 移動相機到使用者位置
              view.goTo({
                position: {
                  x: userLongitude,
                  y: userLatitude,
                  z: 5000  // 更近的視角
                },
                tilt: 60,
                heading: 0
              }).then(() => {
                // 在使用者位置添加標記
                const userLocationGraphic = new Graphic({
                  geometry: {
                    type: "point",
                    longitude: userLongitude,
                    latitude: userLatitude
                  },
                  symbol: {
                    type: "simple-marker",
                    color: [0, 122, 255],  // 藍色
                    size: "16px",
                    outline: {
                      color: [255, 255, 255],
                      width: 3
                    }
                  },
                  popupTemplate: {
                    title: "📍 您的位置",
                    content: `經度: ${userLongitude.toFixed(5)}<br/>緯度: ${userLatitude.toFixed(5)}<br/>精確度: ±${position.coords.accuracy.toFixed(0)} 公尺`
                  }
                });

                // 移除舊的使用者位置標記
                view.graphics.forEach(graphic => {
                  if (graphic.popupTemplate && graphic.popupTemplate.title === "📍 您的位置") {
                    view.graphics.remove(graphic);
                  }
                });

                view.graphics.add(userLocationGraphic);

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

      //#endregion

      // 使用 template 創建選單
      const menuTemplate = document.getElementById("customMenuTemplate");
      const customMenu = menuTemplate.content.cloneNode(true).firstElementChild;

      // 將選單添加到右上角
      view.ui.add(customMenu, "top-right");

      // 為選單按鈕添加事件
      document.getElementById("applyZoom").addEventListener("click", () => {
        const zoomLevel = document.getElementById("zoomSelect").value;
        // 3D 視圖使用 camera.position.z 來控制高度
        const heightMap = {
          "10": 150000,
          "12": 50000,
          "14": 20000,
          "16": 5000
        };
        view.goTo({
          position: {
            x: view.camera.position.longitude,
            y: view.camera.position.latitude,
            z: heightMap[zoomLevel]
          },
          tilt: view.camera.tilt,
          heading: view.camera.heading
        });
      });

      document.getElementById("showInfo").addEventListener("click", () => {
        alert(`目前中心點：${view.camera.position.longitude.toFixed(2)}, ${view.camera.position.latitude.toFixed(2)}\n高度：${view.camera.position.z.toFixed(0)} 公尺\n傾斜角：${view.camera.tilt.toFixed(0)}°\n方向：${view.camera.heading.toFixed(0)}°`);
      });

      // 底圖切換功能（移除 WMS 選項，因為 WMS 現在是疊加層）
      document.getElementById("basemapSelect").addEventListener("change", (event) => {
        const selectedBasemap = event.target.value;
        console.log(`🔄 切換底圖到: ${selectedBasemap}`);

        switch (selectedBasemap) {
          // 台灣電子地圖 (WMTS)
          case "taiwanWmts":
            map.basemap = taiwanWmtsBasemap;
            console.log("✅ 已切換到台灣電子地圖 (WMTS)");
            break;
          // 衛星影像
          case "imagery":
            map.basemap = imageryBasemap;
            console.log("✅ 已切換到衛星影像");
            break;
          // 街道圖
          case "streets":
            map.basemap = streetsBasemap;
            console.log("✅ 已切換到街道圖");
            break;
        }
      });

      // WMS 疊加圖層顯示控制
      document.getElementById("toggleWmsLayer").addEventListener("change", (event) => {
        wmsLayer.visible = event.target.checked;
        if (event.target.checked) {
          console.log("✅ WMS 疊加圖層已顯示");
        } else {
          console.log("🔲 WMS 疊加圖層已隱藏");
        }
      });

      // 圖層顯示控制
      document.getElementById("toggleConsumerLayer").addEventListener("change", (event) => {
        toggleConsumerLayer.visible = event.target.checked;
      });

      //#region ◆建立 FeatureLayer 顯示台中市消費資訊
      // 定義 FeatureLayer
      const toggleConsumerLayer = new FeatureLayer({
        // 關鍵修正：網址結尾必須是 /0 (代表服務中的第一個圖層)
        url: "https://services4.arcgis.com/Fq9gaTaVCzPfNO0o/ArcGIS/rest/services/%e5%8f%b0%e4%b8%ad%e5%b8%82%e6%b6%88%e8%b2%bb20250624/FeatureServer/0",
        // 註：實務上請替換為台灣政府 Opendata 的 FeatureServer URL

        outFields: ["*"], // 讀取所有欄位
        visible: false, // 預設隱藏圖層
        renderer: {
          type: "simple",
          symbol: {
            type: "simple-marker",
            style: "diamond",  // 使用菱形來模擬旗標
            color: [255, 0, 0],  // 紅色
            size: "14px",
            outline: {
              color: [128, 0, 0], // 深紅色邊框
              width: 2
            }
          }
        },
        popupTemplate: {
          title: "{標題}",
          content: [{
            type: "fields", // 使用列表模式，這會自動顯示所有非空值的欄位
            fieldInfos: [
              { fieldName: "編號", label: "編號" },
              { fieldName: "標題", label: "標題" },
              { fieldName: "簡介說明", label: "簡介說明" },
              { fieldName: "電話", label: "電話" },
              { fieldName: "停車資訊", label: "停車資訊" }
            ]
          }]
        }
      });

      // 將圖層加入地圖
      map.add(toggleConsumerLayer);
      //#endregion

      //#region ◆路徑規劃功能

      // 路徑服務 URL (使用 ArcGIS Online 的公開路徑服務)
      const routeUrl = "https://route-api.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World";

      // 用於儲存起點和終點
      let startPoint = null;
      let endPoint = null;
      let isSelectingStart = false;
      let isSelectingEnd = false;

      // 在 JS 中使用範本
      const template = document.getElementById("routePanelTemplate");
      const routePanel = template.content.cloneNode(true).firstElementChild;
      view.ui.add(routePanel, "top-right");

      // 按鈕元素
      const selectStartBtn = document.getElementById("selectStartBtn");
      const selectEndBtn = document.getElementById("selectEndBtn");
      const calculateRouteBtn = document.getElementById("calculateRouteBtn");
      const clearRouteBtn = document.getElementById("clearRouteBtn");
      const routeStatus = document.getElementById("routeStatus");
      const routeInfo = document.getElementById("routeInfo");

      // 顯示狀態訊息
      function showStatus(message, type = "info") {
        routeStatus.textContent = message;
        routeStatus.className = `status ${type}`;
        routeStatus.style.display = "block";
      }

      // 創建點的圖形
      function createPointGraphic(point, color, label) {
        return new Graphic({
          geometry: point,
          symbol: {
            type: "simple-marker",
            color: color,
            size: "12px",
            outline: {
              color: [255, 255, 255],
              width: 2
            }
          },
          attributes: {
            name: label
          },
          popupTemplate: {
            title: label,
            content: `經度: ${point.longitude.toFixed(5)}<br/>緯度: ${point.latitude.toFixed(5)}`
          }
        });
      }

      // 選擇起點按鈕
      selectStartBtn.addEventListener("click", () => {
        isSelectingStart = true;
        isSelectingEnd = false;
        selectStartBtn.style.backgroundColor = "#28a745";
        selectEndBtn.style.backgroundColor = "#0079c1";
        showStatus("請在地圖上點擊選擇起點", "info");
        view.container.style.cursor = "crosshair";
      });

      // 選擇終點按鈕
      selectEndBtn.addEventListener("click", () => {
        isSelectingStart = false;
        isSelectingEnd = true;
        selectStartBtn.style.backgroundColor = "#0079c1";
        selectEndBtn.style.backgroundColor = "#28a745";
        showStatus("請在地圖上點擊選擇終點", "info");
        view.container.style.cursor = "crosshair";
      });

      // 地圖點擊事件
      view.on("click", (event) => {
        if (isSelectingStart) {
          startPoint = {
            type: "point",
            longitude: event.mapPoint.longitude,
            latitude: event.mapPoint.latitude
          };

          // 移除舊的起點標記
          routeLayer.graphics.forEach(graphic => {
            if (graphic.attributes && graphic.attributes.name === "起點") {
              routeLayer.remove(graphic);
            }
          });

          // 添加新的起點標記
          const startGraphic = createPointGraphic(startPoint, [0, 255, 0], "起點");
          routeLayer.add(startGraphic);

          isSelectingStart = false;
          selectStartBtn.style.backgroundColor = "#0079c1";
          view.container.style.cursor = "default";
          showStatus("起點已設定！", "success");

        } else if (isSelectingEnd) {
          endPoint = {
            type: "point",
            longitude: event.mapPoint.longitude,
            latitude: event.mapPoint.latitude
          };

          // 移除舊的終點標記
          routeLayer.graphics.forEach(graphic => {
            if (graphic.attributes && graphic.attributes.name === "終點") {
              routeLayer.remove(graphic);
            }
          });

          // 添加新的終點標記
          const endGraphic = createPointGraphic(endPoint, [255, 0, 0], "終點");
          routeLayer.add(endGraphic);

          isSelectingEnd = false;
          selectEndBtn.style.backgroundColor = "#0079c1";
          view.container.style.cursor = "default";
          showStatus("終點已設定！", "success");
        }
      });

      // 計算路徑按鈕
      calculateRouteBtn.addEventListener("click", () => {
        if (!startPoint || !endPoint) {
          showStatus("請先選擇起點和終點！", "error");
          return;
        }

        showStatus("正在計算路徑...", "info");
        routeInfo.style.display = "none";

        // 設定路徑參數
        const routeParams = new RouteParameters({
          stops: new FeatureSet({
            features: [
              new Graphic({ geometry: startPoint }),
              new Graphic({ geometry: endPoint })
            ]
          }),
          returnDirections: true,
          directionsLanguage: "zh-TW",
          outSpatialReference: view.spatialReference
        });

        // 計算路徑
        route.solve(routeUrl, routeParams)
          .then((result) => {
            if (result.routeResults.length > 0) {
              const routeResult = result.routeResults[0];

              // 移除舊的路徑線
              routeLayer.graphics.forEach(graphic => {
                if (graphic.geometry.type === "polyline") {
                  routeLayer.remove(graphic);
                }
              });

              // 添加路徑線（3D 中使用 line-3d 符號）
              const routeGraphic = new Graphic({
                geometry: routeResult.route.geometry,
                symbol: {
                  type: "simple-line",
                  color: [0, 120, 255, 0.8],
                  width: 5
                }
              });
              routeLayer.add(routeGraphic);

              // 縮放到路徑範圍（3D 視角）
              view.goTo({
                target: routeGraphic.geometry.extent.expand(1.5),
                tilt: 60,
                heading: 30
              });

              // 顯示路徑資訊
              const distance = (routeResult.route.attributes.Total_Kilometers).toFixed(2);
              const time = (routeResult.route.attributes.Total_TravelTime).toFixed(0);

              routeInfo.innerHTML = `
                        <div><strong>📏 總距離：</strong>${distance} 公里</div>
                        <div><strong>⏱️ 預計時間：</strong>${time} 分鐘</div>
                    `;
              routeInfo.style.display = "block";
              showStatus("路徑計算成功！", "success");

              // 如果有轉向指示，可以在控制台顯示
              if (routeResult.directions) {
                console.log("轉向指示：", routeResult.directions.features.map(f => f.attributes.text));
              }
            }
          })
          .catch((error) => {
            console.error("路徑計算錯誤：", error);
            showStatus("路徑計算失敗！可能超出服務範圍或網路問題。", "error");

            // 提供備用方案：直線連接
            if (startPoint && endPoint) {
              const lineGraphic = new Graphic({
                geometry: {
                  type: "polyline",
                  paths: [[
                    [startPoint.longitude, startPoint.latitude],
                    [endPoint.longitude, endPoint.latitude]
                  ]]
                },
                symbol: {
                  type: "simple-line",
                  color: [255, 0, 0, 0.6],
                  width: 3,
                  style: "dash"
                }
              });
              routeLayer.add(lineGraphic);
              showStatus("顯示直線距離參考（非實際路徑）", "info");
            }
          });
      });

      // 清除路徑按鈕
      clearRouteBtn.addEventListener("click", () => {
        startPoint = null;
        endPoint = null;
        isSelectingStart = false;
        isSelectingEnd = false;
        routeLayer.removeAll();
        selectStartBtn.style.backgroundColor = "#0079c1";
        selectEndBtn.style.backgroundColor = "#0079c1";
        view.container.style.cursor = "default";
        routeStatus.style.display = "none";
        routeInfo.style.display = "none";
      });
      //#endregion

    });
</script>
