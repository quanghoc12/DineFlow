# DineFlow Project Structure Explanation

> Tài liệu này giải thích chi tiết cấu trúc project **DineFlow_FullStack_MigrationBase**.  
> Mục tiêu là giúp cả nhóm hiểu rõ: mỗi project dùng để làm gì, luồng gọi code đi như thế nào, khi làm module mới thì thêm file ở đâu, migration ai quản lý, web đặt ở đâu, API đặt ở đâu, WPF gọi tầng nào.

---

## 1. Tổng quan kiến trúc

DineFlow là project quản lý gọi món bằng QR cho quán ăn, gồm 3 phần chạy chính:

```txt
Customer Web QR
    ↓ gọi HTTP/SignalR
DineFlow.Api
    ↓ gọi Service
DineFlow.Services
    ↓ gọi Repository
DineFlow.Repositories
    ↓ gọi DAO
DineFlow.DataAccessObjects
    ↓ dùng EF Core/AppDbContext
SQL Server
```

WPF Staff App cũng dùng chung tầng Service:

```txt
DineFlow.WPFApp
    ↓ gọi Service
DineFlow.Services
    ↓ gọi Repository
DineFlow.Repositories
    ↓ gọi DAO
DineFlow.DataAccessObjects
    ↓ dùng EF Core/AppDbContext
SQL Server
```

Rule quan trọng:

```txt
CustomerWeb không gọi database trực tiếp.
WPFApp không gọi DbContext trực tiếp.
WPFApp không gọi DAO trực tiếp.
WPFApp không viết SQL trực tiếp.
Api không gọi DAO trực tiếp nếu đã có Service.
Service xử lý business rule.
Repository làm lớp trung gian giữa Service và DAO.
DAO mới là nơi dùng AppDbContext để truy vấn database.
BusinessObjects chỉ chứa Entity, DTO, Enum.
```

---

## 2. Cấu trúc thư mục gốc

```txt
DineFlow_FullStack_MigrationBase
│
├── DineFlow.sln
├── README.md
├── .gitignore
├── .editorconfig
│
├── .config
│   └── dotnet-tools.json
│
├── docs
│   ├── PROJECT_STRUCTURE.md
│   ├── MIGRATION_RULES.md
│   ├── GIT_WORKFLOW.md
│   ├── MEMBER_ASSIGNMENT.md
│   ├── API_CONTRACT.md
│   ├── SIGNALR_CONTRACT.md
│   └── FEATURE_CHECKLIST.md
│
├── database
│   ├── seed
│   │   └── SeedData.sql
│   └── manual
│       └── reset-local-db.sql
│
└── src
    ├── DineFlow.BusinessObjects
    ├── DineFlow.DataAccessObjects
    ├── DineFlow.Repositories
    ├── DineFlow.Services
    ├── DineFlow.Api
    ├── DineFlow.WPFApp
    └── DineFlow.CustomerWeb
```

Ý nghĩa nhanh:

| Thành phần | Ý nghĩa |
|---|---|
| `DineFlow.sln` | Solution chính mở bằng Visual Studio |
| `src/` | Chứa toàn bộ source code chính |
| `docs/` | Tài liệu rule, migration, Git, API, SignalR, phân công |
| `database/` | Script seed/reset phụ trợ, không phải nguồn chính của schema |
| `.config/dotnet-tools.json` | Cấu hình local tool, ví dụ `dotnet-ef` |
| `.gitignore` | Chặn commit `bin/`, `obj/`, `.env`, appsettings local |
| `.editorconfig` | Chuẩn format code chung |

---

## 3. Vì sao có nhiều project?

Dự án không gom tất cả code vào một project WPF. Lý do là để chia rõ trách nhiệm:

```txt
BusinessObjects      → định nghĩa dữ liệu
DataAccessObjects    → truy cập database
Repositories         → abstraction cho DAO
Services             → nghiệp vụ và validate
Api                  → backend cho CustomerWeb và WPF nếu cần
WPFApp               → giao diện nhân viên/admin
CustomerWeb          → web QR cho khách hàng
```

Lợi ích:

```txt
Dễ chia member theo feature.
Dễ debug từng tầng.
Dễ test nghiệp vụ ở Service.
Dễ thay UI mà không phá database.
Dễ dùng chung business logic giữa API và WPF.
Dễ bảo vệ rule: UI không truy cập database trực tiếp.
```

