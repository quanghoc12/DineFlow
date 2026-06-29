import { FormEvent, useState } from "react";

const paymentMethods = [
  { value: "Cash", label: "Tiền mặt" },
  { value: "BankTransfer", label: "Chuyển khoản" },
  { value: "Card", label: "Thẻ" },
  { value: "Combined", label: "Kết hợp phương thức" }
];

export function PaymentRequestSheet({
  onClose,
  onSend
}: {
  onClose: () => void;
  onSend: (paymentMethod: string) => void;
}) {
  const [paymentMethod, setPaymentMethod] = useState("Cash");

  function submit(event: FormEvent) {
    event.preventDefault();
    onSend(paymentMethod);
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
          <h2>Gọi thanh toán</h2>
          <p className="modal-subtitle">Chọn phương thức thanh toán bạn muốn sử dụng.</p>
        </div>

        <div className="payment-options">
          {paymentMethods.map((method) => (
            <button
              type="button"
              className={`modal-payment-option ${paymentMethod === method.value ? "selected" : ""}`}
              key={method.value}
              onClick={() => setPaymentMethod(method.value)}
            >
              <span className="modal-radio-circle">
                {paymentMethod === method.value && <span className="modal-radio-dot" />}
              </span>
              <span className="modal-payment-label">{method.label}</span>
            </button>
          ))}
        </div>

        <button className="primary-button modal-submit-btn" type="submit">
          Gửi tin nhắn
        </button>
      </form>
    </div>
  );
}
