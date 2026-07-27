const apiBaseUrl = import.meta.env.VITE_API_BASE_URL
  ?? `${window.location.protocol}//${window.location.hostname}:5080`;

export type StaffLoginResponse = {
  token: string;
  tokenType: string;
  userId: number;
  username: string;
  fullName: string;
  role: string;
};

export type StaffTableOtp = {
  tableId: number;
  tableName: string;
  areaId: number | null;
  area: string;
  status: string;
  currentOtp: string;
  otpUpdatedAt: string;
  currentSessionId: number | null;
  currentSessionStatus: string | null;
};

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...init?.headers
    },
    ...init
  });

  if (!response.ok) {
    let message = response.status === 403 ? "Bạn không có quyền thực hiện thao tác này." : "Không thể kết nối tới server.";
    try {
      const error = await response.json();
      message = error.message ?? error.title ?? message;
    } catch {
      message = await response.text() || message;
    }
    throw new Error(message);
  }

  return await response.json() as T;
}

export const staffOtpApi = {
  login(username: string, password: string) {
    return request<StaffLoginResponse>("/api/staff/auth/login", {
      method: "POST",
      body: JSON.stringify({ username, password })
    });
  },

  list(token: string, filters: { areaId?: number | null; status?: string; search?: string }) {
    const params = new URLSearchParams();
    if (filters.areaId) params.set("areaId", String(filters.areaId));
    if (filters.status) params.set("status", filters.status);
    if (filters.search?.trim()) params.set("search", filters.search.trim());

    return request<StaffTableOtp[]>(`/api/staff/table-otps?${params.toString()}`, {
      headers: { Authorization: `Bearer ${token}` }
    });
  },

  resetOne(token: string, tableId: number) {
    return request<StaffTableOtp>(`/api/staff/table-otps/${tableId}/reset`, {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` }
    });
  },

  resetBatch(token: string, body: { areaId?: number | null; tableIds?: number[] }) {
    return request<StaffTableOtp[]>("/api/staff/table-otps/reset", {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: JSON.stringify({ areaId: body.areaId ?? null, tableIds: body.tableIds ?? [] })
    });
  }
};