---

## 4. DineFlow.BusinessObjects

### 4.1. Vai trò

`DineFlow.BusinessObjects` chứa:

```txt
Entity
DTO
Enum
Base model
```

Không chứa:

```txt
Không query database.
Không dùng AppDbContext.
Không dùng MessageBox.
Không gọi Service.
Không gọi Repository.
Không gọi DAO.
Không chứa business logic dài.
```

### 4.2. Cấu trúc hiện tại

```txt
src/DineFlow.BusinessObjects
│
├── Common
│   ├── AppEnums.cs
│   └── BaseEntity.cs
│
├── Auth
│   ├── User.cs
│   ├── LoginRequestDto.cs
│   └── CurrentUserDto.cs
│
├── Tables
│   ├── DiningTable.cs
│   ├── TableSession.cs
│   └── TableSessionCustomer.cs
│
├── Menu
│   ├── Category.cs
│   └── MenuItem.cs
│
├── Orders
│   ├── Order.cs
│   └── OrderItem.cs
│
├── Requests
│   └── ServiceRequest.cs
│
├── Bills
│   ├── Bill.cs
│   ├── BillDetail.cs
│   └── Payment.cs
│
└── Reports
    └── DashboardDto.cs
```

### 4.3. Giải thích từng nhóm

#### `Common/AppEnums.cs`

Chứa enum dùng chung toàn hệ thống, ví dụ:

```txt
UserRole
DiningTableStatus
TableSessionStatus
OrderStatus
PrintStatus
ServiceRequestType
ServiceRequestStatus
BillStatus
PaymentMethod
```

Mục tiêu là tránh viết status bằng string lung tung trong code.

#### `Common/BaseEntity.cs`

Chứa field chung như:

```txt
CreatedAt
UpdatedAt
```

Entity nào cần timestamp có thể kế thừa hoặc tự dùng pattern tương tự.

#### `Auth/User.cs`

Entity tài khoản nhân viên/admin.

Thường map với bảng:

```txt
Users
```

Dùng cho:

```txt
Login
Phân quyền Admin/Staff
Audit CreatedBy/CancelledBy/PaidBy
```

#### `Auth/LoginRequestDto.cs`

DTO nhận dữ liệu đăng nhập:

```txt
Username
Password
```

DTO này không phải bảng database.

#### `Auth/CurrentUserDto.cs`

DTO trả về thông tin user sau khi login thành công:

```txt
UserId
Username
FullName
Role
```

Dùng để WPF/API biết user hiện tại là ai.

#### `Tables/`

Chứa entity nghiệp vụ bàn và phiên bàn:

```txt
DiningTable              → bàn ăn
TableSession             → phiên khách đang ngồi tại bàn
TableSessionCustomer     → thiết bị/khách trong một phiên bàn
```

Đây là nhóm rất quan trọng vì order, request, bill đều gắn với `TableSession`.

#### `Menu/`

Chứa dữ liệu menu:

```txt
Category                 → loại món
MenuItem                 → món ăn/đồ uống
```

Có liên quan đến stock:

```txt
TrackStock
AvailableQuantity
IsAvailable
IsActive
```

#### `Orders/`

Chứa dữ liệu order:

```txt
Order                    → một lần gửi order
OrderItem                → từng món trong order
```

OrderItem nên lưu snapshot:

```txt
ItemName
UnitPrice
TotalPrice
```

để lịch sử không bị đổi khi giá món thay đổi.

#### `Requests/ServiceRequest.cs`

Dùng cho:

```txt
Khách gọi nhân viên
Khách yêu cầu thanh toán
```

Có type:

```txt
CallStaff
PaymentRequest
```

#### `Bills/`

Chứa thanh toán:

```txt
Bill                     → phiếu thanh toán
BillDetail               → món trong bill
Payment                  → giao dịch thanh toán của bill
```

Dùng cho:

```txt
Xem bill
Tách bill
Chuyển món giữa bill
Xác nhận thanh toán
Dashboard doanh thu
```

#### `Reports/DashboardDto.cs`

DTO trả dữ liệu dashboard:

```txt
RevenueToday
PaidBillCount
OrderCount
TopSellingItems
PrintFailedCount
```

---

## 5. DineFlow.DataAccessObjects

### 5.1. Vai trò

