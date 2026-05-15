<template>
  <!-- 地圖會顯示在這個 div 裡面 -->
  <div id="viewDiv"></div>

  <!-- 底圖切換面板 -->
  <div class="custom-menu">
    <select id="basemapSelect" style="margin-bottom: 12px;"></select>
  </div>

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
</style>

<!--初始化-->
<script setup lang="ts">
  // 使用 require 載入模組
  // 加上 @ts-ignore 後，TypeScript 會忽略下一行的型別檢查，錯誤「應有 1 個引數，但得到 2 個」就不會再出現了。
  require(["esri/Map",
    "esri/views/SceneView",
    "esri/widgets/BasemapGallery",
    "esri/Basemap",
    "esri/layers/WMTSLayer",
    "esri/identity/IdentityManager"],
    // @ts-ignore
    (Map, SceneView, BasemapGallery, Basemap, WMTSLayer, esriId) => {

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
        localStorage.setItem(CREDENTIALS_KEY, JSON.stringify(esriId.toJSON()));
      });
      //#endregion

      //#region -- 初始化地圖和視圖

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
        }
      });
      //#endregion

      //#region ◆使用 BasemapGallery 替代 BasemapToggle，支援多個底圖切換
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

      //#region ◆底圖切換功能
      view.when(() => {
        const basemapSelectEl = document.getElementById("basemapSelect");
        if (!basemapSelectEl) return;
        basemapSelectEl.addEventListener("change", (event) => {
          const selectedBasemap = (event.target as HTMLSelectElement).value;
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
      });
      //#endregion

      //#endregion

    });
</script>
