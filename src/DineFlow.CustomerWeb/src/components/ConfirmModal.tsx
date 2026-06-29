export function ConfirmModal({
  title,
  message,
  onConfirm,
  onCancel,
  confirmText = "Đồng ý",
  cancelText = "Hủy",
  isDanger = false
}: {
  title: string;
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
  confirmText?: string;
  cancelText?: string;
  isDanger?: boolean;
}) {
  return (
    <div className="modal-backdrop" style={{ zIndex: 3000 }}>
      <div className="modal-box confirm-modal">
        <h3 className="confirm-title">{title}</h3>
        <p className="confirm-message">{message}</p>
        <div className="confirm-actions">
          <button type="button" className="confirm-btn cancel" onClick={onCancel}>
            {cancelText}
          </button>
          <button
            type="button"
            className={`confirm-btn ${isDanger ? "danger" : "confirm"}`}
            onClick={onConfirm}
          >
            {confirmText}
          </button>
        </div>
      </div>
    </div>
  );
}