`DineFlow.DataAccessObjects` là tầng truy cập database.

Nó chứa:

```txt
AppDbContext
DesignTimeDbContextFactory
Migrations
DAO theo từng module
```

Tầng này được phép dùng:

```txt
EF Core
AppDbContext
DbSet
Include
Where
OrderBy
SaveChanges
Transaction nếu cần
```

Không được dùng:

```txt
MessageBox
XAML
UI control
Business rule dài
Phân quyền UI
```

### 5.2. Cấu trúc hiện tại

```txt
src/DineFlow.DataAccessObjects
│
├── DbContexts
│   └── AppDbContext.cs
│
├── DesignTime
│   └── AppDbContextFactory.cs
│
├── Migrations
│   └── README.md
│
├── Auth
│   └── UserDAO.cs
│
├── Menu
│   ├── CategoryDAO.cs
│   └── MenuItemDAO.cs
│
├── Tables
├── Orders
├── Requests
├── Bills
└── Reports
```

### 5.3. `DbContexts/AppDbContext.cs`

Đây là file trung tâm của database.

Nhiệm vụ:

```txt
Khai báo DbSet cho các bảng.
Cấu hình khóa chính.
Cấu hình FK.
Cấu hình unique index.
Cấu hình filtered index.
Cấu hình decimal(18,2).
Cấu hình enum conversion.
Cấu hình delete behavior.
```

Ví dụ:

```csharp
public DbSet<User> Users => Set<User>();
public DbSet<DiningTable> DiningTables => Set<DiningTable>();
public DbSet<TableSession> TableSessions => Set<TableSession>();
public DbSet<Category> Categories => Set<Category>();
public DbSet<MenuItem> MenuItems => Set<MenuItem>();
public DbSet<Order> Orders => Set<Order>();
public DbSet<OrderItem> OrderItems => Set<OrderItem>();
public DbSet<Bill> Bills => Set<Bill>();
public DbSet<Payment> Payments => Set<Payment>();
```

Khi dùng Code First + Migration, `AppDbContext` là nơi EF Core đọc để sinh database schema.

### 5.4. `DesignTime/AppDbContextFactory.cs`

File này giúp chạy lệnh migration bằng CLI:

```bash
dotnet ef migrations add InitialCreate
```

Khi chạy migration, EF Core cần biết cách tạo `AppDbContext`. `AppDbContextFactory` cung cấp connection string và cấu hình context ở design-time.

### 5.5. `Migrations/`

Thư mục chứa migration do EF Core sinh ra.

Rule:

```txt
Chỉ DB owner/leader được tạo migration.
Member thường không tự add migration.
Không tự sửa file migration nếu không hiểu rõ.
Migration phải được commit lên Git.
Cả nhóm pull migration mới rồi chạy database update.
```

### 5.6. DAO là gì?

DAO là lớp thao tác database cho từng entity/module.

Ví dụ:

```txt
Auth/UserDAO.cs
Menu/CategoryDAO.cs
Menu/MenuItemDAO.cs
```

DAO thường có các hàm:

```txt
GetAll
GetById
Find/Search
Add
Update
Delete
```

Ví dụ flow:

```txt
MenuItemService.GetAvailableItems()
    ↓
MenuItemRepository.GetAvailableItems()
    ↓
MenuItemDAO.GetAvailableItems()
    ↓
AppDbContext.MenuItems.Where(...)
```

---

## 6. DineFlow.Repositories

### 6.1. Vai trò

Repository là lớp trung gian giữa Service và DAO.

Mục tiêu:

```txt
Service không phụ thuộc trực tiếp DAO.
Dễ mock/test.
Dễ thay đổi cách truy vấn sau này.
Giữ kiến trúc nhiều layer.
```

### 6.2. Cấu trúc hiện tại

```txt
src/DineFlow.Repositories
│
├── Auth
│   ├── IUserRepository.cs
│   └── UserRepository.cs
│
├── Menu
│   ├── ICategoryRepository.cs
│   ├── CategoryRepository.cs
│   ├── IMenuItemRepository.cs
│   └── MenuItemRepository.cs
│
├── Tables
├── Orders
├── Requests
├── Bills
└── Reports
```

### 6.3. Interface Repository

Mỗi repository phải có interface.

Ví dụ:

