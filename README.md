# DineFlow - QR Food Ordering Management System

Bộ khung dự án nhóm theo hướng **Feature-based + Code First + EF Core Migration**.

## Kiến trúc tổng thể

```text
Customer Web QR (React/Vite)
    ↓ HTTP/SignalR
DineFlow.Api (ASP.NET Core 10 Web API + SignalR)
    ↓
DineFlow.Services
    ↓
DineFlow.Repositories
    ↓
DineFlow.DataAccessObjects
    ↓ EF Core Migration / AppDbContext
PostgreSQL Docker

DineFlow.WPFApp (.NET 8 WPF)
    ↓
DineFlow.Services
    ↓
DineFlow.Repositories
    ↓
DineFlow.DataAccessObjects
    ↓ EF Core
PostgreSQL Docker
```

## Stack rule

```text
WPF Staff App: .NET 8 / net8.0-windows
Customer/API Web: ASP.NET Core 10 API + React/Vite customer web
Database: PostgreSQL in Docker
Shared layers: net8.0 so WPF and API can both reference them
```

## Project structure

```text
DineFlow.sln
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
├── .config
├── .gitignore
└── README.md
```

## Setup nhanh

### 1. Start PostgreSQL

```bash
docker compose up -d postgres
```

Default local connection:

```text
Host=localhost;Port=5433;Database=dineflow;Username=dineflow_user;Password=dineflow_password
```

### 2. Restore .NET

```bash
dotnet tool restore
dotnet restore
```

### 3. Build backend/WPF projects

```bash
dotnet build DineFlow.sln
```

### 4. Tạo database bằng Migration

Chỉ DB owner/leader chạy lệnh này:

```bash
dotnet ef migrations add InitialCreate \
  --project src/DineFlow.DataAccessObjects \
  --startup-project src/DineFlow.Api \
  --context AppDbContext \
  --output-dir Migrations


dotnet ef database update \
  --project src/DineFlow.DataAccessObjects \
  --startup-project src/DineFlow.Api \
  --context AppDbContext
```

### 5. Chạy API

```bash
dotnet run --project src/DineFlow.Api
```

### 6. Chạy WPF

Mở solution bằng Visual Studio 2022, set startup project là `DineFlow.WPFApp`.

### 7. Chạy Customer Web

```bash
cd src/DineFlow.CustomerWeb
npm install
npm run dev
```

## Rule chính

- WPFApp không gọi DAO hoặc DbContext trực tiếp.
- WPFApp chỉ gọi Services.
- Api chỉ gọi Services.
- Services gọi Repositories.
- Repositories gọi DAO.
- DAO dùng AppDbContext/EF Core để thao tác PostgreSQL.
- Migration chỉ do DB owner/leader tạo.
- CustomerWeb chỉ gọi API, không truy cập database.
