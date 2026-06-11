# API Contract Draft

## Auth

```text
POST /api/auth/login
```

## Customer

```text
GET  /api/customer/tables/by-token/{token}
GET  /api/customer/menu
POST /api/customer/orders
POST /api/customer/service-requests/call-staff
POST /api/customer/service-requests/payment-request
```

## Staff

```text
GET  /api/staff/orders
GET  /api/staff/orders/{id}
PUT  /api/staff/orders/{id}/mark-printed
PUT  /api/staff/orders/{id}/mark-print-failed
PUT  /api/staff/orders/{id}/cancel
```

## Bills

```text
GET  /api/staff/sessions/{tableSessionId}/bills
POST /api/staff/bills/{sourceBillId}/split
POST /api/staff/bills/move-item
POST /api/staff/bills/{billId}/confirm-payment
```

## Dashboard

```text
GET /api/revenue/dashboard
```