```csharp
public interface IUserRepository
{
    List<User> GetAll();
    User? GetById(int id);
    User? GetByUsername(string username);
    User Add(User user);
    void Update(User user);
    void Delete(int id);
}
```

### 6.4. Implementation Repository

Implementation gọi xuống DAO.

Ví dụ:

```csharp
public class UserRepository : IUserRepository
{
    private readonly UserDAO _userDAO;

    public UserRepository()
    {
        _userDAO = new UserDAO();
    }

    public User? GetByUsername(string username)
    {
        return _userDAO.GetByUsername(username);
    }
}
```

Repository không được:

```txt
Không MessageBox.
Không xử lý UI.
Không validate form.
Không xử lý business rule dài.
```

---

## 7. DineFlow.Services

### 7.1. Vai trò

`DineFlow.Services` là nơi chứa nghiệp vụ chính.

Service được phép:

```txt
Validate dữ liệu.
Kiểm tra quyền.
Tính tiền.
Xử lý order.
Trừ stock.
Rollback stock khi cancel order.
Tạo bill mặc định.
Tách bill.
Confirm payment.
Gọi nhiều repository trong một nghiệp vụ.
Dùng transaction nếu nghiệp vụ cần nhiều bảng.
```

Service không được:

```txt
Không MessageBox.
Không code XAML.
Không gọi trực tiếp control WPF.
Không viết SQL trực tiếp.
Không gọi DbContext trực tiếp nếu đã có DAO/Repository.
```

### 7.2. Cấu trúc hiện tại

```txt
src/DineFlow.Services
│
├── Auth
│   ├── IAuthService.cs
│   └── AuthService.cs
│
├── Menu
│   ├── ICategoryService.cs
│   ├── CategoryService.cs
│   ├── IMenuItemService.cs
│   └── MenuItemService.cs
│
├── Tables
├── Orders
├── Requests
├── Bills
└── Reports
```

### 7.3. Ví dụ AuthService

`AuthService` xử lý:

```txt
Username có rỗng không.
Password có rỗng không.
User có tồn tại không.
User có active không.
Password đúng không.
Trả về CurrentUserDto.
```

WPF hoặc API chỉ nhận kết quả, không tự xử lý nghiệp vụ login.

### 7.4. Ví dụ MenuItemService

`MenuItemService` xử lý:

```txt
Tên món không được rỗng.
Giá không âm.
Nếu TrackStock = true thì AvailableQuantity phải >= 0.
Nếu AvailableQuantity = 0 thì IsAvailable = false.
```

Đây là business rule, nên nằm ở Service chứ không nằm trong WPF.

---

## 8. DineFlow.Api

### 8.1. Vai trò

`DineFlow.Api` là ASP.NET Core Web API.

Nó phục vụ:

```txt
Customer Web QR
WPF nếu cần gọi backend HTTP
SignalR realtime
Swagger/API test
```

API không nên chứa business rule dài. API chỉ nhận request, gọi Service, trả response.

### 8.2. Cấu trúc hiện tại

```txt
src/DineFlow.Api
│
├── Program.cs
├── appsettings.example.json
│
├── Controllers
│   ├── HealthController.cs
│   ├── AuthController.cs
│   │
│   ├── Customer
│   │   └── CustomerMenuController.cs
│   │
│   └── Staff
│       └── StaffMenuItemController.cs
│
└── Hubs
    ├── StaffHub.cs
    └── CustomerHub.cs
```

### 8.3. `Program.cs`

File cấu hình API:

```txt
AddControllers
AddEndpointsApiExplorer
AddSwaggerGen
AddSignalR
AddCors
MapControllers
MapHub
```

Nơi đăng ký Service/Repository nếu chuyển sang DI đầy đủ.

### 8.4. Controllers

Controller chia theo nhóm:

```txt
Controllers/AuthController.cs
Controllers/Customer/*
Controllers/Staff/*
Controllers/Admin/* nếu thêm sau
```

Ví dụ:

```txt
CustomerMenuController       → API cho khách xem menu
StaffMenuItemController      → API cho nhân viên quản lý món
AuthController               → login
HealthController             → kiểm tra API chạy
```

Rule:

```txt
Controller nhận request.
Controller validate format cơ bản nếu cần.
Controller gọi Service.
Controller trả HTTP response.
Controller không gọi DAO trực tiếp.
Controller không viết SQL.
```

