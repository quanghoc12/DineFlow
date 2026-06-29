import { useState } from "react";
import { CartItem, ChoiceGroup, MenuItem, SelectedChoiceGroup } from "../models/customer";
import { QuantityControl } from "../components/QuantityControl";
import { buildCartKey } from "../utils/cart";
import { formatMoney } from "../utils/money";

export function ItemSheet({
  item,
  initialCartItem,
  onClose,
  onAdd
}: {
  item: MenuItem;
  initialCartItem?: CartItem | null;
  onClose: () => void;
  onAdd: (item: CartItem) => void;
}) {
  const [quantity, setQuantity] = useState(initialCartItem?.quantity ?? 1);
  const [note, setNote] = useState(initialCartItem?.note ?? "");
  const [selectedChoices, setSelectedChoices] = useState<SelectedChoiceGroup[]>(initialCartItem?.selectedChoices ?? []);
  const [validation, setValidation] = useState<string | null>(null);

  function getSelected(groupId: number) {
    return selectedChoices.find((group) => group.choiceGroupId === groupId)?.choiceItemIds ?? [];
  }

  function toggleChoice(group: ChoiceGroup, choiceItemId: number) {
    setValidation(null);
    setSelectedChoices((current) => {
      const existing = current.find((selected) => selected.choiceGroupId === group.choiceGroupId);
      const currentIds = existing?.choiceItemIds ?? [];
      const hasChoice = currentIds.includes(choiceItemId);
      let nextIds: number[];

      if (group.effectiveMaxSelect === 1) {
        nextIds = hasChoice ? [] : [choiceItemId];
      } else {
        nextIds = hasChoice
          ? currentIds.filter((id) => id !== choiceItemId)
          : [...currentIds, choiceItemId].slice(0, group.effectiveMaxSelect);
      }

      const withoutGroup = current.filter((selected) => selected.choiceGroupId !== group.choiceGroupId);
      return nextIds.length === 0
        ? withoutGroup
        : [...withoutGroup, { choiceGroupId: group.choiceGroupId, choiceItemIds: nextIds }];
    });
  }

  function submit() {
    for (const group of item.choiceGroups) {
      const selected = getSelected(group.choiceGroupId);
      if (group.isRequired && selected.length !== 1) {
        setValidation(`Vui lòng chọn ${group.groupName}.`);
        return;
      }

      if (selected.length > group.effectiveMaxSelect) {
        setValidation(`${group.groupName} chỉ được chọn tối đa ${group.effectiveMaxSelect}.`);
        return;
      }
    }

    const normalizedChoices = selectedChoices
      .filter((group) => group.choiceItemIds.length > 0)
      .sort((a, b) => a.choiceGroupId - b.choiceGroupId);
    const normalizedNote = note.trim();

    onAdd({
      key: buildCartKey(item.menuItemId, normalizedChoices, normalizedNote),
      menuItem: item,
      quantity,
      note: normalizedNote,
      selectedChoices: normalizedChoices
    });
  }

  return (
    <div className="sheet-backdrop">
      <div className="bottom-sheet flex-sheet">
        <div className="sheet-header">
          <div>
            <h2>{item.name}</h2>
            <p>{formatMoney(item.finalPrice)}</p>
          </div>
          <button className="icon-button" onClick={onClose}>×</button>
        </div>

        <div className="sheet-scroll-body">
          {item.choiceGroups
            .slice()
            .sort((a, b) => a.displayOrder - b.displayOrder)
            .map((group) => (
              <section className="choice-section" key={group.choiceGroupId}>
                <div className="choice-title">
                  <strong>{group.groupName}</strong>
                  <span>{group.isRequired ? "Bắt buộc" : `Tối đa ${group.effectiveMaxSelect}`}</span>
                </div>
                {group.choiceItems.filter((choice) => choice.isAvailable).map((choice) => {
                  const checked = getSelected(group.choiceGroupId).includes(choice.choiceItemId);
                  return (
                    <button
                      className={`choice-row ${checked ? "selected" : ""}`}
                      key={choice.choiceItemId}
                      onClick={() => toggleChoice(group, choice.choiceItemId)}
                    >
                      <div className="choice-left">
                        {group.isRequired ? (
                          <span className="choice-indicator radio">
                            {checked && <span className="choice-indicator-dot" />}
                          </span>
                        ) : (
                          <span className="choice-indicator checkbox">
                            {checked && (
                              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="3.5" strokeLinecap="round" strokeLinejoin="round" style={{ width: 10, height: 10 }}>
                                <polyline points="20 6 9 17 4 12"></polyline>
                              </svg>
                            )}
                          </span>
                        )}
                        <span className="choice-name">{choice.choiceName}</span>
                      </div>
                      <span className="choice-price">{choice.finalExtraPrice > 0 ? `+${formatMoney(choice.finalExtraPrice)}` : "0đ"}</span>
                    </button>
                  );
                })}
              </section>
            ))}

          <label className="note-field">
            Ghi chú
            <textarea value={note} onChange={(event) => setNote(event.target.value)} placeholder="Ví dụ: ít đá, không hành..." />
          </label>
        </div>

        {validation && <p className="validation-text">{validation}</p>}

        <div className="sheet-actions">
          <QuantityControl value={quantity} min={1} onChange={setQuantity} />
          <button className="primary-button" onClick={submit}>{initialCartItem ? "Lưu thay đổi" : "Thêm vào giỏ"}</button>
        </div>
      </div>
    </div>
  );
}
