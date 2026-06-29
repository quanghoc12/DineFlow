# Huong Dan Cau Truc Project DineFlow

Tai lieu nay mo ta cau truc solution DineFlow de team co the code lai tu dau ma van giu dung kien truc, dung module, dung dependency rule.

Ngay cap nhat: 2026-06-16

## 1. Solution Tong The

```text
DineFlow
├── DineFlow.sln
├── src
│   ├── DineFlow.BusinessObjects
│   ├── DineFlow.DataAccessObjects
│   ├── DineFlow.Repositories
│   ├── DineFlow.Services
│   ├── DineFlow.Api
│   ├── DineFlow.WPFApp
│   └── DineFlow.CustomerWeb
├── database
│   ├── seed
│   └── manual
├── docs
│   └── srs
└── README.md
```

## 2. Dependency Rule

Khong di tat qua tang.

```text
DineFlow.BusinessObjects
  Khong reference project nao.

DineFlow.DataAccessObjects
  -> BusinessObjects

DineFlow.Repositories
  -> BusinessObjects
  -> DataAccessObjects

DineFlow.Services
  -> BusinessObjects
  -> Repositories

DineFlow.Api
  -> BusinessObjects
  -> Services

DineFlow.WPFApp
  -> BusinessObjects
  -> Services

DineFlow.CustomerWeb
  -> Goi DineFlow.Api bang HTTP/SignalR
```

Rule bat buoc:

- WPF khong goi DAO hoac `AppDbContext` truc tiep.
- API khong goi DAO hoac `AppDbContext` truc tiep.
- Service chua business rule, validation, transaction.
- Repository la lop trung gian giua service va DAO.
- DAO la noi query database bang EF Core.
- BusinessObjects chi chua entity, DTO, enum, request model.

## 3. Cau Truc Thu Muc Theo Project

### 3.1. DineFlow.BusinessObjects

```text
src/DineFlow.BusinessObjects
├── Auth
├── Bills
├── Common
├── Menu
├── Orders
├── Reports
├── Requests
└── Tables
```

Noi dung:

- `Common`: `BaseEntity`, enum dung chung.
- `Auth`: `User`, login/current user DTO.
- `Tables`: `Area`, `DiningTable`, `TableSession`, `TableSessionCustomer`.
- `Menu`: `Category`, `MenuItem`, menu DTO/request.
- `Orders`: `Order`, `OrderItem`, order DTO/request.
- `Requests`: `ServiceRequest`, request DTO.
- `Bills`: `Bill`, `BillDetail`, `Payment`.
- `Reports`: dashboard/report DTO.

### 3.2. DineFlow.DataAccessObjects

```text
src/DineFlow.DataAccessObjects
├── Auth
├── Bills
├── DbContexts
├── DesignTime
├── Menu
├── Migrations
├── Orders
├── Reports
├── Requests
└── Tables
```

Noi dung:

- `DbContexts/AppDbContext.cs`: nguon schema chinh.
- `DesignTime/AppDbContextFactory.cs`: tao DbContext khi chay migration.
- `Migrations`: EF Core migration.
- Cac folder module: DAO theo entity/module.

### 3.3. DineFlow.Repositories

```text
src/DineFlow.Repositories
├── Auth
├── Bills
├── Menu
├── Orders
├── Reports
├── Requests
└── Tables
```

Moi module nen co:

```text
I<Entity>Repository.cs
<Entity>Repository.cs
```

### 3.4. DineFlow.Services

```text
src/DineFlow.Services
├── Auth
├── Bills
├── Menu
├── Orders
├── Reports
├── Requests
└── Tables
```

Moi module nen co:

```text
I<Entity>Service.cs
<Entity>Service.cs
```

### 3.5. DineFlow.Api

```text
src/DineFlow.Api
├── Controllers
│   ├── Customer
│   └── Staff
├── Hubs
├── Properties
└── Program.cs
```

Quy uoc:

- `Controllers/Customer`: API cho Customer Web QR.
- `Controllers/Staff`: API cho staff/realtime hoac integration.
- `Hubs`: SignalR hubs.
- `Program.cs`: DI, CORS, DbContext, controller, SignalR mapping.

### 3.6. DineFlow.WPFApp

```text
src/DineFlow.WPFApp
├── DependencyInjection
├── Features
│   ├── Auth
│   ├── Billing
│   ├── Dashboard
│   ├── Management
│   └── Operations
├── Helpers
├── Resources
├── Services
├── Shared
├── Shell
└── Views
```

Quy uoc:

- `Features`: UI theo module nghiep vu.
- `Shell`: MainWindow, route, sidebar.
- `Services`: service rieng cho WPF nhu authorization, navigation, dialog, realtime.
- `Shared`: controls, converters, UI models.
- `Resources`: colors, styles.

### 3.7. DineFlow.CustomerWeb

```text
src/DineFlow.CustomerWeb
├── src
│   ├── api
│   ├── features
│   │   ├── menu
│   │   ├── orders
│   │   └── table
│   └── pages
├── index.html
├── package.json
└── vite.config.js
```

Quy uoc:

- `api`: HTTP client, SignalR client.
- `features/table`: client token/table storage.
- `features/menu`: menu API.
- `features/orders`: order API.
- `pages`: QR landing, menu, cart, service request, payment request.

## 4. ERD Da Chot

Khong dung `WaitingPayment`.

```text
Users
Areas
DiningTables
TableSessions
TableSessionCustomers
Categories
MenuItems
Orders
OrderItems
ServiceRequests
Bills
BillDetails
Payments
```

Quan he chinh:

```text
Areas 1 -- N DiningTables
DiningTables 1 -- N TableSessions
TableSessions 1 -- N TableSessionCustomers
TableSessions 1 -- N Orders
TableSessions 1 -- N ServiceRequests
TableSessions 1 -- N Bills
Categories 1 -- N MenuItems
Orders 1 -- N OrderItems
Bills 1 -- N BillDetails
MenuItems 1 -- N BillDetails
Bills 1 -- N Payments
```

Quy tac moi:

- Mot ban thuoc mot khu vuc.
- Mot khu vuc co nhieu ban.
- BillDetails la snapshot tinh tien, khong luu OrderItemId.
- Mot bill co the co nhieu payment records.
- `Payments.BillId` khong unique.

## 5. Thu Tu Code Lai

Nen code theo thu tu:

1. BusinessObjects: entity, enum, DTO.
2. AppDbContext va migration.
3. DAO.
4. Repository interface va implementation.
5. Service interface va implementation.
6. API controller va SignalR hub.
7. WPF UI.
8. Customer Web.
9. Test end-to-end.

## 6. Lenh Kiem Tra

```bash
dotnet restore
dotnet build DineFlow.sln

dotnet ef migrations list \
  --project src/DineFlow.DataAccessObjects/DineFlow.DataAccessObjects.csproj \
  --startup-project src/DineFlow.DataAccessObjects/DineFlow.DataAccessObjects.csproj \
  --context AppDbContext
```
