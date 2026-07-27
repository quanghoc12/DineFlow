import { FormEvent, useEffect, useMemo, useState } from "react";
import { StaffLoginResponse, StaffTableOtp, staffOtpApi } from "./api/staffOtpApi";
import { ScreenShell } from "./components/ScreenShell";
import { Toast } from "./components/Toast";

const tokenKey = "dineflow.staffOtp.token";
const userKey = "dineflow.staffOtp.user";

type AreaGroup = {
  areaId: number | null;
  area: string;
  tables: StaffTableOtp[];
};

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
  const [isResetOpen, setIsResetOpen] = useState(false);
  const [openAreaKeys, setOpenAreaKeys] = useState<string[]>([]);
  const [toast, setToast] = useState<{ message: string; type: "success" | "error" } | null>(null);

  const areaGroups = useMemo(() => groupTables(tables), [tables]);
  const visibleTables = useMemo(() => filterTables(tables, areaId, status, search), [tables, areaId, status, search]);
  const visibleGroups = useMemo(() => groupTables(visibleTables), [visibleTables]);
  const canReset = user?.role === "Admin";

  useEffect(() => {
    if (!token) return;
    loadTables();
  }, [token]);

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
      const response = await staffOtpApi.list(token, {});
      setTables(response);
    } catch (err) {
      setToast({ message: (err as Error).message, type: "error" });
    }
  }

  async function resetSelected() {
    if (selectedIds.length === 0 || !confirmReset()) return;

    try {
      await staffOtpApi.resetBatch(token, { tableIds: selectedIds });
      setSelectedIds([]);
      setIsResetOpen(false);
      await loadTables();
      setToast({ message: "Đã reset OTP các bàn đã chọn.", type: "success" });
    } catch (err) {
      setToast({ message: (err as Error).message, type: "error" });
    }
  }

  function toggleTable(tableId: number) {
    setSelectedIds((current) =>
      current.includes(tableId) ? current.filter((id) => id !== tableId) : [...current, tableId]);
  }

  function toggleArea(group: AreaGroup) {
    const groupIds = group.tables.map((table) => table.tableId);
    const selectedSet = new Set(selectedIds);
    const allSelected = groupIds.every((id) => selectedSet.has(id));

    if (allSelected) {
      setSelectedIds((current) => current.filter((id) => !groupIds.includes(id)));
      return;
    }

    groupIds.forEach((id) => selectedSet.add(id));
    setSelectedIds([...selectedSet]);
  }

  function toggleAreaOpen(group: AreaGroup) {
    const key = areaKey(group);
    setOpenAreaKeys((current) =>
      current.includes(key) ? current.filter((item) => item !== key) : [...current, key]);
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
      <ScreenShell>
        <section className="staff-otp-page staff-login-page">
          <form className="staff-login" onSubmit={login}>
            <p className="eyebrow">DineFlow Staff</p>
            <h1>Đăng nhập</h1>
            <input value={username} onChange={(event) => setUsername(event.target.value)} placeholder="Username" />
            <input value={password} onChange={(event) => setPassword(event.target.value)} placeholder="Password" type="password" />
            <button type="submit">Đăng nhập</button>
          </form>
          {toast && <Toast message={toast.message} type={toast.type} onClose={() => setToast(null)} />}
        </section>
      </ScreenShell>
    );
  }

  return (
    <ScreenShell>
      <section className="staff-otp-page">
        <header className="staff-otp-header">
          <div>
            <p className="eyebrow">{user?.role}</p>
            <h1>OTP bàn</h1>
          </div>
          <button className="staff-text-button" onClick={logout}>Thoát</button>
        </header>

        <section className="staff-otp-filters">
          <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Tìm bàn, khu vực, OTP" />
          <div className="staff-filter-row">
            <select value={areaId ?? ""} onChange={(event) => setAreaId(event.target.value ? Number(event.target.value) : null)}>
              <option value="">Tất cả khu vực</option>
              {areaGroups
                .filter((group) => group.areaId !== null)
                .map((group) => <option key={group.areaId} value={group.areaId ?? ""}>{group.area}</option>)}
            </select>
            <select value={status} onChange={(event) => setStatus(event.target.value)}>
              <option value="">Tất cả</option>
              <option value="Available">Trống</option>
              <option value="Occupied">Đang phục vụ</option>
              <option value="WaitingPayment">Chờ TT</option>
            </select>
          </div>
          <button className="staff-reset-entry" disabled={!canReset} onClick={() => setIsResetOpen(true)}>
            Reset OTP
          </button>
        </section>

        <div className="staff-otp-summary">{visibleTables.length} bàn</div>

        {visibleGroups.map((group) => (
          <section className="staff-otp-area" key={`${group.areaId ?? "legacy"}-${group.area}`}>
            <header className="staff-area-header">
              <div>
                <h2>{group.area}</h2>
                <p>Chọn bàn để lấy mã OTP và bắt đầu sử dụng</p>
              </div>
            </header>
            <div className="staff-otp-grid">
              {group.tables.map((table) => (
                <article className={`staff-otp-table ${statusClass(table.status)}`} key={table.tableId}>
                  <div className="staff-table-top">
                    <div className="staff-table-icon" aria-hidden="true">
                      <span></span>
                    </div>
                    <div className="staff-table-title">
                      <h3>{table.tableName}</h3>
                      <span>{statusDotLabel(table.status)}</span>
                    </div>
                    <span className="staff-status-badge">{statusBadgeLabel(table.status)}</span>
                  </div>
                  <strong>{table.currentOtp}</strong>
                  <small>{formatOtpTime(table.otpUpdatedAt)}</small>
                </article>
              ))}
            </div>
          </section>
        ))}

        {isResetOpen && (
          <div className="staff-reset-backdrop" role="dialog" aria-modal="true">
            <section className="staff-reset-sheet">
              <header>
                <div>
                  <p className="eyebrow">Admin</p>
                  <h2>Chọn bàn reset</h2>
                </div>
                <button className="staff-text-button" onClick={() => setIsResetOpen(false)}>Đóng</button>
              </header>

              <div className="staff-reset-list">
                {areaGroups.map((group) => (
                  <section className="staff-reset-area" key={areaKey(group)}>
                    <button
                      type="button"
                      className="staff-reset-area-head"
                      aria-expanded={openAreaKeys.includes(areaKey(group))}
                      onClick={() => toggleAreaOpen(group)}
                    >
                      <label onClick={(event) => event.stopPropagation()}>
                        <input
                          type="checkbox"
                          checked={isAreaSelected(group, selectedIds)}
                          onChange={() => toggleArea(group)}
                        />
                        {group.area}
                      </label>
                      <span>{group.tables.length}</span>
                    </button>
                    {openAreaKeys.includes(areaKey(group)) && (
                      <div className="staff-reset-tables">
                        {group.tables.map((table) => (
                          <label className="staff-reset-table" key={table.tableId}>
                            <input
                              type="checkbox"
                              checked={selectedIds.includes(table.tableId)}
                              onChange={() => toggleTable(table.tableId)}
                            />
                            <span>{table.tableName}</span>
                            <strong>{table.currentOtp}</strong>
                          </label>
                        ))}
                      </div>
                    )}
                  </section>
                ))}
              </div>

              <button className="staff-reset-submit" disabled={selectedIds.length === 0} onClick={resetSelected}>
                Reset {selectedIds.length} bàn
              </button>
            </section>
          </div>
        )}

        {toast && <Toast message={toast.message} type={toast.type} onClose={() => setToast(null)} />}
      </section>
    </ScreenShell>
  );
}

