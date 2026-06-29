import { FormEvent } from "react";
import logoBannerImg from "../assets/logo_banner.webp";

export function WelcomeName({
  value,
  onChange,
  onSubmit,
  tableName,
  area
}: {
  value: string;
  onChange: (value: string) => void;
  onSubmit: (event: FormEvent) => void;
  tableName: string;
  area: string;
}) {
  return (
    <section className="welcome-screen">
      <div className="welcome-header">
        <div className="welcome-logo-container">
          <img src={logoBannerImg} className="welcome-logo-banner" alt="DineFlow" />
        </div>
        <h1 className="welcome-title">Chào mừng bạn</h1>
        <div className="welcome-info-row">
          <svg className="location-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
            <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" />
            <circle cx="12" cy="10" r="3" />
          </svg>
          Bạn đang ngồi tại {tableName} - {area}
        </div>
      </div>
      <form className="name-form" onSubmit={onSubmit}>
        <label className="name-label">Tên của bạn</label>
        <div className="welcome-input-wrapper">
          <div className="welcome-input-icon">
            <svg viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z" />
            </svg>
          </div>
          <input
            value={value}
            onChange={(event) => onChange(event.target.value)}
            placeholder="Nhập tên của bạn..."
            autoFocus
          />
        </div>
        <button className="primary-button welcome-submit-btn" type="submit">
          <div className="welcome-btn-icon-wrapper">
            <svg viewBox="0 0 24 24" fill="currentColor">
              <path d="M12 3a1 1 0 0 1 1 1v.07C16.93 4.52 20 7.9 20 12v1H4v-1c0-4.1 3.07-7.48 7-7.93V4a1 1 0 0 1 1-1zm9 11v1a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1v-1h18z" />
              {/* Star/rays sparkles */}
              <path d="M4 8l-.6-.6M20 8l.6-.6M12 1.5v1" />
            </svg>
          </div>
          <span>Vào gọi món</span>
        </button>
      </form>
    </section>
  );
}
