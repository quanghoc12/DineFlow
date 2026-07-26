import { FormEvent, useEffect, useMemo, useRef, useState } from "react";
import { customerApi } from "./api/customerApi";
import { ScreenShell } from "./components/ScreenShell";
import { ConfirmModal } from "./components/ConfirmModal";
import { Toast } from "./components/Toast";
import { CartItem, CustomerMessage, CustomerSession, MenuCatalog, MenuItem, View } from "./models/customer";
import { CartSheet } from "./sheets/CartSheet";
import { ItemSheet } from "./sheets/ItemSheet";
import { PaymentRequestSheet } from "./sheets/PaymentRequestSheet";
import { StaffRequestSheet } from "./sheets/StaffRequestSheet";
import {
  RealtimeEvent,
  createDineFlowConnection,
  joinCustomerRealtime,
  realtimeEvents,
  upsertCustomerMessage
} from "./signalr/dineFlowConnection";
import { getCartItemTotal } from "./utils/cart";
import { getQrToken, getTokenStorageKey } from "./utils/qr";
import { HomeView } from "./views/HomeView";
import { MenuView } from "./views/MenuView";
import { MessagesView } from "./views/MessagesView";
import { WelcomeName } from "./views/WelcomeName";

function App() {
  const qrToken = useMemo(getQrToken, []);
  const tokenKey = useMemo(() => getTokenStorageKey(qrToken), [qrToken]);
  const [session, setSession] = useState<CustomerSession | null>(null);
  const [displayNameDraft, setDisplayNameDraft] = useState("");
  const [isEditingName, setIsEditingName] = useState(false);
  const [view, setView] = useState<View>("home");
  const [catalog, setCatalog] = useState<MenuCatalog>({ categories: [], items: [] });
  const [selectedCategoryIds, setSelectedCategoryIds] = useState<number[]>([]);
  const [search, setSearch] = useState("");
  const [selectedItem, setSelectedItem] = useState<MenuItem | null>(null);
  const [editingCartKey, setEditingCartKey] = useState<string | null>(null);
  const [cart, setCart] = useState<CartItem[]>([]);
  const [isCartOpen, setIsCartOpen] = useState(false);
  const [isStaffRequestOpen, setIsStaffRequestOpen] = useState(false);
  const [isPaymentRequestOpen, setIsPaymentRequestOpen] = useState(false);
  const [messages, setMessages] = useState<CustomerMessage[]>([]);
  const [unreadMessageCount, setUnreadMessageCount] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<{ message: string; type: "success" | "error" } | null>(null);
  const [isConfirmClearOpen, setIsConfirmClearOpen] = useState(false);
  const [isConfirmOrderOpen, setIsConfirmOrderOpen] = useState(false);

  function showSuccess(msg: string) {
    setToast({ message: msg, type: "success" });
  }

  function showError(msg: string) {
    setToast({ message: msg, type: "error" });
  }
  const viewRef = useRef<View>(view);

  useEffect(() => {
    if (!toast) {
      return;
    }

    const timer = window.setTimeout(() => setToast(null), 3000);
    return () => window.clearTimeout(timer);
  }, [toast]);

  useEffect(() => {
    viewRef.current = view;
    if (view === "messages") {
      setUnreadMessageCount(0);
    }
  }, [view]);

  useEffect(() => {
    if (!qrToken) {
      setError("Link QR không hợp lệ.");
      setIsLoading(false);
      return;
    }

    const savedToken = localStorage.getItem(tokenKey);
    customerApi.scan(qrToken, savedToken)
      .then((response) => {
        localStorage.setItem(tokenKey, response.clientToken);
        setSession(response);
        setDisplayNameDraft(response.displayName ?? "");
      })
      .catch((err: Error) => setError(err.message))
      .finally(() => setIsLoading(false));
  }, [qrToken, tokenKey]);

  useEffect(() => {
    if (view !== "menu") {
      return;
    }

    customerApi.getMenu(null, search)
      .then(setCatalog)
      .catch((err: Error) => showError(err.message));
  }, [view, search]);

  useEffect(() => {
    if (view !== "messages" || !session) {
      return;
    }

    let active = true;
    const loadMessages = () => {
      customerApi.getMessages(session.clientToken)
        .then((response) => {
          if (active) {
            setMessages(response);
          }
        })
        .catch((err: Error) => showError(err.message));
    };

    loadMessages();
    const interval = window.setInterval(loadMessages, 30000);
    return () => {
      active = false;
      window.clearInterval(interval);
    };
  }, [view, session]);

  useEffect(() => {
    if (!session) {
      return;
    }

    let cancelled = false;
    const connection = createDineFlowConnection(customerApi.apiBaseUrl);

    connection.on(realtimeEvents.customerMessageCreated, (message: CustomerMessage) => {
      receiveRealtimeMessage(message);
    });

    connection.on(realtimeEvents.customerOrderStatusChanged, (message: CustomerMessage) => {
      receiveRealtimeMessage(message);
    });

    connection.on(realtimeEvents.tableSessionChanged, (event: RealtimeEvent) => {
      if (event.tableSessionId !== session.tableSessionId) {
        return;
      }

      customerApi.scan(qrToken, session.clientToken)
        .then((updated) => {
          localStorage.setItem(tokenKey, updated.clientToken);
          setSession(updated);
          setDisplayNameDraft(updated.displayName ?? "");
        })
        .catch((err: Error) => showError(err.message));
    });

    const startConnection = async () => {
      try {
        await joinCustomerRealtime(connection, session);

        if (cancelled) {
          await connection.stop();
        }
      } catch (err) {
        if (cancelled || isStartCancelledError(err)) {
          return;
        }

        showError(`Realtime chưa kết nối: ${(err as Error).message}`);
      }
    };

    void startConnection();

    return () => {
      cancelled = true;
      if (connection.state === "Connected" || connection.state === "Connecting" || connection.state === "Reconnecting") {
        connection.stop().catch(() => undefined);
      }
    };

    function receiveRealtimeMessage(message: CustomerMessage) {
      setMessages((current) => {
        const previous = current.find((item) =>
          item.messageType === message.messageType && item.sourceId === message.sourceId);

        if (
          viewRef.current !== "messages" &&
          isRestaurantResponse(message) &&
          previous?.status !== message.status
        ) {
          setUnreadMessageCount((count) => count + 1);
          showSuccess(getRealtimeReplyToast(message));
        }

        return upsertCustomerMessage(current, message);
      });
    }
  }, [qrToken, session, tokenKey]);

  const cartCount = cart.reduce((sum, item) => sum + item.quantity, 0);
  const cartTotal = cart.reduce((sum, item) => sum + getCartItemTotal(item), 0);

  async function saveName(event: FormEvent) {
    event.preventDefault();
    if (!session) {
      return;
    }

    const name = displayNameDraft.trim();
    if (!name) {
      showError("Vui lòng nhập tên của bạn.");
      return;
    }

    try {
      const updated = await customerApi.updateName(session.clientToken, name);
      setSession(updated);
      setIsEditingName(false);
    } catch (err) {
      showError((err as Error).message);
    }
  }

  async function openMenu() {
    setView("menu");
    setSearch("");
    setSelectedCategoryIds([]);
    try {
      const response = await customerApi.getMenu(null, "");
      setCatalog(response);
    } catch (err) {
      showError((err as Error).message);
    }
  }

  function openMessages() {
    setUnreadMessageCount(0);
    setView("messages");
  }

  async function openItem(item: MenuItem) {
    try {
      const detail = await customerApi.getMenuItem(item.menuItemId);
      setSelectedItem(detail);
    } catch (err) {
      showError((err as Error).message);
    }
  }

  function saveCartItem(item: CartItem) {
    if (editingCartKey) {
      setCart((current) => mergeEditedCartItem(current, editingCartKey, item));
      setEditingCartKey(null);
      setSelectedItem(null);
      setIsCartOpen(true);
      showSuccess("Đã cập nhật giỏ hàng.");
      return;
    }

    setCart((current) => mergeCartItem(current, item));
    setSelectedItem(null);
    showSuccess("Đã thêm vào giỏ hàng.");
  }

  function editCartItem(item: CartItem) {
    setEditingCartKey(item.key);
    setSelectedItem(item.menuItem);
    setIsCartOpen(false);
  }

  function updateCartQuantity(key: string, nextQuantity: number) {
    setCart((current) => {
      if (nextQuantity <= 0) {
        return current.filter((item) => item.key !== key);
      }

      return current.map((item) => item.key === key ? { ...item, quantity: nextQuantity } : item);
    });
  }

  function handleSendOrderClick() {
    setIsConfirmOrderOpen(true);
  }

  async function confirmSendOrder() {
    setIsConfirmOrderOpen(false);
    if (!session || cart.length === 0) {
      return;
    }

    try {
      const response = await customerApi.createOrder(qrToken, session, cart);
      if (response.rejectedItems.length > 0) {
        showError(response.rejectedItems.map((item) => item.reasonMessage).join("\n"));
      }

      if (response.orderId) {
        setCart([]);
        setIsCartOpen(false);
        setView("messages");
        showSuccess("Đã gửi order, vui lòng chờ nhân viên xác nhận.");
      }
    } catch (err) {
      showError((err as Error).message);
    }
  }

  async function sendStaffRequest(reason: string) {
    if (!session) {
      return;
    }

    try {
      await customerApi.callStaff(session.clientToken, reason);
      setIsStaffRequestOpen(false);
      setView("messages");
      showSuccess("Đã gửi yêu cầu gọi nhân viên.");
    } catch (err) {
      showError((err as Error).message);
    }
  }

  async function sendPaymentRequest(paymentMethod: string) {
    if (!session) {
      return;
    }

    try {
      await customerApi.requestPayment(session.clientToken, paymentMethod);
      setIsPaymentRequestOpen(false);
      setView("messages");
      showSuccess("Đã gửi yêu cầu thanh toán.");
    } catch (err) {
      showError((err as Error).message);
    }
  }

  function closeItemSheet() {
    if (editingCartKey) {
      setIsCartOpen(true);
    }
    setEditingCartKey(null);
    setSelectedItem(null);
  }

  function clearCart() {
    setIsConfirmClearOpen(true);
  }

  function confirmClearCart() {
    setCart([]);
    setIsConfirmClearOpen(false);
  }

  if (isLoading) {
    return <ScreenShell><div className="center-state">Đang mở bàn...</div></ScreenShell>;
  }

  if (error || !session) {
    return <ScreenShell><div className="center-state error-text">{error ?? "Không tìm thấy session."}</div></ScreenShell>;
  }

  const needsName = session.requiresName || isEditingName;
  const editingCartItem = editingCartKey ? cart.find((item) => item.key === editingCartKey) ?? null : null;

  return (
    <ScreenShell>
      {needsName ? (
        <WelcomeName
          value={displayNameDraft}
          onChange={setDisplayNameDraft}
          onSubmit={saveName}
          tableName={session.tableName}
          area={session.area}
        />
      ) : (
        <>
          {view === "home" && (
            <HomeView
              session={session}
              onEditName={() => setIsEditingName(true)}
              onOpenMenu={openMenu}
              onOpenMessages={openMessages}
              unreadMessageCount={unreadMessageCount}
              onCallStaff={() => setIsStaffRequestOpen(true)}
              onRequestPayment={() => setIsPaymentRequestOpen(true)}
            />
          )}

          {view === "menu" && (
            <MenuView
              catalog={catalog}
              selectedCategoryIds={selectedCategoryIds}
              search={search}
              cartCount={cartCount}
              cartTotal={cartTotal}
              onBack={() => setView("home")}
              onCategoryChange={setSelectedCategoryIds}
              onSearchChange={setSearch}
              onOpenItem={openItem}
              onOpenCart={() => setIsCartOpen(true)}
              onSendOrder={handleSendOrderClick}
            />
          )}

          {view === "messages" && (
            <MessagesView
              messages={messages}
              onBack={() => setView("home")}
            />
          )}
        </>
      )}

      {selectedItem && (
        <ItemSheet
          item={selectedItem}
          initialCartItem={editingCartItem}
          onClose={closeItemSheet}
          onAdd={saveCartItem}
        />
      )}

      {isCartOpen && (
        <CartSheet
          cart={cart}
          cartTotal={cartTotal}
          onClose={() => setIsCartOpen(false)}
          onClear={clearCart}
          onEdit={editCartItem}
          onQuantityChange={updateCartQuantity}
          onSendOrder={handleSendOrderClick}
        />
      )}

      {isStaffRequestOpen && (
        <StaffRequestSheet
          onClose={() => setIsStaffRequestOpen(false)}
          onSend={sendStaffRequest}
        />
      )}

      {isPaymentRequestOpen && (
        <PaymentRequestSheet
          onClose={() => setIsPaymentRequestOpen(false)}
          onSend={sendPaymentRequest}
        />
      )}

      {isConfirmClearOpen && (
        <ConfirmModal
          title="Xóa giỏ hàng"
          message="Bạn chắc chắn muốn xóa toàn bộ giỏ hàng?"
          confirmText="Xóa sạch"
          cancelText="Bỏ qua"
          isDanger
          onConfirm={confirmClearCart}
          onCancel={() => setIsConfirmClearOpen(false)}
        />
      )}

      {isConfirmOrderOpen && (
        <ConfirmModal
          title="Gửi order"
          message="Bạn chắc chắn muốn gửi order này tới nhà hàng?"
          confirmText="Gửi ngay"
          cancelText="Hủy"
          onConfirm={confirmSendOrder}
          onCancel={() => setIsConfirmOrderOpen(false)}
        />
      )}

      {toast && <Toast message={toast.message} type={toast.type} onClose={() => setToast(null)} />}
    </ScreenShell>
  );
}

