# DineFlow System Flow - Sales Channel Bill

## Muc tieu

He thong ho tro nhan don tu nhieu kenh ban, vi du:

- `DINE_IN` - khach ngoi tai ban.
- `CUSTOMER_WEB` - khach quet QR tu ban.
- `GRABFOOD` - nhan vien nhap don GrabFood vao app.
- `SHOPEEFOOD` - nhan vien nhap don ShopeeFood vao app.

Nguyen tac moi: **kenh ban di kem voi bill**, khong chi di kem voi order. Bill va bill detail phai luu snapshot de bill cu khong bi thay doi khi admin cap nhat gia kenh ban sau nay.

## Flow 1 - Cau hinh kenh ban va gia

1. Admin tao/cap nhat `SALES_CHANNELS`.
2. Admin cau hinh phu thu theo kenh:
   - `MENU_ITEM_CHANNEL_PRICES` cho mon.
   - `CHOICE_ITEM_CHANNEL_PRICES` cho lua chon.
3. Gia hien thi/them vao bill duoc tinh theo cong thuc:

```text
Final item price = MenuItems.BasePrice + MenuItemChannelPrices.ChannelExtraPrice
Final choice price = ChoiceItems.ExtraPrice + ChoiceItemChannelPrices.ChannelExtraPrice
Bill detail unit price = Final item price + sum(Final choice price)
```

## Flow 2 - Khach quet QR goi mon

1. Khach quet QR ban.
2. Customer Web gui order voi `SalesChannelCode = CUSTOMER_WEB`.
3. He thong tao `ORDERS` va snapshot gia vao:
   - `ORDER_ITEMS.BasePriceSnapshot`
   - `ORDER_ITEMS.ChannelExtraPriceSnapshot`
   - `ORDER_ITEMS.FinalUnitPriceSnapshot`
   - `ORDER_ITEM_SELECTED_CHOICES.*Snapshot`
4. Khi staff xac nhan order, he thong dua order vao default bill.
5. `BILLS` luu kenh ban cua bill.
6. `BILL_DETAILS` luu snapshot gia thanh toan.

## Flow 3 - Staff nhap don GrabFood vao app

1. Staff mo man hinh Order/Bill trong WPF.
2. Staff chon hoac tao bill moi.
3. Staff chon kenh ban `GRABFOOD`.
4. He thong tao bill voi:

```text
BILLS.SalesChannelId
BILLS.SalesChannelCodeSnapshot = "GRABFOOD"
BILLS.SalesChannelNameSnapshot = "GrabFood"
```

5. Staff them mon vao bill.
6. He thong lay gia theo `GRABFOOD` tu `MENU_ITEM_CHANNEL_PRICES` va `CHOICE_ITEM_CHANNEL_PRICES`.
7. Moi dong mon duoc luu vao `BILL_DETAILS`:

```text
BillId
MenuItemId
SalesChannelId
ItemName
ChoiceSummary
Quantity
BasePriceSnapshot
MenuItemChannelExtraPriceSnapshot
ChoiceExtraPriceSnapshot
UnitPrice
TotalPrice
```

8. `BILLS.SubTotal` va `BILLS.FinalAmount` duoc tinh tu `BILL_DETAILS`.

Vi du:

```text
Mon: Com ga
BasePriceSnapshot = 50000
MenuItemChannelExtraPriceSnapshot = 10000
ChoiceExtraPriceSnapshot = 5000
UnitPrice = 65000
Quantity = 2
TotalPrice = 130000
Bill channel = GRABFOOD
```

## Flow 4 - Doi kenh ban cua bill chua thanh toan

Chi ap dung cho bill `Unpaid`.

1. Staff chon bill.
2. Staff doi kenh ban, vi du tu `DINE_IN` sang `GRABFOOD`.
3. He thong cap nhat:

```text
BILLS.SalesChannelId
BILLS.SalesChannelCodeSnapshot
BILLS.SalesChannelNameSnapshot
```

4. He thong tinh lai tung `BILL_DETAILS` theo kenh moi:

```text
BasePriceSnapshot = gia goc hien tai cua mon
MenuItemChannelExtraPriceSnapshot = phu thu mon theo kenh moi
ChoiceExtraPriceSnapshot = tong phu thu choice theo kenh moi
UnitPrice = BasePriceSnapshot + MenuItemChannelExtraPriceSnapshot + ChoiceExtraPriceSnapshot
TotalPrice = UnitPrice * Quantity
```

5. He thong tinh lai `BILLS.SubTotal` va `BILLS.FinalAmount`.

Khong duoc doi kenh ban khi bill da `Paid` hoac `Cancelled`.

## Flow 5 - Thanh toan

1. Staff chon bill `Unpaid`.
2. He thong hien:
   - Kenh ban cua bill.
   - Danh sach bill detail.
   - Gia snapshot da luu.
   - Tong tien.
3. Staff tao `PAYMENTS`.
4. Khi tong payment dat `BILLS.FinalAmount`, bill chuyen sang `Paid`.
5. Bill `Paid` bi khoa, khong duoc sua kenh ban, sua gia, sua so luong, split/move item.

## Flow 6 - Bao cao

Bao cao doanh thu doc tu bill da thanh toan:

```text
BILLS.Status = Paid
```

Bao cao theo kenh ban doc tu:

```text
BILLS.SalesChannelId
BILLS.SalesChannelCodeSnapshot
BILLS.SalesChannelNameSnapshot
```

Top mon ban chay doc tu:

```text
BILL_DETAILS
join BILLS
where BILLS.Status = Paid
```

Gia tri doanh thu dung snapshot trong `BILL_DETAILS`, khong tinh lai tu menu hien tai.

## Business Rules

- `BILLS` bat buoc co `SalesChannelId`.
- `BILLS` luu snapshot code/name cua kenh ban tai thoi diem tao/thanh toan.
- `BILL_DETAILS` luu snapshot gia tai thoi diem them vao bill hoac doi kenh bill.
- Sua gia GrabFood trong menu khong anh huong bill cu.
- Bill `Paid` va `Cancelled` khong duoc doi kenh ban.
- Split/move item giua bill chi hop le khi source/target cung kenh ban, hoac he thong phai tinh lai gia theo kenh cua target bill.
- Bao cao doanh thu theo kenh ban dua tren `BILLS`, khong dua tren `ORDERS`.

## Mapping bang chinh

```text
SALES_CHANNELS
  -> cau hinh kenh ban

MENU_ITEM_CHANNEL_PRICES
CHOICE_ITEM_CHANNEL_PRICES
  -> cau hinh phu thu theo kenh

BILLS
  -> header bill va kenh ban cua bill

BILL_DETAILS
  -> snapshot dong mon va gia da ban

PAYMENTS
  -> lich su thanh toan cua bill
```
