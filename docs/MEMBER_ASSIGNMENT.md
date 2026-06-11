# Member Assignment

| Member | Feature | BusinessObjects | DAO | Repository | Service | UI/API/Web |
|---|---|---|---|---|---|---|
| Member 1 | Auth + Navigation | User, Role, DTO | UserDAO | UserRepository | AuthService | LoginWindow, AuthController |
| Member 2 | Table/Session/QR | DiningTable, TableSession, TableSessionCustomer | TableDAO, SessionDAO | TableRepository | TableService | TableSessionWindow, Customer table API |
| Member 3 | Menu/Stock | Category, MenuItem | CategoryDAO, MenuItemDAO | CategoryRepository, MenuItemRepository | CategoryService, MenuItemService | MenuItemWindow, Customer menu web |
| Member 4 | Order/Print/Request | Order, OrderItem, ServiceRequest | OrderDAO, RequestDAO | OrderRepository, RequestRepository | OrderService, RequestService | OrderWindow, StaffHub events |
| Member 5 | Bill/Payment/Dashboard | Bill, BillDetail, Payment, DashboardDto | BillDAO, PaymentDAO, ReportDAO | BillRepository, ReportRepository | BillService, ReportService | BillWindow, DashboardWindow |

## File dùng chung cần owner

| File/folder | Owner đề xuất |
|---|---|
| AppDbContext.cs | DB owner |
| Migrations/ | DB owner |
| MainWindow.xaml | Navigation owner |
| Program.cs API | API owner |
| CustomerWeb routes | Web owner |
| docs/API_CONTRACT.md | API owner |
| docs/SIGNALR_CONTRACT.md | API/realtime owner |
