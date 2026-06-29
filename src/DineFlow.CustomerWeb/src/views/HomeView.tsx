import { CustomerSession } from "../models/customer";
import btnCallStaffImg from "../assets/btn_call_staff.webp";
import btnMenuImg from "../assets/btn_menu.webp";
import btnPaymentImg from "../assets/btn_payment.webp";
import logoBannerImg from "../assets/logo_banner.webp";
import btnMessageImg from "../assets/btn_message.webp";

export function HomeView({
  session,
  onEditName,
  onOpenMenu,
  onOpenMessages,
  unreadMessageCount,
  onCallStaff,
  onRequestPayment
}: {
  session: CustomerSession;
  onEditName: () => void;
  onOpenMenu: () => void;
  onOpenMessages: () => void;
  unreadMessageCount: number;
  onCallStaff: () => void;
  onRequestPayment: () => void;
}) {
  return (
    <section className="home-screen">
      <header className="home-header">
        <div className="header-brand-row">
          <img src={logoBannerImg} className="home-logo-banner" alt="DineFlow" />
        </div>

        <div className="info-row">
          <svg className="location-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
            <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
            <circle cx="12" cy="10" r="3" />
          </svg>
          Bàn {session.tableName} - {session.area}
        </div>

        <button className="name-button" onClick={onEditName} aria-label="Sửa tên hiển thị">
          <svg className="user-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z" />
          </svg>
          <span className="display-name">{session.displayName}</span>
          <span className="edit-action">
            <svg className="edit-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
              <path d="M18.5 2.5a2.121 2.121 0 1 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
            </svg>
            Sửa
          </span>
        </button>
      </header>

      <div className="support-section">
        <div className="support-title">
          <svg className="title-sparkle" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2L14 10L22 12L14 14L12 22L10 14L2 12L10 10Z" /></svg>
          Bạn đang cần hỗ trợ gì?
          <svg className="title-sparkle" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2L14 10L22 12L14 14L12 22L10 14L2 12L10 10Z" /></svg>
        </div>

        <div className="support-grid">
          <button className="support-card-img-btn green" onClick={onCallStaff} aria-label="Gọi nhân viên">
            <img src={btnCallStaffImg} alt="Gọi nhân viên" />
          </button>
          
          <button className="support-card-img-btn yellow" onClick={onRequestPayment} aria-label="Gọi thanh toán">
            <img src={btnPaymentImg} alt="Gọi thanh toán" />
          </button>
        </div>
      </div>

      <button className="menu-hero-img-btn" onClick={onOpenMenu} aria-label="Thực đơn & gọi món">
        <img src={btnMenuImg} alt="Thực đơn & gọi món" />
      </button>

      {/* Floating Message Button */}
      <button className="floating-message-img-btn" onClick={onOpenMessages} aria-label="Hộp thư tin nhắn">
        <img src={btnMessageImg} alt="Tin nhắn" />
        {unreadMessageCount > 0 && (
          <span className="message-badge">{unreadMessageCount > 99 ? "99+" : unreadMessageCount}</span>
        )}
      </button>
    </section>
  );
}

