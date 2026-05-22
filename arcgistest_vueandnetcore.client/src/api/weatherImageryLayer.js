/**
 * 取得天氣圖層資訊
 */
export async function getWeatherImageryLayer() {
  const base = import.meta.env.BASE_URL.replace(/\/$/, "");
  return `${window.location.origin}${base}/weather-imagery`; // 回傳 ImageServer 服務根路徑
  // const res = await fetch(`${window.location.origin}${base}/weather-imagery`);
  // if (!res.ok) throw new Error(`取得圖層資訊失敗：${res.status}`);
  // const data = await res.json();
  // // 將相對路徑補上瀏覽器 origin 與 base path，確保 ArcGIS SDK 使用正確的完整 URL
  // if (data.url && data.url.startsWith('/')) {
  //   data.url = `${window.location.origin}${base}${data.url}`;
  // }
  // return data;
}
