import { useMemo, useState } from "react";
import { MenuCatalog, MenuItem } from "../models/customer";
import { formatMoney } from "../utils/money";

export function MenuView({
  catalog,
  selectedCategoryIds,
  search,
  cartCount,
  cartTotal,
  onBack,
  onCategoryChange,
  onSearchChange,
  onOpenItem,
  onOpenCart,
  onSendOrder
}: {
  catalog: MenuCatalog;
  selectedCategoryIds: number[];
  search: string;
  cartCount: number;
  cartTotal: number;
  onBack: () => void;
  onCategoryChange: (value: number[] | ((prev: number[]) => number[])) => void;
  onSearchChange: (value: string) => void;
  onOpenItem: (item: MenuItem) => void;
  onOpenCart: () => void;
  onSendOrder: () => void;
}) {
  const [isCategorySheetOpen, setIsCategorySheetOpen] = useState(false);

  // Group filtered items by category
  const groupedItems = useMemo(() => {
    const items = catalog.items.filter((item) => {
      if (selectedCategoryIds.length > 0) {
        if (!item.categoryId || !selectedCategoryIds.includes(item.categoryId)) {
          return false;
        }
      }
      return true;
    });

    const groups: { categoryName: string; items: MenuItem[] }[] = [];

    catalog.categories.forEach((cat) => {
      const catItems = items.filter((item) => item.categoryId === cat.categoryId);
      if (catItems.length > 0) {
        groups.push({
          categoryName: cat.categoryName,
          items: catItems
        });
      }
    });

    const uncategorizedItems = items.filter((item) => !item.categoryId);
    if (uncategorizedItems.length > 0) {
      groups.push({
        categoryName: "Khác",
        items: uncategorizedItems
      });
    }

    return groups;
  }, [catalog.categories, catalog.items, selectedCategoryIds]);

  function handleSelectCategory(catId: number | null) {
    if (catId === null) {
      onCategoryChange([]);
    } else {
      onCategoryChange((current) => {
        if (current.includes(catId)) {
          return current.filter((id) => id !== catId);
        } else {
          return [...current, catId];
        }
      });
    }
  }

  return (
    <section className="menu-screen">
      <header className="sub-header">
        <button className="icon-button" onClick={onBack}>‹</button>
        <h1>Thực đơn</h1>
      </header>
      <div className="search-filter-row">
        <button
          type="button"
          className="category-menu-button"
          onClick={() => setIsCategorySheetOpen(true)}
          aria-label="Chọn danh mục"
        >
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
            <line x1="3" y1="12" x2="21" y2="12" />
            <line x1="3" y1="6" x2="21" y2="6" />
            <line x1="3" y1="18" x2="21" y2="18" />
          </svg>
        </button>
        <div className="search-input-wrapper">
          <svg className="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
            <circle cx="11" cy="11" r="8"></circle>
            <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
          </svg>
          <input
            className="search-input"
            value={search}
            onChange={(event) => onSearchChange(event.target.value)}
            placeholder="Tìm tên món"
          />
        </div>
      </div>

      <div className="menu-list">
        {groupedItems.map((group) => (
          <div className="menu-group" key={group.categoryName}>
            <h3 className="menu-group-title">{group.categoryName} ({group.items.length})</h3>
            <div className="menu-group-items">
              {group.items.map((item) => (
                <button className="menu-item-row" key={item.menuItemId} onClick={() => onOpenItem(item)}>
                  <div className="food-thumb">
                    {item.imageUrl ? <img src={item.imageUrl} alt="" /> : <span>{item.name.slice(0, 1)}</span>}
                  </div>
                  <div className="food-info">
                    <strong>{item.name}</strong>
                    <span>{formatMoney(item.finalPrice)}</span>
                  </div>
                  <span className="round-plus">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3.5" strokeLinecap="round" strokeLinejoin="round" style={{ width: 16, height: 16 }}>
                      <line x1="12" y1="5" x2="12" y2="19"></line>
                      <line x1="5" y1="12" x2="19" y2="12"></line>
                    </svg>
                  </span>
                </button>
              ))}
            </div>
          </div>
        ))}
      </div>

      <div className="cart-bar">
        <button className="cart-icon-button" onClick={onOpenCart} aria-label="Mở giỏ hàng">
          <span className="cart-glyph" aria-hidden="true" />
          {cartCount > 0 && <strong>{cartCount}</strong>}
        </button>
        <div className="cart-total-footer">{formatMoney(cartTotal)}</div>
        <button className="order-button" disabled={cartCount === 0} onClick={onSendOrder}>Gọi món</button>
      </div>

      {isCategorySheetOpen && (
        <div className="modal-backdrop">
          <div className="modal-box request-modal">
            <button type="button" className="modal-close-btn" onClick={() => setIsCategorySheetOpen(false)} aria-label="Đóng">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <line x1="18" y1="6" x2="6" y2="18" />
                <line x1="6" y1="6" x2="18" y2="18" />
              </svg>
            </button>

            <div className="modal-header">
              <h2>Chọn danh mục</h2>
              <p className="modal-subtitle">Lọc món ăn theo danh mục phù hợp.</p>
            </div>

            <div className="payment-options category-select-options">
              <button
                type="button"
                className={`modal-payment-option ${selectedCategoryIds.length === 0 ? "selected" : ""}`}
                onClick={() => handleSelectCategory(null)}
              >
                <span className="modal-checkbox-box">
                  {selectedCategoryIds.length === 0 && (
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3.5" strokeLinecap="round" strokeLinejoin="round" style={{ width: 12, height: 12 }}>
                      <polyline points="20 6 9 17 4 12"></polyline>
                    </svg>
                  )}
                </span>
                <span className="modal-payment-label">Tất cả</span>
              </button>
              {catalog.categories.map((category) => {
                const isSelected = selectedCategoryIds.includes(category.categoryId);
                return (
                  <button
                    key={category.categoryId}
                    type="button"
                    className={`modal-payment-option ${isSelected ? "selected" : ""}`}
                    onClick={() => handleSelectCategory(category.categoryId)}
                  >
                    <span className="modal-checkbox-box">
                      {isSelected && (
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3.5" strokeLinecap="round" strokeLinejoin="round" style={{ width: 12, height: 12 }}>
                          <polyline points="20 6 9 17 4 12"></polyline>
                        </svg>
                      )}
                    </span>
                    <span className="modal-payment-label">{category.categoryName}</span>
                  </button>
                );
              })}
            </div>
          </div>
        </div>
      )}
    </section>
  );
}