### 8.5. Hubs

SignalR Hub dùng realtime.

```txt
StaffHub      → realtime cho WPF Staff
CustomerHub   → realtime cho Customer Web
```

Event dự kiến:

```txt
NewOrderAccepted
OrderPrintRequested
ServiceRequestCreated
PaymentRequestCreated
BillUpdated
TableStatusChanged
PaymentConfirmed
```

---

## 9. DineFlow.WPFApp

### 9.1. Vai trò

`DineFlow.WPFApp` là app desktop cho nhân viên/admin.

Dùng để:

```txt
Đăng nhập
Xem bàn/session
Quản lý món/stock
Xem order
In order
Xử lý gọi nhân viên
Xử lý yêu cầu thanh toán
Tách bill
Confirm payment
Dashboard doanh thu
```

### 9.2. Cấu trúc hiện tại

```txt
src/DineFlow.WPFApp
│
├── App.xaml
├── App.xaml.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
├── appsettings.example.json
├── appsettings.Development.example.json
│
├── Views
│   ├── LoginWindow.xaml
│   └── LoginWindow.xaml.cs
│
├── Helpers
│   ├── MessageBoxHelper.cs
│   ├── NavigationHelper.cs
│   └── ValidationHelper.cs
│
└── Resources
    ├── Colors.xaml
    └── Styles.xaml
```

### 9.3. WPF gọi Service như thế nào?

Đúng:

```csharp
private readonly IAuthService _authService;

public LoginWindow()
{
    InitializeComponent();
    _authService = new AuthService();
}

private void btnLogin_Click(object sender, RoutedEventArgs e)
{
    try
    {
        var user = _authService.Login(new LoginRequestDto
        {
            Username = txtUsername.Text.Trim(),
            Password = txtPassword.Password
        });

        MessageBox.Show($"Xin chào {user.FullName}");
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.Message);
    }
}
```

Sai:

```csharp
using var db = new AppDbContext();
var user = db.Users.FirstOrDefault(...);
```

WPF không được gọi DbContext trực tiếp.

### 9.4. Helpers

#### `MessageBoxHelper.cs`

Tập trung hiển thị thông báo.

#### `NavigationHelper.cs`

Điều hướng giữa các Window/UserControl.

#### `ValidationHelper.cs`

Validate format input cơ bản như parse số, kiểm tra textbox rỗng.  
Lưu ý: business rule vẫn phải nằm ở Service.

### 9.5. Resources

```txt
Colors.xaml      → màu dùng chung
Styles.xaml      → style button, textbox, datagrid
```

Giúp UI thống nhất giữa các member.

---

## 10. DineFlow.CustomerWeb

### 10.1. Vai trò

`DineFlow.CustomerWeb` là web QR cho khách hàng.

Khách dùng để:

```txt
Quét QR
Xem menu
Thêm món vào giỏ
Gửi order
Gọi nhân viên
Yêu cầu thanh toán
Theo dõi phản hồi realtime
```

CustomerWeb không gọi database. CustomerWeb chỉ gọi API.

### 10.2. Cấu trúc hiện tại

```txt
src/DineFlow.CustomerWeb
│
├── package.json
├── index.html
├── vite.config.js
├── .env.example
│
└── src
    ├── main.jsx
    ├── App.jsx
    ├── styles.css
    │
    ├── api
    │   ├── httpClient.js
    │   └── customerHub.js
    │
    ├── pages
    │   ├── QrLandingPage.jsx
    │   ├── MenuPage.jsx
    │   ├── CartPage.jsx
    │   ├── ServiceRequestPage.jsx
    │   ├── PaymentRequestPage.jsx
    │   └── NotFoundPage.jsx
    │
    └── features
        ├── menu
        │   └── menuApi.js
        └── table
            └── tableStorage.js
```

### 10.3. `.env.example`

Chứa cấu hình API URL:

```env
VITE_API_BASE_URL=https://localhost:7001
VITE_SIGNALR_URL=https://localhost:7001/hubs/customer
```

Mỗi máy copy thành `.env` và sửa port nếu cần.  
Không commit `.env` thật lên Git.

### 10.4. `api/httpClient.js`

File cấu hình HTTP client để gọi API.

Tất cả API call nên đi qua file này để dễ đổi base URL.

### 10.5. `api/customerHub.js`

File kết nối SignalR cho Customer Web.

