import {
  CartItem,
  CreateOrderResponse,
  CustomerMessage,
  CustomerSession,
  MenuCatalog,
  MenuItem
} from "../models/customer";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL
  ?? `${window.location.protocol}//${window.location.hostname}:5080`;

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...init?.headers
    },
    ...init
  });

  if (!response.ok) {
    let message = "Không thể kết nối tới nhà hàng.";
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

export const customerApi = {
  apiBaseUrl,

  scan(qrToken: string, clientToken?: string | null) {
    return request<CustomerSession>("/api/customer/table-sessions/scan", {
      method: "POST",
      body: JSON.stringify({ qrToken, clientToken })
    });
  },

  updateName(clientToken: string, displayName: string) {
    return request<CustomerSession>("/api/customer/table-sessions/customer-name", {
      method: "PUT",
      body: JSON.stringify({ clientToken, displayName })
    });
  },

  getMenu(categoryId?: number | null, search?: string) {
    const params = new URLSearchParams({ salesChannelCode: "CUSTOMER_WEB" });
    if (categoryId) {
      params.set("categoryId", String(categoryId));
    }
    if (search?.trim()) {
      params.set("search", search.trim());
    }

    return request<MenuCatalog>(`/api/customer/menu?${params.toString()}`);
  },

  getMenuItem(menuItemId: number) {
    return request<MenuItem>(`/api/customer/menu/${menuItemId}?salesChannelCode=CUSTOMER_WEB`);
  },

  createOrder(qrToken: string, session: CustomerSession, cartItems: CartItem[]) {
    return request<CreateOrderResponse>("/api/customer/orders", {
      method: "POST",
      body: JSON.stringify({
        tableToken: qrToken,
        clientToken: session.clientToken,
        displayName: session.displayName,
        salesChannelCode: "CUSTOMER_WEB",
        items: cartItems.map((item) => ({
          menuItemId: item.menuItem.menuItemId,
          quantity: item.quantity,
          note: item.note || null,
          selectedChoices: item.selectedChoices
        }))
      })
    });
  },

  callStaff(clientToken: string, reason: string) {
    return request("/api/customer/service-requests/call-staff", {
      method: "POST",
      body: JSON.stringify({
        clientToken,
        reason,
        message: reason
      })
    });
  },

  requestPayment(clientToken: string, paymentMethod: string) {
    const labels: Record<string, string> = {
      Cash: "Tiền mặt",
      BankTransfer: "Chuyển khoản",
      Card: "Thẻ",
      Combined: "Kết hợp phương thức"
    };

    return request("/api/customer/service-requests/payment-request", {
      method: "POST",
      body: JSON.stringify({
        clientToken,
        paymentMethod,
        message: `Khách gọi thanh toán - ${labels[paymentMethod] ?? paymentMethod}`
      })
    });
  },

  getMessages(clientToken: string) {
    return request<CustomerMessage[]>(`/api/customer/messages?clientToken=${encodeURIComponent(clientToken)}`);
  }
};
