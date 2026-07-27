export type View = "home" | "menu" | "messages";

export type CustomerSession = {
  tableId: number;
  tableName: string;
  area: string;
  tableSessionId: number;
  sessionCustomerId: number;
  clientToken: string;
  displayName: string | null;
  requiresName: boolean;
  isVerified: boolean;
  requiresOtp: boolean;
  canOrder: boolean;
  sessionStatus: string;
};

export type MenuCatalog = {
  categories: MenuCategory[];
  items: MenuItem[];
};

export type MenuCategory = {
  categoryId: number;
  categoryName: string;
  displayOrder: number;
};

export type MenuItem = {
  menuItemId: number;
  categoryId: number;
  name: string;
  description?: string | null;
  basePrice: number;
  channelExtraPrice: number;
  finalPrice: number;
  imageUrl?: string | null;
  isAvailable: boolean;
  stock?: number | null;
  choiceGroups: ChoiceGroup[];
};

export type ChoiceGroup = {
  choiceGroupId: number;
  groupName: string;
  isRequired: boolean;
  effectiveMaxSelect: number;
  displayOrder: number;
  choiceItems: ChoiceItem[];
};

export type ChoiceItem = {
  choiceItemId: number;
  choiceName: string;
  finalExtraPrice: number;
  isAvailable: boolean;
};

export type CartItem = {
  key: string;
  menuItem: MenuItem;
  quantity: number;
  note: string;
  selectedChoices: SelectedChoiceGroup[];
};

export type SelectedChoiceGroup = {
  choiceGroupId: number;
  choiceItemIds: number[];
};

export type CreateOrderResponse = {
  orderId?: number | null;
  orderCode?: string | null;
  tableSessionId?: number | null;
  acceptedItems: unknown[];
  rejectedItems: { menuItemId: number; reasonCode: string; reasonMessage: string }[];
};

export type CustomerMessage = {
  messageType: "Order" | "ServiceRequest" | string;
  sourceId: number;
  title: string;
  status: string;
  message?: string | null;
  createdAt: string;
  items: CustomerMessageItem[];
};

export type CustomerMessageItem = {
  name: string;
  quantity: number;
  note?: string | null;
  lineTotal: number;
  choices: string[];
};