Dùng để nhận event realtime như:

```txt
OrderAccepted
OrderFailed
ServiceRequestConfirmed
PaymentRequestCompleted
```

### 10.6. Pages

#### `QrLandingPage.jsx`

Đọc token từ URL, ví dụ:

```txt
?t=abc123
```

Sau đó gọi API xác định bàn.

#### `MenuPage.jsx`

Hiển thị category và menu item.

#### `CartPage.jsx`

Hiển thị món đã chọn, tăng/giảm số lượng, gửi order.

#### `ServiceRequestPage.jsx`

Khách gọi nhân viên.

#### `PaymentRequestPage.jsx`

Khách yêu cầu thanh toán.

#### `NotFoundPage.jsx`

Trang lỗi route không tồn tại.

---

## 11. database/

### 11.1. Vai trò

Vì project dùng Code First + Migration, schema chính không nằm trong SQL script.

Schema chính được sinh từ:

```txt
BusinessObjects Entity
+
AppDbContext Fluent API
+
EF Core Migration
```

Thư mục `database/` chỉ dùng phụ trợ:

```txt
Seed data mẫu
Reset database local
Manual script đặc biệt
```

### 11.2. Cấu trúc

```txt
database
├── seed
│   └── SeedData.sql
└── manual
    └── reset-local-db.sql
```

### 11.3. `SeedData.sql`

Dùng để thêm dữ liệu mẫu sau khi migration tạo database.

Ví dụ:

```txt
Admin user
Staff user
Category mẫu
Menu item mẫu
Dining table mẫu
```

### 11.4. `reset-local-db.sql`

Dùng để reset database local khi cần demo/test lại.

Không nên dùng script này trên database production/demo chính nếu chưa backup.

---

## 12. docs/

### 12.1. Vai trò

`docs/` là nơi nhóm đọc rule trước khi code.

```txt
docs
├── PROJECT_STRUCTURE.md
├── MIGRATION_RULES.md
├── GIT_WORKFLOW.md
├── MEMBER_ASSIGNMENT.md
├── API_CONTRACT.md
├── SIGNALR_CONTRACT.md
└── FEATURE_CHECKLIST.md
```

### 12.2. `PROJECT_STRUCTURE.md`

Tóm tắt cấu trúc project.

### 12.3. `MIGRATION_RULES.md`

Rule quan trọng nhất khi dùng Code First:

```txt
Chỉ DB owner tạo migration.
Member không tự tạo migration.
Đổi Entity phải báo DB owner.
Migration phải build được trước khi merge.
```

### 12.4. `GIT_WORKFLOW.md`

Quy định branch:

```txt
main
 develop
 feature/auth-login
 feature/menu-stock
 feature/order-print
 feature/bill-payment
```

Quy định commit:

```txt
feat:
fix:
refactor:
ui:
docs:
test:
chore:
```

### 12.5. `MEMBER_ASSIGNMENT.md`

Phân công member theo feature.

### 12.6. `API_CONTRACT.md`

Ghi endpoint backend.

Ví dụ:

```txt
POST /api/auth/login
GET  /api/customer/menu
POST /api/customer/orders
GET  /api/staff/orders
POST /api/staff/bills/{id}/confirm-payment
```

### 12.7. `SIGNALR_CONTRACT.md`

Ghi event realtime.

Ví dụ:

```txt
NewOrderAccepted
OrderPrintRequested
ServiceRequestCreated
PaymentRequestCreated
BillUpdated
```

### 12.8. `FEATURE_CHECKLIST.md`

Checklist trước khi báo xong feature.

---

## 13. Luồng gọi code theo từng case

### 13.1. Login từ WPF

```txt
LoginWindow.xaml.cs
    ↓
AuthService.Login()
    ↓
UserRepository.GetByUsername()
    ↓
UserDAO.GetByUsername()
    ↓
AppDbContext.Users
    ↓
SQL Server
```

WPF chỉ hiển thị kết quả hoặc lỗi.

### 13.2. Customer Web xem menu

```txt
MenuPage.jsx
    ↓
menuApi.js
    ↓
GET /api/customer/menu
    ↓
CustomerMenuController
    ↓
MenuItemService.GetAvailableItems()
    ↓
MenuItemRepository
    ↓
MenuItemDAO
    ↓
AppDbContext.MenuItems
    ↓
SQL Server
```

