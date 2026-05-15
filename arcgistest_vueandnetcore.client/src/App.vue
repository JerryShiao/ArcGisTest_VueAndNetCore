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
  try {
    // 使用 require 載入模組
    // 加上 @ts-ignore 後，TypeScript 會忽略下一行的型別檢查，錯誤「應有 1 個引數，但得到 2 個」就不會再出現了。
    require(["esri/Map",
      "esri/views/SceneView",
      "esri/widgets/BasemapGallery",
      "esri/Basemap",
      "esri/layers/WMTSLayer",
      "esri/identity/IdentityManager",
      "esri/Graphic"],
      // @ts-ignore
      (Map, SceneView, BasemapGallery, Basemap, WMTSLayer, esriId, Graphic) => {

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

                // 移動相機到使用者位置（使用 camera 確保 SceneView 座標精確）
                view.goTo({
                  position: {
                    longitude: userLongitude,
                    latitude: userLatitude,
                    z: 3000   // 相機高度（公尺），約街道俯視層級
                  },
                  tilt: 0,    // 俯視（無傾斜角）
                  heading: 0
                }).then(() => {
                  // 移除舊的使用者位置標記（先收集再移除，避免修改集合時出錯）
                  const toRemove: any[] = [];
                  view.graphics.forEach((graphic: any) => {
                    if (
                      (graphic.popupTemplate && graphic.popupTemplate.title === "📍 您的位置") ||
                      (graphic.attributes && graphic.attributes.isUserLocationPulse)
                    ) {
                      toRemove.push(graphic);
                    }
                  });
                  toRemove.forEach((g: any) => view.graphics.remove(g));

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

                  // 脈衝光環標記（使用 point-3d 符號，相容 SceneView）
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

                  // 主要位置標記
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

                  view.graphics.add(pulseGraphic);
                  view.graphics.add(userLocationGraphic);

                  // 啟動脈衝動畫：光環由小擴大並淡出，循環播放
                  let pulseStep = 0;
                  (window as any)._userLocationAnimInterval = setInterval(() => {
                    pulseStep = (pulseStep + 1) % 40;
                    const progress = pulseStep / 40;
                    const size = 20 + progress * 40; // 20px → 60px
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

      });
  }
  catch (error) {
    console.error("載入 ArcGIS API for JavaScript 時發生錯誤：", error);
    alert("無法載入地圖，請稍後再試。");
  }
</script>
