/**
 * 取得福衛二號 WMS 圖層資訊
 * @returns {Promise<{name: string, url: string, layerName: string, format: string, transparent: boolean, version: string}>}
 */
export async function getFs2WmsLayer() {
  const res = await fetch(`/asrsgovlayer/fs2`);
  if (!res.ok) throw new Error(`取得圖層資訊失敗：${res.status}`);
  const data = await res.json();
  // 將相對路徑補上瀏覽器 origin，確保 ArcGIS SDK 使用正確的完整 URL
  if (data.url && data.url.startsWith('/')) {
    data.url = `${window.location.origin}${data.url}`;
  }
  return data;
}


