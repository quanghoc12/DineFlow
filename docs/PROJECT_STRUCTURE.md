# Project Structure

```text
DineFlow
├── src
│   ├── DineFlow.BusinessObjects      # Entity, DTO, Enum
│   ├── DineFlow.DataAccessObjects    # AppDbContext, DAO, Migrations
│   ├── DineFlow.Repositories         # Repository interfaces + implementations
│   ├── DineFlow.Services             # Business rules, validations, transactions
│   ├── DineFlow.Api                  # ASP.NET Core API + SignalR
│   ├── DineFlow.WPFApp               # Staff/Admin WPF app
│   └── DineFlow.CustomerWeb          # Customer QR web React/Vite
├── database
│   ├── seed                          # SQL seed data only
│   └── manual                        # reset/manual script only
└── docs
```

## Rule dependency

```text
BusinessObjects
  Không reference project nào.

DataAccessObjects
  Reference BusinessObjects.

Repositories
  Reference BusinessObjects + DataAccessObjects.

Services
  Reference BusinessObjects + Repositories.

Api
  Reference BusinessObjects + Services.

WPFApp
  Reference BusinessObjects + Services.

CustomerWeb
  Gọi API qua HTTP/SignalR.
```
