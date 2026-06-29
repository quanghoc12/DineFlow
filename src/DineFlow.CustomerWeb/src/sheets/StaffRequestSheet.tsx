import { FormEvent, useMemo, useState } from "react";

const quickReasons = ["Thêm bát", "Thêm đũa", "Thêm rau sống", "Dọn bàn", "Lấy thêm nước"];

export function StaffRequestSheet({
  onClose,
  onSend
}: {
  onClose: () => void;
  onSend: (reason: string) => void;
}) {
  const [note, setNote] = useState("");
  const [selectedReasons, setSelectedReasons] = useState<string[]>([]);

  const reason = useMemo(() => {
    return [...selectedReasons, note.trim()].filter(Boolean).join(", ");
  }, [note, selectedReasons]);

  function toggleReason(value: string) {
    setSelectedReasons((current) => current.includes(value)
      ? current.filter((item) => item !== value)
      : [...current, value]);
  }

  function submit(event: FormEvent) {
    event.preventDefault();
    if (!reason) {
      return;
    }

    onSend(reason);
  }

  return (
    <div className="modal-backdrop">
      <form className="modal-box request-modal" onSubmit={submit}>
        <button type="button" className="modal-close-btn" onClick={onClose} aria-label="Đóng">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>

        <div className="modal-header">
          <h2>Gọi nhân viên</h2>
          <p className="modal-subtitle">Chọn lý do để nhà hàng hỗ trợ nhanh hơn.</p>
        </div>

        <div className="modal-field">
          <label className="modal-label">Lý do gọi nhân viên</label>
          <textarea
            value={note}
            onChange={(event) => setNote(event.target.value)}
            placeholder="Ví dụ: Lấy thêm bát đũa, dọn bàn,..."
          />
        </div>

        <div className="modal-field">
          <label className="modal-label">Chọn nhanh lý do</label>
          <div className="modal-chips">
            {quickReasons.map((item) => (
              <button
                type="button"
                className={`modal-chip ${selectedReasons.includes(item) ? "selected" : ""}`}
                key={item}
                onClick={() => toggleReason(item)}
              >
                {item}
              </button>
            ))}
          </div>
        </div>

        <button className="primary-button modal-submit-btn" type="submit" disabled={!reason}>
          Gửi yêu cầu
        </button>

        <p className="modal-footnote">Yêu cầu của bạn chỉ được gửi đến nhân viên</p>
      </form>
    </div>
  );
}
