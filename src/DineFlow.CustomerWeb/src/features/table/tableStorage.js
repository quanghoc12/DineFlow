export function getClientToken() {
  const key = 'dineflow_client_token';
  let token = localStorage.getItem(key);
  if (!token) {
    token = crypto.randomUUID();
    localStorage.setItem(key, token);
  }
  return token;
}

export function getTableTokenFromUrl() {
  return new URLSearchParams(window.location.search).get('t');
}