CustomerWeb không truy cập database.

### 13.3. Staff cập nhật món từ WPF

```txt
MenuItemManagementWindow.xaml.cs
    ↓
MenuItemService.Update()
    ↓
MenuItemRepository.Update()
    ↓
MenuItemDAO.Update()
    ↓
AppDbContext.SaveChanges()
    ↓
SQL Server
```

Business rule như giá không âm, stock không âm phải nằm ở Service.

### 13.4. Customer gửi order

Flow sau này nên làm:

```txt
CartPage.jsx
    ↓
POST /api/customer/orders
    ↓
CustomerOrderController
    ↓
OrderService.CreateCustomerOrder()
    ↓
TableSessionRepository
    ↓
OrderRepository
    ↓
MenuItemRepository
    ↓
BillRepository
    ↓
DAO/AppDbContext transaction
    ↓
SQL Server
    ↓
SignalR NewOrderAccepted / OrderPrintRequested
```

Nghiệp vụ order cần transaction vì liên quan nhiều bảng:

```txt
TableSessions
TableSessionCustomers
Orders
OrderItems
MenuItems stock
Bills
BillDetails
```

---

## 14. Migration trong cấu trúc này

### 14.1. Project dùng migration như thế nào?

Project dùng hướng:

```txt
Code First + EF Core Migration
```

Nghĩa là:

```txt
Viết Entity C# trước
    ↓
Cấu hình AppDbContext
    ↓
DB owner tạo migration
    ↓
EF Core sinh database schema
```

### 14.2. Lệnh tạo migration

Chạy ở thư mục gốc:

```bash
dotnet tool restore
dotnet restore
dotnet build DineFlow.sln
```

Tạo migration:

```bash
dotnet ef migrations add InitialCreate \
  --project src/DineFlow.DataAccessObjects \
  --startup-project src/DineFlow.Api \
  --context AppDbContext \
  --output-dir Migrations
```

Update database:

```bash
dotnet ef database update \
  --project src/DineFlow.DataAccessObjects \
  --startup-project src/DineFlow.Api \
  --context AppDbContext
```

### 14.3. Ai được tạo migration?

Chỉ DB owner/leader.

Member khác làm feature thì:

```txt
Tạo/sửa Entity của module mình.
Báo DB owner nếu thay đổi ảnh hưởng database.
DB owner review Entity và AppDbContext.
DB owner tạo migration.
DB owner commit migration.
Cả nhóm pull về và database update.
```

### 14.4. Vì sao không để ai cũng migration?

Vì sẽ dễ:

```txt
Conflict migration snapshot.
Database mỗi máy lệch version.
Merge Git khó.
Mất kiểm soát FK/index/constraint.
```

---

## 15. Khi member thêm module mới thì làm ở đâu?

Ví dụ member làm module `Tables`.

### 15.1. BusinessObjects

```txt
src/DineFlow.BusinessObjects/Tables
├── DiningTable.cs
├── TableSession.cs
└── TableSessionCustomer.cs
```

Nếu cần DTO:

```txt
TableDto.cs
TableSessionDetailDto.cs
CreateTableRequestDto.cs
```

### 15.2. DataAccessObjects

```txt
src/DineFlow.DataAccessObjects/Tables
├── DiningTableDAO.cs
├── TableSessionDAO.cs
└── TableSessionCustomerDAO.cs
```

### 15.3. Repositories

```txt
src/DineFlow.Repositories/Tables
├── IDiningTableRepository.cs
├── DiningTableRepository.cs
├── ITableSessionRepository.cs
└── TableSessionRepository.cs
```

### 15.4. Services

```txt
src/DineFlow.Services/Tables
├── IDiningTableService.cs
├── DiningTableService.cs
├── ITableSessionService.cs
└── TableSessionService.cs
```

### 15.5. API

```txt
src/DineFlow.Api/Controllers/Admin/AdminTableController.cs
src/DineFlow.Api/Controllers/Staff/StaffTableSessionController.cs
src/DineFlow.Api/Controllers/Customer/CustomerTableController.cs
```

### 15.6. WPF

```txt
src/DineFlow.WPFApp/Views/TableManagementWindow.xaml
src/DineFlow.WPFApp/Views/TableSessionWindow.xaml
src/DineFlow.WPFApp/Views/TableSessionDetailWindow.xaml
```

