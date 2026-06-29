export function Toast({ message, type, onClose }: { message: string; type: "success" | "error"; onClose: () => void }) {
  return (
    <div className={`toast ${type}`} onClick={onClose}>
      <span className="toast-icon">
        {type === "success" ? "✓" : "✕"}
      </span>
      <span className="toast-text">{message}</span>
    </div>
  );
}
