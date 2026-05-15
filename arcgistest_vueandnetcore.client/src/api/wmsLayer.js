const BASE_URL = "/wmslayer";

/**
 * 取得福衛二號 WMS 圖層資訊
 * @returns {Promise<{name: string, url: string, layerName: string, format: string, transparent: boolean, version: string}>}
 */
export async function getFs2WmsLayer() {
  const res = await fetch(`${BASE_URL}/fs2`);
  if (!res.ok) throw new Error(`取得圖層資訊失敗：${res.status}`);
  return res.json();
}