### 15.7. CustomerWeb

Nếu khách cần dùng:

```txt
src/DineFlow.CustomerWeb/src/features/table
├── tableApi.js
├── tableStorage.js
└── useTableToken.js
```

### 15.8. Migration

Member không tự tạo migration.  
DB owner cập nhật `AppDbContext` nếu cần rồi tạo migration.

---

## 16. File nào là file dùng chung dễ conflict?

Các file cần owner:

```txt
DineFlow.sln
src/DineFlow.DataAccessObjects/DbContexts/AppDbContext.cs
src/DineFlow.DataAccessObjects/Migrations/*
src/DineFlow.Api/Program.cs
src/DineFlow.WPFApp/App.xaml
src/DineFlow.WPFApp/MainWindow.xaml
src/DineFlow.CustomerWeb/src/App.jsx
docs/API_CONTRACT.md
docs/SIGNALR_CONTRACT.md
docs/MEMBER_ASSIGNMENT.md
```

Rule:

```txt
Không tự sửa file dùng chung nếu chưa báo nhóm.
Nếu cần sửa, tạo branch riêng và nói rõ lý do trong pull request.
```

---

## 17. Gợi ý phân công member

| Member | Module | Phạm vi chính |
|---|---|---|
| Member 1 | Auth + Navigation | Users, Login, Role, MainWindow, phân quyền |
| Member 2 | Table/Session/QR | DiningTables, TableSessions, TableSessionCustomers |
| Member 3 | Menu/Stock | Categories, MenuItems, stock rule, Customer menu |
| Member 4 | Order/Print/Request | Orders, OrderItems, ServiceRequests, SignalR order/request |
| Member 5 | Bill/Payment/Dashboard | Bills, BillDetails, Payments, split bill, revenue |

Mỗi member làm theo feature dọc:

```txt
BusinessObjects
→ DataAccessObjects
→ Repositories
→ Services
→ Api nếu cần
→ WPFApp nếu staff/admin dùng
→ CustomerWeb nếu khách dùng
→ báo DB owner tạo migration nếu đổi schema
```

---

## 18. Checklist trước khi push feature

```txt
[ ] Pull code mới nhất từ develop.
[ ] Code đúng folder feature của mình.
[ ] Không gọi DbContext trong WPF.
[ ] Không gọi DAO trong WPF.
[ ] Không viết SQL trong WPF/Service.
[ ] Service có validate business rule.
[ ] Repository có interface.
[ ] DAO chỉ truy vấn database.
[ ] Nếu đổi Entity đã báo DB owner.
[ ] Project build không lỗi.
[ ] API liên quan chạy được nếu có.
[ ] WPF màn liên quan chạy được nếu có.
[ ] CustomerWeb page liên quan chạy được nếu có.
[ ] Không commit appsettings.Development.json thật.
[ ] Không commit .env thật.
[ ] Không commit bin/obj/node_modules.
```

---

## 19. Tóm tắt quy tắc nhớ nhanh

```txt
BusinessObjects:
    Chỉ chứa Entity/DTO/Enum.

DataAccessObjects:
    Chứa AppDbContext, Migration, DAO.

Repositories:
    Interface + implementation, gọi DAO.

Services:
    Business rule, validate, transaction nghiệp vụ.

Api:
    HTTP endpoint + SignalR, gọi Service.

WPFApp:
    UI cho nhân viên/admin, chỉ gọi Service.

CustomerWeb:
    UI cho khách, chỉ gọi Api.

database:
    Seed/reset/manual script, không phải nguồn chính của schema.

Migration:
    Chỉ DB owner tạo.
```

---

## 20. Kết luận

Cấu trúc DineFlow hiện tại phù hợp để nhóm làm song song vì đã tách rõ:

```txt
Desktop app cho staff/admin
Backend API
Customer Web QR
Business layer
Data access layer
Migration/database schema
Docs/rule làm việc nhóm
```

Khi làm đúng rule, mỗi member có thể tự phát triển module của mình mà không phá module khác. Điều quan trọng nhất là luôn giữ đúng luồng:

```txt
WPFApp → Services → Repositories → DAO → AppDbContext → SQL Server
CustomerWeb → Api → Services → Repositories → DAO → AppDbContext → SQL Server
```

Không đi tắt qua database từ UI.
