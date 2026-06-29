const tokenPrefix = "dineflow:customerToken:";

export function getQrToken() {
  const match = window.location.pathname.match(/\/table\/([^/]+)/i);
  return decodeURIComponent(match?.[1] ?? "");
}

export function getTokenStorageKey(qrToken: string) {
  return `${tokenPrefix}${qrToken}`;
}
