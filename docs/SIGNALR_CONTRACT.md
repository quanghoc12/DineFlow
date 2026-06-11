# SignalR Contract

## Hubs

```text
/hubs/staff
/hubs/customer
```

## Groups

```text
staff
customer-{clientToken}
table-{tableId}
```

## Server -> WPF

```text
NewOrderAccepted
OrderPrintRequested
OrderCancelled
OrderPrintSucceeded
OrderPrintFailed
BillUpdated
ServiceRequestCreated
PaymentRequestCreated
MenuItemSoldOut
TableStatusChanged
PaymentConfirmed
```

## Server -> Customer Web

```text
OrderAccepted
OrderPartiallyAccepted
OrderFailed
OrderCancelled
ServiceRequestConfirmed
ServiceRequestCompleted
PaymentRequestConfirmed
PaymentRequestCompleted
```