function groupTables(tables: StaffTableOtp[]): AreaGroup[] {
  const map = new Map<string, AreaGroup>();
  tables.forEach((table) => {
    const key = `${table.areaId ?? "legacy"}:${table.area}`;
    if (!map.has(key)) {
      map.set(key, { areaId: table.areaId, area: table.area, tables: [] });
    }
    map.get(key)!.tables.push(table);
  });

  return [...map.values()]
    .map((group) => ({
      ...group,
      tables: group.tables.sort((a, b) => a.tableName.localeCompare(b.tableName, "vi"))
    }))
    .sort((a, b) => a.area.localeCompare(b.area, "vi"));
}

function filterTables(tables: StaffTableOtp[], areaId: number | null, status: string, search: string) {
  const keyword = search.trim().toLowerCase();
  return tables.filter((table) => {
    const matchesArea = !areaId || table.areaId === areaId;
    const matchesStatus = !status || table.status === status;
    const matchesSearch = !keyword ||
      table.tableName.toLowerCase().includes(keyword) ||
      table.area.toLowerCase().includes(keyword) ||
      table.currentOtp.toLowerCase().includes(keyword);
    return matchesArea && matchesStatus && matchesSearch;
  });
}

function isAreaSelected(group: AreaGroup, selectedIds: number[]) {
  return group.tables.length > 0 && group.tables.every((table) => selectedIds.includes(table.tableId));
}

function areaKey(group: AreaGroup) {
  return `${group.areaId ?? "legacy"}-${group.area}`;
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

function statusDotLabel(status: string) {
  if (status === "Available") return "Trống";
  if (status === "Occupied") return "Đang phục vụ";
  if (status === "WaitingPayment") return "Chờ thanh toán";
  return status;
}

function statusBadgeLabel(status: string) {
  if (status === "Available") return "Sẵn sàng";
  if (status === "Occupied") return "Phục vụ";
  if (status === "WaitingPayment") return "Chờ TT";
  return status;
}

function statusClass(status: string) {
  if (status === "Available") return "is-available";
  if (status === "Occupied") return "is-occupied";
  if (status === "WaitingPayment") return "is-waiting-payment";
  return "";
}

function formatOtpTime(value: string) {
  const date = new Date(value);
  return date.toLocaleString("vi-VN", {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
    day: "2-digit",
    month: "2-digit",
    year: "numeric"
  });
}
