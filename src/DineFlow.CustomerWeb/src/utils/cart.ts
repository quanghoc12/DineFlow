import { CartItem, MenuItem, SelectedChoiceGroup } from "../models/customer";

export function buildCartKey(menuItemId: number, selectedChoices: SelectedChoiceGroup[], note: string) {
  const choiceKey = selectedChoices
    .map((group) => `${group.choiceGroupId}:${group.choiceItemIds.slice().sort((a, b) => a - b).join(",")}`)
    .sort()
    .join("|");

  return `${menuItemId}|${choiceKey}|${note.trim()}`;
}

export function getCartItemTotal(item: CartItem) {
  const choiceTotal = item.selectedChoices.reduce((sum, selectedGroup) => {
    const group = item.menuItem.choiceGroups.find((choiceGroup) => choiceGroup.choiceGroupId === selectedGroup.choiceGroupId);
    if (!group) {
      return sum;
    }

    return sum + selectedGroup.choiceItemIds.reduce((choiceSum, choiceItemId) => {
      const choice = group.choiceItems.find((choiceItem) => choiceItem.choiceItemId === choiceItemId);
      return choiceSum + (choice?.finalExtraPrice ?? 0);
    }, 0);
  }, 0);

  return (item.menuItem.finalPrice + choiceTotal) * item.quantity;
}

export function describeChoices(item: MenuItem, selectedGroup: SelectedChoiceGroup) {
  const group = item.choiceGroups.find((choiceGroup) => choiceGroup.choiceGroupId === selectedGroup.choiceGroupId);
  if (!group) {
    return "";
  }

  const names = selectedGroup.choiceItemIds
    .map((choiceItemId) => group.choiceItems.find((choiceItem) => choiceItem.choiceItemId === choiceItemId)?.choiceName)
    .filter(Boolean)
    .join(", ");

  return `${group.groupName}: ${names}`;
}
