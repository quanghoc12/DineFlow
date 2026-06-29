import { CustomerMessage } from "../models/customer";

export function MessagesView({
  messages,
  onBack
}: {
  messages: CustomerMessage[];
  onBack: () => void;
}) {
  return (
    <section className="messages-screen">
      <header className="sub-header">
        <button className="icon-button" onClick={onBack}>‹</button>
        <h1>Phản hồi của nhà hàng</h1>
      </header>
      <div className="messages-list">
        {messages.length === 0 && <div className="empty-message">Chưa có tin nhắn nào.</div>}
        {messages.flatMap((message) => buildChatBubbles(message)).map((bubble) => (
          <article className={`message-card ${bubble.fromCustomer ? "customer" : "restaurant"}`} key={bubble.key}>
            <div className="message-title">
              <strong>{bubble.title}</strong>
            </div>
            {bubble.items.map((item, index) => (
              <div className="message-item" key={`${item.name}-${index}`}>
                <strong>{item.name} x{item.quantity}</strong>
                {item.choices.length > 0 && <p>{item.choices.join(", ")}</p>}
                {item.note && <p>Ghi chú: {item.note}</p>}
              </div>
            ))}
            {bubble.message && <p className="muted">{bubble.message}</p>}
          </article>
        ))}
      </div>
    </section>
  );
}

function buildChatBubbles(message: CustomerMessage) {
  const bubbles = [
    {
      key: `${message.messageType}-${message.sourceId}-customer`,
      fromCustomer: true,
      title: message.title,
      message: getCustomerMessage(message),
      items: message.items
    }
  ];

  const reply = getRestaurantReply(message);
  if (reply) {
    bubbles.push({
      key: `${message.messageType}-${message.sourceId}-restaurant-${message.status}`,
      fromCustomer: false,
      title: getReplyTitle(message),
      message: reply,
      items: []
    });
  }

  return bubbles;
}

function getCustomerMessage(message: CustomerMessage) {
  // The API uses Message for the restaurant's cancellation reason once an order is cancelled.
  if (message.messageType === "Order" && message.status === "Cancelled") {
    return null;
  }

  return message.message;
}

function getReplyTitle(message: CustomerMessage) {
  const time = formatMessageTime(message.createdAt);
  return `Phản hồi ${time}`;
}

function getRestaurantReply(message: CustomerMessage) {
  if (message.messageType === "Order") {
    if (message.status === "Accepted") {
      return "Nhà hàng đã xác nhận đơn hàng.";
    }

    if (message.status === "Cancelled") {
      return message.message
        ? `Nhà hàng đã hủy đơn hàng. Lý do: ${message.message}`
        : "Nhà hàng đã hủy đơn hàng.";
    }
  }

  if (message.messageType === "ServiceRequest" && message.status === "Confirmed") {
    return "Nhà hàng đã nhận được yêu cầu.";
  }

  return null;
}

function formatMessageTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return date.toLocaleTimeString("vi-VN", {
    hour: "2-digit",
    minute: "2-digit"
  });
}
