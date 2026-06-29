import { ReactNode } from "react";

export function ScreenShell({ children }: { children: ReactNode }) {
  return (
    <main className="app-shell">
      <div className="phone-frame">{children}</div>
    </main>
  );
}
