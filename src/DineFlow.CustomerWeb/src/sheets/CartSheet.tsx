import { CartItem } from "../models/customer";
import { QuantityControl } from "../components/QuantityControl";
import { describeChoices, getCartItemTotal } from "../utils/cart";
import { formatMoney } from "../utils/money";

export function CartSheet({
  cart,
  cartTotal,
  onClose,
  onClear,
  onEdit,
  onQuantityChange,
  onSendOrder
}: {
  cart: CartItem[];
  cartTotal: number;
  onClose: () => void;
  onClear: () => void;
  onEdit: (item: CartItem) => void;
  onQuantityChange: (key: string, quantity: number) => void;
  onSendOrder: () => void;
}) {
  return (
    <div className="sheet-backdrop">
      <div className="bottom-sheet flex-sheet">
        <div className="sheet-header">
          <h2>Đã chọn ({cart.length})</h2>
          <button className="link-button danger" onClick={onClear}>Xóa giỏ hàng</button>
        </div>
        
        <div className="sheet-scroll-body">
          <div className="cart-list">
            {cart.map((item) => (
              <div className="cart-item" key={item.key}>
                <div className="cart-item-top">
                  <div className="cart-item-details">
                    <strong>{item.menuItem.name}</strong>
                    {item.selectedChoices.map((group) => (
                      <p key={group.choiceGroupId} className="muted">
                        {describeChoices(item.menuItem, group)}
                      </p>
                    ))}
                    {item.note && <p className="muted">Ghi chú: {item.note}</p>}
                  </div>
                  <div className="cart-item-qty">
                    <QuantityControl
                      value={item.quantity}
                      min={0}
                      onChange={(value) => onQuantityChange(item.key, value)}
                    />
                  </div>
                </div>
                <div className="cart-item-bottom">
                  <div className="cart-links">
                    <button className="link-button" onClick={() => onEdit(item)}>Chỉnh sửa</button>
                    <button className="link-button danger" onClick={() => onQuantityChange(item.key, 0)}>Xóa</button>
                  </div>
                  <strong className="cart-item-price">{formatMoney(getCartItemTotal(item))}</strong>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="cart-total-row">
          <span>Tiền hàng</span>
          <strong>{formatMoney(cartTotal)}</strong>
        </div>
        <div className="sheet-actions">
          <button className="secondary-button" onClick={onClose}>Quay lại</button>
          <button className="primary-button" disabled={cart.length === 0} onClick={onSendOrder}>Gọi món</button>
        </div>
      </div>
    </div>
  );
}
