import { FormEvent, useEffect, useMemo, useState } from "react";
import { StaffLoginResponse, StaffTableOtp, staffOtpApi } from "./api/staffOtpApi";
import { Toast } from "./components/Toast";

const tokenKey = "dineflow.staffOtp.token";
const userKey = "dineflow.staffOtp.user";

export function StaffOtpApp() {
  const [token, setToken] = useState(() => localStorage.getItem(tokenKey) ?? "");
  const [user, setUser] = useState<StaffLoginResponse | null>(() => {
    const raw = localStorage.getItem(userKey);
    return raw ? JSON.parse(raw) as StaffLoginResponse : null;
  });
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [tables, setTables] = useState<StaffTableOtp[]>([]);
  const [areaId, setAreaId] = useState<number | null>(null);
  const [status, setStatus] = useState("");
  const [search, setSearch] = useState("");
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [toast, setToast] = useState<{ message: string; type: "success" | "error" } | null>(null);

  const areas = useMemo(() => {
    const map = new Map<number, string>();
    tables.forEach((table) => {
      if (table.areaId) map.set(table.areaId, table.area);
    });
    return [...map.entries()].sort((a, b) => a[1].localeCompare(b[1]));
  }, [tables]);

  const grouped = useMemo(() => {
    return tables.reduce<Record<string, StaffTableOtp[]>>((result, table) => {
      result[table.area] ??= [];
      result[table.area].push(table);
      return result;
    }, {});
  }, [tables]);

  useEffect(() => {
    if (!token) return;
    loadTables();
  }, [token, areaId, status, search]);

  useEffect(() => {
    if (!toast) return;
    const timer = window.setTimeout(() => setToast(null), 3000);
    return () => window.clearTimeout(timer);
  }, [toast]);

  async function login(event: FormEvent) {
    event.preventDefault();
    try {
      const response = await staffOtpApi.login(username, password);
      localStorage.setItem(tokenKey, response.token);
      localStorage.setItem(userKey, JSON.stringify(response));
      setToken(response.token);
      setUser(response);
      history.replaceState(null, "", "/nhanvien/banotp");
    } catch (err) {
      setToast({ message: (err as Error).message, type: "error" });
    }
  }

  async function loadTables() {
    try {
      const response = await staffOtpApi.list(token, { areaId, status, search });
      setTables(response);
    } catch (err) {
      setToast({ message: (err as Error).message, type: "error" });
    }
  }

  async function resetOne(tableId: number) {
    if (!confirmReset()) return;
    try {
      await staffOtpApi.resetOne(token, tableId);
      await loadTables();
      setToast({ message: "Đã reset OTP.", type: "success" });
    } catch (err) {
      setToast({ message: (err as Error).message, type: "error" });
    }
  }

  async function resetSelected() {
    if (selectedIds.length === 0 || !confirmReset()) return;
    try {
      await staffOtpApi.resetBatch(token, { tableIds: selectedIds });
      setSelectedIds([]);
      await loadTables();
      setToast({ message: "Đã reset OTP các bàn đã chọn.", type: "success" });
    } catch (err) {
      setToast({ message: (err as Error).message, type: "error" });
    }
  }

  async function resetArea() {
    if (!areaId || !confirmReset()) return;
    try {
      await staffOtpApi.resetBatch(token, { areaId });
      await loadTables();
      setToast({ message: "Đã reset OTP khu vực.", type: "success" });
    } catch (err) {
      setToast({ message: (err as Error).message, type: "error" });
    }
  }

  function toggle(tableId: number) {
    setSelectedIds((current) =>
      current.includes(tableId) ? current.filter((id) => id !== tableId) : [...current, tableId]);
  }

  function logout() {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(userKey);
    setToken("");
    setUser(null);
    history.replaceState(null, "", "/nhanvien/dangnhap");
  }

  if (!token || location.pathname.endsWith("/dangnhap")) {
    return (
      <main className="staff-otp-page">
        <form className="staff-login" onSubmit={login}>
          <h1>Đăng nhập nhân viên</h1>
          <input value={username} onChange={(event) => setUsername(event.target.value)} placeholder="Username" />
          <input value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Password" type="password" />
          <button type="submit">Đăng nhập</button>
        </form>
        {toast && <Toast message={toast.message} type={toast.type} onClose={() => setToast(null)} />}
      </main>
    );
  }

  const canReset = user?.role === "Admin";
  return (
    <main className="staff-otp-page">
      <header className="staff-otp-header">
        <div>
          <p className="eyebrow">{user?.role}</p>
          <h1>OTP bàn</h1>
        </div>
        <button className="secondary-button" onClick={logout}>Đăng xuất</button>
      </header>

      <section className="staff-otp-filters">
        <select value={areaId ?? ""} onChange={(event) => setAreaId(event.target.value ? Number(event.target.value) : null)}>
          <option value="">Tất cả khu vực</option>
          {areas.map(([id, name]) => <option key={id} value={id}>{name}</option>)}
        </select>
        <select value={status} onChange={(event) => setStatus(event.target.value)}>
          <option value="">Tất cả trạng thái</option>
          <option value="Available">Trống</option>
          <option value="Occupied">Đang phục vụ</option>
          <option value="WaitingPayment">Chờ thanh toán</option>
        </select>
        <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Tìm bàn, khu vực, OTP" />
        <button disabled={!canReset || selectedIds.length === 0} onClick={resetSelected}>Reset đã chọn</button>
        <button disabled={!canReset || !areaId} onClick={resetArea}>Reset khu vực</button>
      </section>

      {Object.entries(grouped).map(([area, areaTables]) => (
        <section className="staff-otp-area" key={area}>
          <h2>{area}</h2>
          <div className="staff-otp-grid">
            {areaTables.map((table) => (
              <article className="staff-otp-table" key={table.tableId}>
                <label>
                  <input type="checkbox" checked={selectedIds.includes(table.tableId)} onChange={() => toggle(table.tableId)} />
                  {table.tableName}
                </label>
                <strong>{table.currentOtp}</strong>
                <span>{statusLabel(table.status)}</span>
                <small>{new Date(table.otpUpdatedAt).toLocaleString("vi-VN")}</small>
                <div>
                  <button onClick={() => navigator.clipboard.writeText(table.currentOtp)}>Copy</button>
                  <button disabled={!canReset} onClick={() => resetOne(table.tableId)}>Reset</button>
                </div>
              </article>
            ))}
          </div>
        </section>
      ))}

      {toast && <Toast message={toast.message} type={toast.type} onClose={() => setToast(null)} />}
    </main>
  );
}

function confirmReset() {
  return window.confirm(
    "Reset OTP sẽ tạo mã mới cho khách mới vào bàn.\nKhách đã xác thực trong session hiện tại vẫn tiếp tục gọi món được.");
}

function statusLabel(status: string) {
  if (status === "Available") return "Trống";
  if (status === "Occupied") return "Đang phục vụ";
  if (status === "WaitingPayment") return "Chờ thanh toán";
  return status;
}