function mergeCartItem(current: CartItem[], nextItem: CartItem) {
  const existing = current.find((cartItem) => cartItem.key === nextItem.key);
  if (!existing) {
    return [...current, nextItem];
  }

  return current.map((cartItem) => cartItem.key === nextItem.key
    ? { ...cartItem, quantity: cartItem.quantity + nextItem.quantity }
    : cartItem);
}

function mergeEditedCartItem(current: CartItem[], editingKey: string, nextItem: CartItem) {
  const withoutEdited = current.filter((cartItem) => cartItem.key !== editingKey);
  return mergeCartItem(withoutEdited, nextItem);
}

function isStartCancelledError(err: unknown) {
  return err instanceof Error && err.message.includes("before stop() was called");
}

function isRestaurantResponse(message: CustomerMessage) {
  return (
    message.messageType === "Order" &&
    (message.status === "Accepted" || message.status === "Cancelled")
  ) || (
    message.messageType === "ServiceRequest" &&
    message.status === "Confirmed"
  );
}

function getRealtimeReplyToast(message: CustomerMessage) {
  if (message.messageType === "Order") {
    return message.status === "Cancelled"
      ? "Nhà hàng đã phản hồi đơn gọi món."
      : "Nhà hàng đã xác nhận đơn gọi món.";
  }

  if (message.messageType === "ServiceRequest") {
    return "Nhà hàng đã phản hồi yêu cầu của bạn.";
  }

  return "Bạn có phản hồi mới từ nhà hàng.";
}

export default App;
