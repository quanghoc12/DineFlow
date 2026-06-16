# SRS

**\# SRS — QR Food Ordering System After Reassigning Member 4 and Member 5 Scope**

**\*\*Project:\*\*** QR Food Ordering Management System    
**\*\*Document type:\*\*** Common SRS after scope reassignment    
**\*\*Version:\*\*** 2.0    
**\*\*Main change:\*\*** Bill, payment and split bill are moved to Member 4\. Member 5 only handles reporting, revenue statistics, top-selling item reports and Admin payment method correction.

\---

**\#\# 1\. Purpose**

This document updates the system SRS after changing the responsibility of Member 4 and Member 5\.

The goal is to make the team boundary clearer:

\`\`\`text  
Member 4 owns the operational transaction flow.  
Member 5 owns report/dashboard/statistics flow.  
\`\`\`

\---

**\#\# 2\. Updated Module Split**

| Member | Module | Main responsibility |  
|---|---|---|  
| Member 1 | Auth / User / Permission | Login, role, permission, current user |  
| Member 2 | Table / QR | Dining table master data, QR token, table status contract |  
| Member 3 | Menu / Category / Stock / Add-on | Category, menu item, stock, add-on configuration |  
| Member 4 | Session / Order / Request / Bill / Payment | Full operation flow from order to payment |  
| Member 5 | Dashboard / Report / Revenue | Statistics, top selling items, revenue, Admin payment method correction |

\---

**\#\# 3\. System Flow After Reassignment**

\`\`\`text  
Customer scans QR  
→ Member 2 validates QR and table  
→ Customer views menu from Member 3  
→ Customer sends order to Member 4  
→ Member 4 creates/gets session  
→ Member 4 creates order and order items  
→ Member 4 coordinates stock reservation with Member 3  
→ Member 4 creates/updates default bill  
→ WPF receives print request  
→ Customer requests payment  
→ Member 4 handles payment request and bill screen  
→ Staff splits bill if needed  
→ Staff confirms payment  
→ Member 4 closes session if all bills are completed  
→ Member 5 reads paid data for dashboard and reports  
\`\`\`

\---

**\# 4\. Member 4 Updated Scope**

**\#\# 4.1. Member 4 owns**

\`\`\`text  
TableSessions  
TableSessionCustomers  
Orders  
OrderItems  
ServiceRequests  
Order print status  
Bills  
BillDetails  
Payments  
Split bill  
Payment confirmation  
Session close after payment  
Realtime operation events  
\`\`\`

**\#\# 4.2. Member 4 screens**

\`\`\`text  
TableSessionDetailView  
OrderManagementView  
OrderDetailView  
PrintQueueView  
ServiceRequestView  
PaymentRequestView  
BillView  
BillDetailView  
SplitBillView  
PaymentConfirmView  
\`\`\`

**\#\# 4.3. Member 4 database tables**

\`\`\`text  
TableSessions  
TableSessionCustomers  
Orders  
OrderItems  
ServiceRequests  
Bills  
BillDetails  
Payments  
\`\`\`

**\#\# 4.4. Member 4 key rules**

\`\`\`text  
One table has at most one active session.  
Order is auto accepted if valid.  
OrderItems save snapshot data.  
Add-ons are saved as OrderItems with ParentOrderItemId.  
Order accepted must be added to default bill.  
Bill Paid is locked.  
Only Unpaid bill can be split or paid.  
Payment amount equals Bill.FinalAmount.  
Session closes only when no Unpaid bill remains.  
\`\`\`

\---

**\# 5\. Member 5 Updated Scope**

**\#\# 5.1. Member 5 owns**

\`\`\`text  
Dashboard  
Revenue report  
Top selling item report  
Revenue by payment method  
Paid bill history read-only  
Payment correction history  
Admin payment method correction  
\`\`\`

**\#\# 5.2. Member 5 screens**

\`\`\`text  
DashboardView  
RevenueReportView  
TopSellingItemsReportView  
RevenueByPaymentMethodView  
PaidBillHistoryView  
PaymentCorrectionView  
\`\`\`

**\#\# 5.3. Member 5 does not own**

\`\`\`text  
Bill creation  
BillDetails creation  
Split bill  
Payment confirmation  
Order creation/cancel  
Session close  
Stock reservation  
Print order  
\`\`\`

**\#\# 5.4. Member 5 write permission**

Member 5 can update only:

\`\`\`text  
Payments.PaymentMethod  
Payments.UpdatedAt  
Payments.UpdatedBy  
Payments.ChangeReason  
\`\`\`

Only when:

\`\`\`text  
Current user is Admin.  
Bill is Paid.  
Payment exists.  
ChangeReason is provided.  
\`\`\`

\---

**\# 6\. Updated Functional Requirements**

**\#\# FR-COMMON-001: Customer order flow**

Owner: Member 4    
Integrated with: Member 2, Member 3

Rules:

\`\`\`text  
Customer Web sends tableToken, clientToken and items.  
System validates QR and table.  
System validates menu item and stock.  
System creates session, order and order items.  
System adds items to default bill.  
System sends realtime print event.  
\`\`\`

\---

**\#\# FR-COMMON-002: Add-on order flow**

Owner: Member 3 and Member 4

Rules:

\`\`\`text  
Member 3 defines which add-ons belong to which main dish.  
Member 4 validates selected add-ons through Member 3 contract.  
Add-ons are saved as OrderItems linked to main OrderItem.  
Add-ons are included in bill and payment.  
\`\`\`

\---

**\#\# FR-COMMON-003: Bill and split bill flow**

Owner: Member 4

Rules:

\`\`\`text  
Each active session has default bill.  
New order items go into default bill.  
Staff can split bill by moving item quantity.  
Only Unpaid bills can be split.  
Paid bill is locked.  
\`\`\`

\---

**\#\# FR-COMMON-004: Payment flow**

Owner: Member 4

Rules:

\`\`\`text  
Staff selects Unpaid bill.  
Staff selects payment method.  
System creates Payment.  
System updates Bill.Status \= Paid.  
Payment.Amount \= Bill.FinalAmount.  
Session closes if all active bills are Paid or Cancelled.  
\`\`\`

\---

**\#\# FR-COMMON-005: Report flow**

Owner: Member 5

Rules:

\`\`\`text  
Dashboard reads paid bills.  
Revenue is based on Bills.Status \= Paid.  
Revenue date is based on Bills.PaidAt.  
Top selling items are based on BillDetails joined with Paid Bills.  
Reports must not modify operational data.  
\`\`\`

\---

**\#\# FR-COMMON-006: Admin payment method correction**

Owner: Member 5

Rules:

\`\`\`text  
Only Admin can correct payment method after paid.  
Correction requires reason.  
Only PaymentMethod and correction audit fields can be updated.  
Amount and bill total must not change.  
\`\`\`

\---

**\# 7\. Updated Database Ownership**

| Table | Owner | Notes |  
|---|---|---|  
| \`Users\` | Member 1 | Auth and audit user |  
| \`DiningTables\` | Member 2 | Table and QR master data |  
| \`Categories\` | Member 3 | Menu grouping |  
| \`MenuItems\` | Member 3 | Main items and add-on items |  
| \`MenuAddonGroups\` | Member 3 | Add-on groups for main dishes |  
| \`MenuAddonOptions\` | Member 3 | Add-on choices |  
| \`TableSessions\` | Member 4 | Active serving session |  
| \`TableSessionCustomers\` | Member 4 | Customer device in session |  
| \`Orders\` | Member 4 | Order header |  
| \`OrderItems\` | Member 4 | Main and add-on order item snapshots |  
| \`ServiceRequests\` | Member 4 | Call staff/payment request |  
| \`Bills\` | Member 4 | Bill header |  
| \`BillDetails\` | Member 4 | Bill item snapshots |  
| \`Payments\` | Member 4 core / Member 5 correction | Member 4 creates; Member 5 corrects method only |

\---

**\# 8\. Integration Contracts**

**\#\# 8.1. Member 4 uses Member 1**

\`\`\`text  
GetCurrentUserId  
RequirePermission  
RequireAdmin if needed  
\`\`\`

**\#\# 8.2. Member 4 uses Member 2**

\`\`\`text  
ValidateQrToken  
EnsureTableCanServe  
SetTableOccupied  
SetTableWaitingPayment  
SetTableAvailable  
\`\`\`

**\#\# 8.3. Member 4 uses Member 3**

\`\`\`text  
GetMenuItemSnapshot  
ValidateOrderableItems  
ValidateAddonsForOrder  
ReserveStockForOrder  
RollbackStockForCancelledOrder  
\`\`\`

**\#\# 8.4. Member 5 uses Member 4 read/correction port**

\`\`\`text  
GetPaidBillsForReport  
GetBillDetailsForReport  
GetPaymentsForReport  
UpdatePaymentMethodByAdmin  
\`\`\`

\---

**\# 9\. Acceptance Criteria**

\`\`\`text  
\[ \] Scope between Member 4 and Member 5 is clear.  
\[ \] Member 4 can complete order to payment flow.  
\[ \] Member 4 owns bill, payment confirmation and split bill.  
\[ \] Member 5 dashboard reads paid data only.  
\[ \] Member 5 cannot edit bill details or payment amount.  
\[ \] Admin can correct payment method only with reason.  
\[ \] Mermaid ERD reflects updated ownership.  
\`\`\`

# Rule Based WPF

**\# Rule Frame Structure cho Project WPF App theo Feature**

\> Tài liệu này dùng làm rule chung cho nhóm khi làm project Windows App bằng **\*\*WPF \+ Entity Framework Core \+ SQL Server\*\*** theo template nhiều layer:    
\> **\*\*BusinessObjects → DataAccessObjects → Repositories → Services → WPFApp\*\***.

\---

**\#\# 1\. Mục tiêu của rule**

Project nhóm sẽ không chia theo kiểu:

\`\`\`txt  
Người A làm FE  
Người B làm BE  
Người C làm Database  
\`\`\`

Thay vào đó, nhóm sẽ chia theo **\*\*feature dọc\*\***.

Tức là mỗi người chịu trách nhiệm trọn một chức năng từ giao diện đến database:

\`\`\`txt  
WPF UI  
→ Service  
→ Repository  
→ DAO  
→ Entity / DTO  
→ Database table liên quan  
\`\`\`

Cách chia này giúp mỗi thành viên hiểu rõ toàn bộ flow xử lý của một chức năng, dễ demo, dễ debug, dễ chịu trách nhiệm và phù hợp với kiến trúc trong template.

\---

**\#\# 2\. Kiến trúc tổng thể của project**

Solution giữ nguyên cấu trúc nhiều project như template:

\`\`\`txt  
GroupWindowsApp.sln  
│  
├── BusinessObjects  
├── DataAccessObjects  
├── Repositories  
├── Services  
└── WPFApp  
\`\`\`

Luồng xử lý chuẩn:

\`\`\`txt  
WPFApp  
  ↓  
Services  
  ↓  
Repositories  
  ↓  
DataAccessObjects  
  ↓  
Entity Framework Core  
  ↓  
SQL Server  
\`\`\`

\---

**\#\# 3\. Rule dependency giữa các project**

\`\`\`txt  
BusinessObjects  
    Không reference project nào khác.

DataAccessObjects  
    Reference BusinessObjects.

Repositories  
    Reference BusinessObjects.  
    Reference DataAccessObjects.

Services  
    Reference BusinessObjects.  
    Reference Repositories.

WPFApp  
    Reference BusinessObjects.  
    Reference Services.  
\`\`\`

Rule bắt buộc:

\`\`\`txt  
WPFApp không được gọi trực tiếp DAO.  
WPFApp không được gọi trực tiếp DbContext.  
WPFApp không được viết SQL trực tiếp.  
Services không được hiển thị MessageBox.  
Repositories không xử lý UI.  
DAO không xử lý nghiệp vụ phức tạp.  
BusinessObjects chỉ chứa Entity, DTO, Enum.  
\`\`\`

\---

**\#\# 4\. Cách chia việc theo feature**

Mỗi người sẽ nhận một feature hoàn chỉnh.

Ví dụ:

\`\`\`txt  
Member 1: Auth/Login  
Member 2: Product Management  
Member 3: Category Management  
Member 4: Order/Transaction Management  
Member 5: Report \+ AI Assistant  
\`\`\`

Mỗi feature phải có đủ các phần:

\`\`\`txt  
BusinessObjects  
DataAccessObjects  
Repositories  
Services  
WPFApp  
Database  
\`\`\`

\---

**\#\# 5\. Cấu trúc file theo feature**

Ví dụ một người làm feature \`Product Management\`, người đó cần tạo hoặc phụ trách các file sau:

\`\`\`txt  
BusinessObjects  
└── Products  
    ├── Product.cs  
    └── ProductDto.cs

DataAccessObjects  
└── Products  
    └── ProductDAO.cs

Repositories  
└── Products  
    ├── IProductRepository.cs  
    └── ProductRepository.cs

Services  
└── Products  
    ├── IProductService.cs  
    └── ProductService.cs

WPFApp  
├── Views  
│   └── ProductWindow.xaml  
│  
└── ViewModels hoặc CodeBehind  
    └── ProductWindow.xaml.cs  
\`\`\`

\---

**\#\# 6\. Cấu trúc thư mục đề xuất**

**\#\#\# 6.1. BusinessObjects**

\`\`\`txt  
BusinessObjects  
├── Auth  
│   ├── User.cs  
│   └── Role.cs  
│  
├── Products  
│   ├── Product.cs  
│   └── Category.cs  
│  
├── Reports  
│   └── ReportDto.cs  
│  
└── AI  
    ├── AiRequestDto.cs  
    ├── AiResponseDto.cs  
    └── AiHistory.cs  
\`\`\`

**\#\#\# 6.2. DataAccessObjects**

\`\`\`txt  
DataAccessObjects  
├── DbContexts  
│   └── AppDbContext.cs  
│  
├── Auth  
│   └── UserDAO.cs  
│  
├── Products  
│   ├── ProductDAO.cs  
│   └── CategoryDAO.cs  
│  
├── Reports  
│   └── ReportDAO.cs  
│  
└── AI  
    └── AiHistoryDAO.cs  
\`\`\`

**\#\#\# 6.3. Repositories**

\`\`\`txt  
Repositories  
├── Auth  
│   ├── IUserRepository.cs  
│   └── UserRepository.cs  
│  
├── Products  
│   ├── IProductRepository.cs  
│   └── ProductRepository.cs  
│  
├── Reports  
│   ├── IReportRepository.cs  
│   └── ReportRepository.cs  
│  
└── AI  
    ├── IAiHistoryRepository.cs  
    └── AiHistoryRepository.cs  
\`\`\`

**\#\#\# 6.4. Services**

\`\`\`txt  
Services  
├── Auth  
│   ├── IAuthService.cs  
│   └── AuthService.cs  
│  
├── Products  
│   ├── IProductService.cs  
│   └── ProductService.cs  
│  
├── Reports  
│   ├── IReportService.cs  
│   └── ReportService.cs  
│  
└── AI  
    ├── IAiService.cs  
    ├── AiService.cs  
    ├── LocalAiProvider.cs  
    └── ApiAiProvider.cs  
\`\`\`

**\#\#\# 6.5. WPFApp**

\`\`\`txt  
WPFApp  
├── Views  
│   ├── LoginWindow.xaml  
│   ├── MainWindow.xaml  
│   ├── ProductWindow.xaml  
│   ├── CategoryWindow.xaml  
│   ├── ReportWindow.xaml  
│   └── AiAssistantWindow.xaml  
│  
├── ViewModels  
│   ├── LoginViewModel.cs  
│   ├── ProductViewModel.cs  
│   ├── CategoryViewModel.cs  
│   ├── ReportViewModel.cs  
│   └── AiAssistantViewModel.cs  
│  
├── Helpers  
│   ├── NavigationHelper.cs  
│   ├── MessageBoxHelper.cs  
│   └── ValidationHelper.cs  
│  
├── Resources  
│   ├── Styles.xaml  
│   └── Colors.xaml  
│  
├── appsettings.json  
└── App.xaml  
\`\`\`

\---

**\#\# 7\. Checklist bắt buộc cho mỗi feature**

Mỗi feature được xem là hoàn thành khi có đủ:

\`\`\`txt  
Database:  
\[ \] Tạo bảng liên quan.  
\[ \] Có khóa chính.  
\[ \] Có khóa ngoại nếu cần.  
\[ \] Có dữ liệu mẫu.

BusinessObjects:  
\[ \] Tạo Entity.  
\[ \] Tạo DTO nếu cần.  
\[ \] Tạo Enum nếu cần.

DataAccessObjects:  
\[ \] GetAll.  
\[ \] GetById.  
\[ \] Search / Find.  
\[ \] Add.  
\[ \] Update.  
\[ \] Delete.

Repositories:  
\[ \] Tạo Interface.  
\[ \] Tạo Implementation.  
\[ \] Repository gọi xuống DAO.  
\[ \] Không xử lý UI trong Repository.

Services:  
\[ \] Tạo Interface.  
\[ \] Tạo Implementation.  
\[ \] Validate dữ liệu.  
\[ \] Xử lý business rule.  
\[ \] Không hiển thị MessageBox.

WPFApp:  
\[ \] Có Window hoặc UserControl.  
\[ \] Có DataGrid nếu là màn CRUD.  
\[ \] Có form nhập liệu.  
\[ \] Có button Create.  
\[ \] Có button Update.  
\[ \] Có button Delete.  
\[ \] Có button Search nếu cần.  
\[ \] Có Clear Form.  
\[ \] Có Load Data.  
\[ \] Có SelectionChanged để đổ dữ liệu lên form.  
\[ \] Có try-catch khi gọi Service.

Test:  
\[ \] Build không lỗi.  
\[ \] Chạy được từ UI.  
\[ \] CRUD thành công.  
\[ \] Validate lỗi đúng.  
\[ \] Không crash khi input sai.  
\`\`\`

\---

**\#\# 8\. Rule cho BusinessObjects**

BusinessObjects chỉ chứa dữ liệu.

Ví dụ:

\`\`\`csharp  
namespace BusinessObjects.Products;

public class Product  
{  
    public int ProductId { get; set; }

    public string ProductName { get; set; } \= string.Empty;

    public int CategoryId { get; set; }

    public short? UnitsInStock { get; set; }

    public decimal? UnitPrice { get; set; }  
}  
\`\`\`

Rule:

\`\`\`txt  
Không query database trong BusinessObjects.  
Không dùng MessageBox trong BusinessObjects.  
Không viết logic UI trong BusinessObjects.  
Không gọi Service, Repository, DAO trong BusinessObjects.  
Entity phải map đúng với database.  
Tên class là danh từ số ít.  
\`\`\`

\---

**\#\# 9\. Rule cho DataAccessObjects**

DAO là nơi truy cập database thông qua Entity Framework Core.

Ví dụ:

\`\`\`csharp  
using BusinessObjects.Products;  
using DataAccessObjects.DbContexts;  
using System.Linq.Expressions;

namespace DataAccessObjects.Products;

public class ProductDAO  
{  
    public List\<Product\> GetAll()  
    {  
        using var db \= new AppDbContext();  
        return db.Products  
                 .OrderBy(p \=\> p.ProductId)  
                 .ToList();  
    }

    public Product? GetById(int id)  
    {  
        using var db \= new AppDbContext();  
        return db.Products.FirstOrDefault(p \=\> p.ProductId \== id);  
    }

    public List\<Product\> Find(Expression\<Func\<Product, bool\>\> predicate)  
    {  
        using var db \= new AppDbContext();  
        return db.Products.Where(predicate).ToList();  
    }

    public Product Add(Product product)  
    {  
        using var db \= new AppDbContext();  
        db.Products.Add(product);  
        db.SaveChanges();  
        return product;  
    }

    public void Update(Product product)  
    {  
        using var db \= new AppDbContext();  
        db.Products.Update(product);  
        db.SaveChanges();  
    }

    public void Delete(int id)  
    {  
        using var db \= new AppDbContext();  
        var entity \= db.Products.Find(id);

        if (entity \== null)  
        {  
            return;  
        }

        db.Products.Remove(entity);  
        db.SaveChanges();  
    }  
}  
\`\`\`

Rule:

\`\`\`txt  
Mỗi bảng chính nên có một DAO.  
DAO chỉ thao tác database.  
DAO không validate nghiệp vụ phức tạp.  
DAO không hiển thị MessageBox.  
DAO không gọi WPF.  
DAO không xử lý giao diện.  
\`\`\`

\---

**\#\# 10\. Rule cho Repositories**

Repository là lớp trung gian giữa Service và DAO.

Interface:

\`\`\`csharp  
using BusinessObjects.Products;

namespace Repositories.Products;

public interface IProductRepository  
{  
    List\<Product\> GetAll();

    Product? GetById(int id);

    List\<Product\> Search(string keyword);

    Product Add(Product product);

    void Update(Product product);

    void Delete(int id);  
}  
\`\`\`

Implementation:

\`\`\`csharp  
using BusinessObjects.Products;  
using DataAccessObjects.Products;

namespace Repositories.Products;

public class ProductRepository : IProductRepository  
{  
    private readonly ProductDAO \_productDAO;

    public ProductRepository()  
    {  
        \_productDAO \= new ProductDAO();  
    }

    public List\<Product\> GetAll()  
    {  
        return \_productDAO.GetAll();  
    }

    public Product? GetById(int id)  
    {  
        return \_productDAO.GetById(id);  
    }

    public List\<Product\> Search(string keyword)  
    {  
        return \_productDAO.Find(p \=\> p.ProductName.Contains(keyword));  
    }

    public Product Add(Product product)  
    {  
        return \_productDAO.Add(product);  
    }

    public void Update(Product product)  
    {  
        \_productDAO.Update(product);  
    }

    public void Delete(int id)  
    {  
        \_productDAO.Delete(id);  
    }  
}  
\`\`\`

Rule:

\`\`\`txt  
Mọi Repository phải có Interface.  
Service gọi Repository thông qua Interface.  
Repository gọi DAO.  
Repository không gọi WPF.  
Repository không hiển thị MessageBox.  
Repository không xử lý validation form.  
\`\`\`

\---

**\#\# 11\. Rule cho Services**

Service là nơi xử lý business logic và validate dữ liệu.

Interface:

\`\`\`csharp  
using BusinessObjects.Products;

namespace Services.Products;

public interface IProductService  
{  
    List\<Product\> GetAll();

    Product? GetById(int id);

    List\<Product\> Search(string keyword);

    Product Create(Product product);

    void Update(Product product);

    void Delete(int id);  
}  
\`\`\`

Implementation:

\`\`\`csharp  
using BusinessObjects.Products;  
using Repositories.Products;

namespace Services.Products;

public class ProductService : IProductService  
{  
    private readonly IProductRepository \_productRepository;

    public ProductService()  
    {  
        \_productRepository \= new ProductRepository();  
    }

    public List\<Product\> GetAll()  
    {  
        return \_productRepository.GetAll();  
    }

    public Product? GetById(int id)  
    {  
        if (id \<= 0)  
        {  
            throw new Exception("ID không hợp lệ.");  
        }

        return \_productRepository.GetById(id);  
    }

    public List\<Product\> Search(string keyword)  
    {  
        if (string.IsNullOrWhiteSpace(keyword))  
        {  
            return \_productRepository.GetAll();  
        }

        return \_productRepository.Search(keyword.Trim());  
    }

    public Product Create(Product product)  
    {  
        Validate(product);  
        return \_productRepository.Add(product);  
    }

    public void Update(Product product)  
    {  
        Validate(product);  
        \_productRepository.Update(product);  
    }

    public void Delete(int id)  
    {  
        if (id \<= 0)  
        {  
            throw new Exception("ID không hợp lệ.");  
        }

        \_productRepository.Delete(id);  
    }

    private void Validate(Product product)  
    {  
        if (string.IsNullOrWhiteSpace(product.ProductName))  
        {  
            throw new Exception("Tên sản phẩm không được để trống.");  
        }

        if (product.UnitPrice \< 0)  
        {  
            throw new Exception("Giá sản phẩm không được âm.");  
        }

        if (product.UnitsInStock \< 0)  
        {  
            throw new Exception("Số lượng tồn kho không được âm.");  
        }  
    }  
}  
\`\`\`

Rule:

\`\`\`txt  
Service xử lý nghiệp vụ chính.  
Service validate dữ liệu trước khi gọi Repository.  
Service không hiển thị MessageBox.  
Service không thao tác trực tiếp với XAML.  
Service không gọi trực tiếp DbContext nếu đã có Repository.  
Service có thể gọi nhiều Repository nếu nghiệp vụ cần nhiều bảng.  
\`\`\`

\---

**\#\# 12\. Rule cho WPFApp**

WPFApp là nơi làm giao diện và nhận thao tác người dùng.

Ví dụ code-behind:

\`\`\`csharp  
using BusinessObjects.Products;  
using Services.Products;  
using System.Windows;

namespace WPFApp.Views;

public partial class ProductWindow : Window  
{  
    private readonly IProductService \_productService;

    public ProductWindow()  
    {  
        InitializeComponent();  
        \_productService \= new ProductService();  
    }

    private void Window\_Loaded(object sender, RoutedEventArgs e)  
    {  
        LoadProducts();  
    }

    private void LoadProducts()  
    {  
        dgProducts.ItemsSource \= \_productService.GetAll();  
    }

    private void btnCreate\_Click(object sender, RoutedEventArgs e)  
    {  
        try  
        {  
            var product \= new Product  
            {  
                ProductName \= txtProductName.Text.Trim(),  
                UnitPrice \= decimal.Parse(txtPrice.Text),  
                UnitsInStock \= short.Parse(txtUnitsInStock.Text),  
                CategoryId \= int.Parse(cboCategory.SelectedValue.ToString()\!)  
            };

            \_productService.Create(product);

            MessageBox.Show("Tạo mới thành công.");  
            LoadProducts();  
            ClearForm();  
        }  
        catch (Exception ex)  
        {  
            MessageBox.Show(ex.Message);  
        }  
    }

    private void ClearForm()  
    {  
        txtProductName.Clear();  
        txtPrice.Clear();  
        txtUnitsInStock.Clear();  
        cboCategory.SelectedIndex \= \-1;  
    }  
}  
\`\`\`

Rule:

\`\`\`txt  
WPF chỉ gọi Service.  
WPF không gọi DAO.  
WPF không gọi DbContext.  
WPF không viết SQL.  
WPF được phép hiển thị MessageBox.  
WPF phải try-catch khi gọi Service.  
WPF phải validate format input cơ bản nếu cần.  
Code-behind không nên chứa business logic dài.  
\`\`\`

\---

**\#\# 13\. Rule đặt tên file**

\`\`\`txt  
Entity:  
Product.cs  
Category.cs  
User.cs

DAO:  
ProductDAO.cs  
CategoryDAO.cs  
UserDAO.cs

Repository Interface:  
IProductRepository.cs  
ICategoryRepository.cs  
IUserRepository.cs

Repository Implementation:  
ProductRepository.cs  
CategoryRepository.cs  
UserRepository.cs

Service Interface:  
IProductService.cs  
ICategoryService.cs  
IAuthService.cs

Service Implementation:  
ProductService.cs  
CategoryService.cs  
AuthService.cs

WPF Window:  
ProductWindow.xaml  
CategoryWindow.xaml  
LoginWindow.xaml  
\`\`\`

Rule:

\`\`\`txt  
Tên Entity là danh từ số ít.  
Tên bảng database là danh từ số nhiều.  
Tên DAO \= Entity \+ DAO.  
Tên Repository \= Entity \+ Repository.  
Tên Service \= Entity \+ Service.  
Tên Window \= Entity \+ Window.  
Tên Interface bắt đầu bằng chữ I.  
\`\`\`

\---

**\#\# 14\. Rule database**

\`\`\`txt  
Mỗi bảng phải có khóa chính.  
Tên bảng dùng số nhiều: Products, Users, Orders.  
Tên cột rõ nghĩa: ProductId, ProductName, UnitPrice.  
Không lưu password plain text.  
Cột tiền dùng decimal hoặc money.  
Cột ngày dùng datetime hoặc datetime2.  
Cột trạng thái nên dùng enum hoặc bảng lookup.  
\`\`\`

Ví dụ:

\`\`\`sql  
CREATE TABLE Products (  
    ProductId INT IDENTITY(1,1) PRIMARY KEY,  
    ProductName NVARCHAR(100) NOT NULL,  
    CategoryId INT NOT NULL,  
    UnitsInStock SMALLINT NULL,  
    UnitPrice DECIMAL(18, 2) NULL  
);  
\`\`\`

\---

**\#\# 15\. Rule appsettings.json**

Trong WPFApp nên có file \`appsettings.json\`.

\`\`\`json  
{  
  "ConnectionStrings": {  
    "DefaultConnection": "Server=localhost;Database=GroupWindowsAppDB;User Id=sa;Password=123456;TrustServerCertificate=True"  
  },  
  "AiSettings": {  
    "Provider": "Local",  
    "LocalEndpoint": "http://localhost:11434",  
    "ApiEndpoint": "",  
    "ApiKey": ""  
  }  
}  
\`\`\`

Rule:

\`\`\`txt  
Không hard-code connection string ở nhiều nơi.  
Không commit API key thật lên GitHub.  
Không commit password thật của môi trường production.  
appsettings.json phải set Copy if newer.  
Có thể tạo appsettings.Development.json cho từng máy.  
\`\`\`

\---

**\#\# 16\. Rule tích hợp AI Local hoặc AI API**

AI nên được tách thành một feature riêng.

Cấu trúc:

\`\`\`txt  
Services  
└── AI  
    ├── IAiService.cs  
    ├── IAiProvider.cs  
    ├── AiService.cs  
    ├── LocalAiProvider.cs  
    └── ApiAiProvider.cs  
\`\`\`

Interface:

\`\`\`csharp  
namespace Services.AI;

public interface IAiService  
{  
    Task\<string\> AskAsync(string prompt);

    Task\<string\> AnalyzeDataAsync(string data);

    Task\<string\> SuggestActionAsync(string userInput);  
}  
\`\`\`

Provider:

\`\`\`csharp  
namespace Services.AI;

public interface IAiProvider  
{  
    Task\<string\> GenerateAsync(string prompt);  
}  
\`\`\`

Rule AI:

\`\`\`txt  
WPF không gọi trực tiếp AI API.  
WPF chỉ gọi IAiService.  
AI không tự ghi database nếu chưa có xác nhận từ người dùng.  
Không gửi password, connection string, API key vào prompt.  
Nếu dùng AI API, chỉ gửi dữ liệu thật sự cần thiết.  
Nếu dùng AI local, vẫn cần giới hạn dữ liệu nhạy cảm.  
Các feature khác muốn dùng AI phải gọi qua IAiService.  
\`\`\`

Ví dụ use case AI hợp lý:

\`\`\`txt  
AI gợi ý mô tả dữ liệu.  
AI phân tích báo cáo.  
AI tóm tắt danh sách dữ liệu.  
AI hỗ trợ tìm kiếm bằng ngôn ngữ tự nhiên.  
AI sinh nội dung ghi chú.  
AI cảnh báo dữ liệu bất thường.  
AI chatbot hướng dẫn sử dụng app.  
\`\`\`

\---

**\#\# 17\. Rule chia việc nhóm**

**\#\#\# Nhóm 4 người**

| Thành viên | Feature | Phạm vi |  
|---|---|---|  
| Member 1 | Auth/Login \+ MainWindow | User, Role, Login, phân quyền, điều hướng |  
| Member 2 | Feature CRUD chính 1 | CRUD đầy đủ cho bảng chính số 1 |  
| Member 3 | Feature CRUD chính 2 | CRUD đầy đủ cho bảng chính số 2 |  
| Member 4 | Report/Dashboard \+ AI | Thống kê, báo cáo, AI local/API |

**\#\#\# Nhóm 5 người**

| Thành viên | Feature | Phạm vi |  
|---|---|---|  
| Member 1 | Auth/Login | Login, User, Role, Session |  
| Member 2 | Master Data 1 | Một module CRUD lớn |  
| Member 3 | Master Data 2 | Một module CRUD lớn |  
| Member 4 | Transaction Feature | Nghiệp vụ nhiều bảng |  
| Member 5 | AI \+ Report | AI Assistant, thống kê, báo cáo |

\---

**\#\# 18\. Rule tránh conflict khi làm Git**

**\#\#\# 18.1. Mỗi người chỉ sửa file feature của mình**

Ví dụ người làm \`Product\` chỉ sửa:

\`\`\`txt  
Product.cs  
ProductDAO.cs  
IProductRepository.cs  
ProductRepository.cs  
IProductService.cs  
ProductService.cs  
ProductWindow.xaml  
ProductWindow.xaml.cs  
\`\`\`

Không tự ý sửa file dùng chung:

\`\`\`txt  
AppDbContext.cs  
App.xaml  
MainWindow.xaml  
appsettings.json  
DatabaseScript.sql  
NavigationHelper.cs  
\`\`\`

\---

**\#\#\# 18.2. File dùng chung phải có owner**

Các file dễ conflict:

\`\`\`txt  
AppDbContext.cs  
App.xaml  
MainWindow.xaml  
appsettings.json  
DatabaseScript.sql  
NavigationHelper.cs  
Global.cs  
\`\`\`

Nên giao owner:

\`\`\`txt  
Member 1 quản lý App.xaml \+ Login \+ Navigation.  
Member 2 quản lý DatabaseScript.sql.  
Member 3 quản lý AppDbContext.cs.  
Member 4 quản lý MainWindow menu.  
\`\`\`

\---

**\#\#\# 18.3. Branch theo feature**

\`\`\`txt  
main  
develop  
feature/auth-login  
feature/product-management  
feature/category-management  
feature/order-management  
feature/report-dashboard  
feature/ai-assistant  
\`\`\`

Không code trực tiếp trên \`main\`.

Luồng làm việc:

\`\`\`bash  
git checkout develop  
git pull  
git checkout \-b feature/product-management

\# Code feature

git add .  
git commit \-m "feat: add product management feature"  
git push origin feature/product-management  
\`\`\`

Sau đó tạo pull request vào \`develop\`.

\---

**\#\# 19\. Rule commit message**

\`\`\`txt  
feat: thêm chức năng mới  
fix: sửa lỗi  
refactor: cải thiện code không đổi logic  
ui: chỉnh giao diện  
docs: cập nhật tài liệu  
test: thêm hoặc sửa test  
chore: cấu hình project  
\`\`\`

Ví dụ:

\`\`\`txt  
feat: add product management feature  
fix: validate product unit price  
refactor: move product query to DAO  
ui: update product window layout  
docs: update project rule document  
\`\`\`

\---

**\#\# 20\. Rule pull request**

Trước khi tạo pull request:

\`\`\`txt  
\[ \] Đã pull code mới nhất từ develop.  
\[ \] Project build không lỗi.  
\[ \] Feature chạy được từ UI.  
\[ \] Không sửa file của người khác nếu chưa thống nhất.  
\[ \] Không commit bin/obj.  
\[ \] Không commit API key thật.  
\[ \] Không commit connection string production.  
\[ \] Không để code test tạm hoặc comment rác.  
\`\`\`

Không merge nếu:

\`\`\`txt  
Code không build được.  
WPF gọi trực tiếp DbContext.  
WPF gọi trực tiếp DAO.  
Service hiển thị MessageBox.  
DAO chứa business logic phức tạp.  
Có conflict chưa xử lý.  
Có connection string cá nhân gây lỗi máy người khác.  
\`\`\`

\---

**\#\# 21\. Mẫu bảng phân công feature**

| Feature | Owner | BusinessObjects | DAO | Repository | Service | WPF | DB |  
|---|---|---|---|---|---|---|---|  
| Auth/Login | Member 1 | User, Role | UserDAO | UserRepository | AuthService | LoginWindow | Users, Roles |  
| Master Data 1 | Member 2 | Entity A | EntityADAO | EntityARepository | EntityAService | EntityAWindow | Table A |  
| Master Data 2 | Member 3 | Entity B | EntityBDAO | EntityBRepository | EntityBService | EntityBWindow | Table B |  
| Transaction | Member 4 | Entity C/D | EntityCDAO | EntityCRepository | EntityCService | EntityCWindow | Table C/D |  
| AI/Report | Member 5 | ReportDto, AiDto | ReportDAO | ReportRepository | ReportService, AiService | ReportWindow, AiWindow | AiHistories |

\---

**\#\# 22\. Definition of Done cho mỗi feature**

Một feature chỉ được tính là xong khi:

\`\`\`txt  
\[ \] Có UI chạy được.  
\[ \] Có Service xử lý nghiệp vụ.  
\[ \] Có Repository.  
\[ \] Có DAO.  
\[ \] Có Entity hoặc DTO.  
\[ \] Có bảng database hoặc dùng bảng đã thống nhất.  
\[ \] Có dữ liệu mẫu để test.  
\[ \] Có validate dữ liệu.  
\[ \] Có xử lý lỗi.  
\[ \] Có demo được từ UI xuống database.  
\[ \] Không phá feature của người khác.  
\[ \] Code đã push lên branch feature.  
\[ \] Đã tạo pull request vào develop.  
\`\`\`

\---

**\#\# 23\. Kết luận**

Rule chính của project:

\`\`\`txt  
Mỗi người làm một feature hoàn chỉnh.

Mỗi feature phải đi đủ flow:  
WPF UI → Service → Repository → DAO → EF Core → SQL Server.

Không chia FE/BE ngang.

Không để WPF gọi trực tiếp database.

Không để Service xử lý giao diện.

Không để DAO xử lý nghiệp vụ.

AI là một feature riêng và các feature khác muốn dùng AI phải gọi qua IAiService.  
\`\`\`

Cách chia này giúp project nhóm dễ quản lý, dễ demo, đúng kiến trúc template và có thể mở rộng thêm AI local hoặc AI API mà không phá cấu trúc ban đầu.

# Quang

**\# Member 1 \- Auth, User, Permission & App Shell**

**\*\*Project:\*\*** QR Food Ordering Management System    
**\*\*Document type:\*\*** Business Rules & Contracts    
**\*\*Owner:\*\*** Member 1    
**\*\*Module:\*\*** Auth / User / Permission / App Shell    
**\*\*Target app:\*\*** WPF Staff/Admin App    
**\*\*Architecture rule:\*\*** Feature-based WPF project    
**\*\*Version:\*\*** 1.0  

\---

**\#\# 1\. Mục tiêu module**

Member 1 chịu trách nhiệm xây dựng nền tảng đăng nhập, phân quyền và điều hướng chính của WPF Staff App.

Module này phải đảm bảo:

\`\`\`text  
Admin / Staff đăng nhập được vào WPF App.  
User inactive không đăng nhập được.  
Password không lưu plain text.  
Sau đăng nhập, hệ thống biết user hiện tại là ai.  
WPF hiển thị menu theo role.  
Service vẫn kiểm tra quyền trước nghiệp vụ nhạy cảm.  
Các module khác lấy được CurrentUserId để lưu audit.  
\`\`\`

Member 1 không chỉ làm màn Login mà còn là người cung cấp **\*\*permission contract\*\*** cho toàn bộ nhóm.

\---

**\#\# 2\. Vị trí module trong kiến trúc WPF**

Theo rule project, mọi feature đi theo flow:

\`\`\`text  
WPF UI  
    ↓  
Service  
    ↓  
Repository  
    ↓  
DAO  
    ↓  
Entity Framework Core  
    ↓  
SQL Server  
\`\`\`

Với module Member 1:

\`\`\`text  
LoginWindow / MainWindow / UserManagementWindow  
    ↓  
AuthService / UserService / PermissionService / CurrentUserService  
    ↓  
UserRepository  
    ↓  
UserDAO  
    ↓  
Users table  
\`\`\`

**\#\#\# Layer rule bắt buộc**

\`\`\`text  
WPF không gọi UserDAO trực tiếp.  
WPF không gọi AppDbContext trực tiếp.  
WPF không viết SQL trực tiếp.  
Service không hiển thị MessageBox.  
Repository không xử lý UI.  
DAO không xử lý business rule phức tạp.  
BusinessObjects chỉ chứa Entity, DTO, Enum/Constants.  
\`\`\`

\---

**\#\# 3\. Phạm vi phụ trách**

**\#\# 3.1. Member 1 phụ trách**

\`\`\`text  
Users table  
User entity  
Role constants  
Permission constants  
Login DTO  
User management DTO  
AuthService contract  
UserService contract  
PermissionService contract  
CurrentUserService contract  
LoginWindow  
MainWindow  
UserManagementWindow  
Navigation shell  
Common helper contract  
Current user context cho các module khác  
\`\`\`

**\#\# 3.2. Member 1 không phụ trách**

\`\`\`text  
Không xử lý Table / QR logic.  
Không xử lý TableSession lifecycle.  
Không xử lý Menu / Stock logic.  
Không xử lý Order / Print logic.  
Không xử lý Bill / Payment logic.  
Không xử lý Dashboard calculation.  
Không tự sửa file module người khác nếu chưa thống nhất.  
\`\`\`

\---

**\#\# 4\. Folder / file contract**

**\#\# 4.1. BusinessObjects**

\`\`\`text  
FoodOrder.BusinessObjects  
└── Auth  
    ├── User.cs  
    ├── UserDto.cs  
    ├── LoginRequestDto.cs  
    ├── LoginResultDto.cs  
    ├── CreateUserRequest.cs  
    ├── UpdateUserRequest.cs  
    ├── ChangePasswordRequest.cs  
    ├── ResetPasswordRequest.cs  
    ├── UserRole.cs  
    └── PermissionKey.cs  
\`\`\`

**\#\# 4.2. DataAccessObjects**

\`\`\`text  
FoodOrder.DataAccessObjects  
└── Auth  
    └── UserDAO.cs  
\`\`\`

**\#\# 4.3. Repositories**

\`\`\`text  
FoodOrder.Repositories  
└── Auth  
    ├── IUserRepository.cs  
    └── UserRepository.cs  
\`\`\`

**\#\# 4.4. Services**

\`\`\`text  
FoodOrder.Services  
└── Auth  
    ├── IAuthService.cs  
    ├── AuthService.cs  
    ├── IUserService.cs  
    ├── UserService.cs  
    ├── ICurrentUserService.cs  
    ├── CurrentUserService.cs  
    ├── IPermissionService.cs  
    ├── PermissionService.cs  
    └── IPasswordHasher.cs  
\`\`\`

**\#\# 4.5. WPF Staff**

\`\`\`text  
FoodOrder.WpfStaff  
├── Views  
│   ├── LoginWindow.xaml  
│   ├── MainWindow.xaml  
│   └── UserManagementWindow.xaml  
│  
├── ViewModels  
│   ├── LoginViewModel.cs  
│   ├── MainViewModel.cs  
│   └── UserManagementViewModel.cs  
│  
├── Helpers  
│   ├── NavigationHelper.cs  
│   ├── MessageBoxHelper.cs  
│   └── ValidationHelper.cs  
│  
└── Common  
    └── CurrentUserContext.cs  
\`\`\`

\---

**\#\# 5\. Database contract \- Users**

**\#\# 5.1. Table: \`Users\`**

| Field | Type đề xuất | Required | Rule |  
|---|---|---:|---|  
| \`UserId\` | int identity | Yes | Primary key |  
| \`Username\` | nvarchar(50) | Yes | Unique, dùng để đăng nhập |  
| \`PasswordHash\` | nvarchar(255) | Yes | Lưu password đã hash |  
| \`FullName\` | nvarchar(100) | Yes | Tên hiển thị nhân viên/admin |  
| \`Role\` | nvarchar(20) | Yes | Chỉ nhận \`Admin\`, \`Staff\` |  
| \`IsActive\` | bit | Yes | \`true\` mới được đăng nhập |  
| \`CreatedAt\` | datetime2 | Yes | Thời điểm tạo user |  
| \`UpdatedAt\` | datetime2 nullable | No | Thời điểm cập nhật gần nhất |

**\#\# 5.2. Key / constraint**

\`\`\`text  
PK: UserId  
UNIQUE: Username  
CHECK: Role IN ('Admin', 'Staff')  
\`\`\`

**\#\# 5.3. User relation / audit usage**

\`Users\` được các module khác dùng để lưu audit:

\`\`\`text  
TableSessions.OpenedBy  
TableSessions.ClosedBy  
Orders.CreatedBy  
Orders.CancelledBy  
ServiceRequests.ConfirmedBy  
ServiceRequests.CompletedBy  
Bills.CreatedBy  
Bills.CancelledBy  
Payments.ConfirmedBy  
Payments.UpdatedBy  
\`\`\`

Member 1 phải cung cấp \`CurrentUserId\` cho các module khác.

\---

**\#\# 6\. Role contract**

Hệ thống chỉ có 2 role trong MVP:

\`\`\`text  
Admin  
Staff  
\`\`\`

**\#\# 6.1. Admin**

Admin là người có quyền cấu hình dữ liệu gốc và thao tác nhạy cảm.

Admin được phép:

\`\`\`text  
Quản lý user.  
Tạo/sửa/ẩn bàn.  
Tạo/reset QR.  
Quản lý category.  
Thêm/sửa/ẩn món.  
Sửa giá món.  
Cập nhật stock.  
Bật/tắt món đang bán.  
Xem order.  
Cancel order.  
Xử lý gọi nhân viên.  
Xử lý yêu cầu thanh toán.  
Tách bill.  
Xác nhận thanh toán.  
Sửa payment method sau khi bill đã Paid.  
Xem dashboard doanh thu.  
\`\`\`

**\#\# 6.2. Staff**

Staff tập trung vào vận hành trong ngày.

Staff được phép:

\`\`\`text  
Đăng nhập.  
Xem bàn.  
Xem TableSession.  
Xem order.  
Xem chi tiết order.  
In lại order.  
Cancel order theo rule.  
Xử lý gọi nhân viên.  
Xử lý yêu cầu thanh toán.  
Cập nhật số lượng món.  
Bật/tắt món đang bán theo rule.  
Tách bill.  
Xác nhận thanh toán.  
Xem lịch sử bill.  
Xem dashboard nếu hệ thống cho phép.  
\`\`\`

Staff không được phép:

\`\`\`text  
Quản lý user.  
Tạo/sửa/ẩn bàn.  
Tạo/reset QR.  
Quản lý category.  
Thêm/sửa/ẩn món gốc.  
Sửa tên/giá/mô tả món.  
Sửa payment method sau khi bill đã Paid.  
\`\`\`

\---

**\#\# 7\. Permission matrix contract**

| Permission key | Admin | Staff | Ghi chú |  
|---|---:|---:|---|  
| \`Auth.Login\` | Yes | Yes | Đăng nhập WPF |  
| \`Users.View\` | Yes | No | Xem danh sách user |  
| \`Users.Create\` | Yes | No | Tạo user |  
| \`Users.Update\` | Yes | No | Sửa user |  
| \`Users.LockUnlock\` | Yes | No | Khóa / mở khóa user |  
| \`Users.ResetPassword\` | Yes | No | Reset password user khác |  
| \`Tables.View\` | Yes | Yes | Xem bàn |  
| \`Tables.Manage\` | Yes | No | Tạo/sửa/ẩn bàn |  
| \`Tables.ResetQr\` | Yes | No | Reset QR |  
| \`Categories.Manage\` | Yes | No | Quản lý category |  
| \`Menu.View\` | Yes | Yes | Xem món |  
| \`Menu.ManageMasterData\` | Yes | No | Thêm/sửa/ẩn món, sửa giá |  
| \`Menu.UpdateStock\` | Yes | Yes | Cập nhật số lượng món |  
| \`Menu.SetAvailability\` | Yes | Yes | Bật/tắt món đang bán |  
| \`Orders.View\` | Yes | Yes | Xem order |  
| \`Orders.Cancel\` | Yes | Yes | Cancel order theo rule |  
| \`Orders.Reprint\` | Yes | Yes | In lại order |  
| \`Requests.Handle\` | Yes | Yes | Xử lý gọi nhân viên / thanh toán |  
| \`Bills.View\` | Yes | Yes | Xem bill |  
| \`Bills.Split\` | Yes | Yes | Tách bill |  
| \`Payments.Confirm\` | Yes | Yes | Xác nhận thanh toán |  
| \`Payments.UpdatePaidMethod\` | Yes | No | Sửa method sau khi Paid |  
| \`Dashboard.View\` | Yes | Yes | Xem dashboard |

Rule quan trọng:

\`\`\`text  
Ẩn button ở UI là chưa đủ.  
Service phải kiểm tra permission lại trước khi thực hiện nghiệp vụ.  
\`\`\`

\---

**\#\# 8\. Business rules \- Auth**

**\#\# BR-AUTH-001: User đăng nhập bằng Username và Password**

Input bắt buộc:

\`\`\`text  
Username  
Password  
\`\`\`

Không cho đăng nhập bằng:

\`\`\`text  
FullName  
UserId  
Role  
\`\`\`

**\#\# BR-AUTH-002: Username bắt buộc tồn tại**

Nếu username không tồn tại, hệ thống trả lỗi chung:

\`\`\`text  
Sai tài khoản hoặc mật khẩu.  
\`\`\`

Không trả lỗi quá cụ thể kiểu:

\`\`\`text  
Username không tồn tại.  
\`\`\`

Lý do: tránh lộ thông tin tài khoản.

**\#\# BR-AUTH-003: Password phải được kiểm tra bằng hash**

\`\`\`text  
Password input \+ PasswordHash trong database → verify  
\`\`\`

Không được:

\`\`\`text  
So sánh password plain text.  
Lưu password plain text.  
Log password ra console/file.  
Hiển thị password trong UI.  
\`\`\`

**\#\# BR-AUTH-004: User inactive không được đăng nhập**

Nếu:

\`\`\`text  
Users.IsActive \= false  
\`\`\`

Thì login phải thất bại.

Message đề xuất:

\`\`\`text  
Tài khoản đã bị khóa hoặc không còn hoạt động.  
\`\`\`

**\#\# BR-AUTH-005: Sau login phải lưu CurrentUser**

Sau khi đăng nhập thành công, hệ thống phải lưu:

\`\`\`text  
UserId  
Username  
FullName  
Role  
LoginAt  
\`\`\`

Dữ liệu này dùng cho:

\`\`\`text  
Hiển thị tên user trên MainWindow.  
Ẩn/hiện menu.  
Kiểm tra quyền ở Service.  
Lưu audit CreatedBy / UpdatedBy / CancelledBy / ConfirmedBy.  
\`\`\`

**\#\# BR-AUTH-006: Logout phải clear CurrentUser**

Khi logout:

\`\`\`text  
Clear CurrentUser  
Đóng MainWindow  
Mở lại LoginWindow  
Không cho quay lại màn cũ nếu chưa login  
\`\`\`

**\#\# BR-AUTH-007: Không cache password**

Sau login thành công, hệ thống không được giữ lại:

\`\`\`text  
Password plain text  
Password input  
PasswordBox content  
\`\`\`

\---

**\#\# 9\. Business rules \- User management**

**\#\# BR-USER-001: Chỉ Admin được quản lý user**

Các nghiệp vụ sau bắt buộc Admin:

\`\`\`text  
Xem danh sách user  
Tạo user  
Sửa user  
Khóa user  
Mở khóa user  
Reset password user khác  
\`\`\`

Staff cố gọi service phải nhận lỗi:

\`\`\`text  
Bạn không có quyền thực hiện chức năng này.  
\`\`\`

**\#\# BR-USER-002: Username unique**

Không được tạo 2 user cùng username.

Rule normalize:

\`\`\`text  
Trim username trước khi lưu.  
Nên so sánh username case-insensitive nếu DB collation không đảm bảo.  
\`\`\`

**\#\# BR-USER-003: Username không được sửa sau khi tạo**

Lý do:

\`\`\`text  
Username là định danh đăng nhập.  
Tránh ảnh hưởng lịch sử audit và thói quen user.  
\`\`\`

Nếu muốn đổi username, Admin tạo user mới và khóa user cũ.

**\#\# BR-USER-004: Role chỉ được nhận Admin hoặc Staff**

Không chấp nhận role ngoài:

\`\`\`text  
Manager  
Cashier  
Customer  
Owner  
\`\`\`

Trong MVP chỉ có:

\`\`\`text  
Admin  
Staff  
\`\`\`

**\#\# BR-USER-005: Không được xóa cứng user**

User đã xuất hiện trong audit không được xóa cứng.

Khi không còn dùng:

\`\`\`text  
IsActive \= false  
\`\`\`

**\#\# BR-USER-006: Không khóa chính mình nếu là Admin cuối cùng**

Nếu hệ thống chỉ còn một Admin active, không cho Admin đó tự khóa chính mình.

Message đề xuất:

\`\`\`text  
Không thể khóa Admin active cuối cùng của hệ thống.  
\`\`\`

**\#\# BR-USER-007: Tạo user mới phải hash password**

Khi tạo user:

\`\`\`text  
Input password → hash → lưu PasswordHash  
\`\`\`

Không lưu password input.

**\#\# BR-USER-008: Reset password phải hash password mới**

Khi reset password:

\`\`\`text  
newPassword → hash → update PasswordHash  
UpdatedAt \= now  
\`\`\`

**\#\# BR-USER-009: FullName bắt buộc**

FullName dùng để hiển thị trên WPF và audit readable.

Rule:

\`\`\`text  
Không rỗng.  
Trim trước khi lưu.  
Độ dài đề xuất: 1-100 ký tự.  
\`\`\`

**\#\# BR-USER-010: IsActive quyết định quyền login**

\`\`\`text  
IsActive \= true  → được phép login nếu password đúng.  
IsActive \= false → không được login.  
\`\`\`

\---

**\#\# 10\. Business rules \- Permission**

**\#\# BR-PERM-001: UI phải hiển thị theo role**

Sau khi login:

\`\`\`text  
Admin thấy đầy đủ menu.  
Staff chỉ thấy menu được phép.  
\`\`\`

**\#\# BR-PERM-002: Service phải kiểm tra quyền lại**

Không được chỉ dựa vào việc ẩn button.

Ví dụ:

\`\`\`text  
Staff không thấy nút Reset QR.  
Nhưng nếu Staff gọi thẳng TableService.ResetQr(), service vẫn phải chặn.  
\`\`\`

**\#\# BR-PERM-003: Permission check dùng PermissionKey thống nhất**

Không hard-code role rải rác kiểu:

\`\`\`text  
if (role \== "Admin")  
\`\`\`

ở nhiều màn.

Nên dùng contract:

\`\`\`text  
PermissionService.HasPermission(role, permissionKey)  
PermissionService.RequirePermission(permissionKey)  
\`\`\`

**\#\# BR-PERM-004: Admin mặc định có mọi quyền trong MVP**

\`\`\`text  
Role \= Admin → HasPermission \= true  
\`\`\`

Trừ khi sau này mở rộng permission chi tiết hơn.

**\#\# BR-PERM-005: Staff chỉ có quyền vận hành**

Staff không được cấu hình dữ liệu gốc nhạy cảm.

Nhạy cảm gồm:

\`\`\`text  
User management  
Table/QR management  
Category management  
Menu master data management  
Update paid payment method  
\`\`\`

**\#\# BR-PERM-006: Các module khác không tự định nghĩa role riêng**

Member 2/3/4/5 phải dùng PermissionKey do Member 1 cung cấp.

Không tạo file:

\`\`\`text  
TablePermission.cs riêng  
BillRole.cs riêng  
OrderRoleChecker.cs riêng  
\`\`\`

nếu logic trùng với PermissionService.

\---

**\#\# 11\. Business rules \- Current user / audit**

**\#\# BR-CURRENT-001: CurrentUser bắt buộc có sau login**

Các nghiệp vụ Staff/Admin trong WPF chỉ chạy khi có CurrentUser.

Nếu chưa login:

\`\`\`text  
Throw: Bạn cần đăng nhập để sử dụng chức năng này.  
\`\`\`

**\#\# BR-CURRENT-002: CurrentUser không được lấy từ UI text**

Không lấy user hiện tại bằng cách đọc:

\`\`\`text  
txtCurrentUser.Text  
lblRole.Content  
\`\`\`

Phải lấy từ:

\`\`\`text  
CurrentUserService  
CurrentUserContext  
\`\`\`

**\#\# BR-CURRENT-003: Module khác dùng CurrentUserId để lưu audit**

Ví dụ:

\`\`\`text  
Order.CancelledBy \= CurrentUserId  
Bill.CreatedBy \= CurrentUserId  
Payment.ConfirmedBy \= CurrentUserId  
ServiceRequest.CompletedBy \= CurrentUserId  
\`\`\`

**\#\# BR-CURRENT-004: Staff không được giả lập UserId từ input**

Các màn không cho user nhập:

\`\`\`text  
CreatedBy  
UpdatedBy  
CancelledBy  
ConfirmedBy  
\`\`\`

Những field này phải lấy từ CurrentUserService.

\---

**\#\# 12\. DTO / data contract**

**\#\# 12.1. \`LoginRequestDto\`**

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`Username\` | string | Yes | Trim, không rỗng |  
| \`Password\` | string | Yes | Không rỗng |

**\#\# 12.2. \`LoginResultDto\`**

| Field | Type | Required | Ghi chú |  
|---|---|---:|---|  
| \`UserId\` | int | Yes | Dùng audit |  
| \`Username\` | string | Yes | Tên đăng nhập |  
| \`FullName\` | string | Yes | Hiển thị trên UI |  
| \`Role\` | string | Yes | Admin/Staff |  
| \`LoginAt\` | datetime | Yes | Thời điểm login |  
| \`Permissions\` | List string | Optional | Có thể trả để UI binding menu |

**\#\# 12.3. \`UserDto\`**

| Field | Type | Required | Ghi chú |  
|---|---|---:|---|  
| \`UserId\` | int | Yes | ID user |  
| \`Username\` | string | Yes | Read-only sau khi tạo |  
| \`FullName\` | string | Yes | Có thể sửa |  
| \`Role\` | string | Yes | Admin/Staff |  
| \`IsActive\` | bool | Yes | Trạng thái tài khoản |  
| \`CreatedAt\` | datetime | Yes | Ngày tạo |  
| \`UpdatedAt\` | datetime? | No | Ngày sửa |

Không trả \`PasswordHash\` ra UI.

**\#\# 12.4. \`CreateUserRequest\`**

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`Username\` | string | Yes | Unique, trim, 3-50 ký tự |  
| \`Password\` | string | Yes | Tối thiểu 6 ký tự |  
| \`FullName\` | string | Yes | 1-100 ký tự |  
| \`Role\` | string | Yes | Admin/Staff |

**\#\# 12.5. \`UpdateUserRequest\`**

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`UserId\` | int | Yes | \> 0 |  
| \`FullName\` | string | Yes | 1-100 ký tự |  
| \`Role\` | string | Yes | Admin/Staff |  
| \`IsActive\` | bool | Yes | Không được khóa Admin cuối cùng |

Không cho sửa \`Username\` ở request này.

**\#\# 12.6. \`ChangePasswordRequest\`**

Dùng cho user tự đổi mật khẩu.

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`CurrentPassword\` | string | Yes | Phải verify đúng password hiện tại |  
| \`NewPassword\` | string | Yes | Tối thiểu 6 ký tự |  
| \`ConfirmPassword\` | string | Yes | Phải khớp NewPassword |

**\#\# 12.7. \`ResetPasswordRequest\`**

Dùng cho Admin reset password user khác.

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`UserId\` | int | Yes | User cần reset |  
| \`NewPassword\` | string | Yes | Tối thiểu 6 ký tự |  
| \`ConfirmPassword\` | string | Yes | Phải khớp NewPassword |

**\#\# 12.8. \`PermissionCheckRequest\`**

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`PermissionKey\` | string | Yes | Nằm trong danh sách PermissionKey |

**\#\# 12.9. \`PermissionCheckResult\`**

| Field | Type | Required | Ghi chú |  
|---|---|---:|---|  
| \`PermissionKey\` | string | Yes | Permission được check |  
| \`Allowed\` | bool | Yes | Có quyền hay không |  
| \`Reason\` | string | No | Lý do nếu bị từ chối |

\---

**\#\# 13\. Service contract**

**\#\# 13.1. \`IAuthService\`**

| Method | Input | Output | Permission | Error chính |  
|---|---|---|---|---|  
| \`Login\` | \`LoginRequestDto\` | \`LoginResultDto\` | Public WPF | Sai thông tin, inactive |  
| \`Logout\` | none | void | Logged in | Chưa login |  
| \`RequireLogin\` | none | void | Logged in | Chưa login |  
| \`RequireAdmin\` | none | void | Admin | Không có quyền |  
| \`ValidatePassword\` | username/password | bool | Internal | Sai thông tin |

**\#\# 13.2. \`IUserService\`**

| Method | Input | Output | Permission |  
|---|---|---|---|  
| \`GetAllUsers\` | filter optional | List \`UserDto\` | \`Users.View\` |  
| \`GetUserById\` | \`userId\` | \`UserDto\` | \`Users.View\` |  
| \`SearchUsers\` | keyword | List \`UserDto\` | \`Users.View\` |  
| \`CreateUser\` | \`CreateUserRequest\` | \`UserDto\` | \`Users.Create\` |  
| \`UpdateUser\` | \`UpdateUserRequest\` | \`UserDto\` | \`Users.Update\` |  
| \`LockUser\` | \`userId\` | void | \`Users.LockUnlock\` |  
| \`UnlockUser\` | \`userId\` | void | \`Users.LockUnlock\` |  
| \`ResetPassword\` | \`ResetPasswordRequest\` | void | \`Users.ResetPassword\` |  
| \`ChangeOwnPassword\` | \`ChangePasswordRequest\` | void | Logged in |

**\#\# 13.3. \`ICurrentUserService\`**

| Method | Input | Output | Ghi chú |  
|---|---|---|---|  
| \`SetCurrentUser\` | \`LoginResultDto\` | void | Gọi sau login |  
| \`GetCurrentUser\` | none | \`LoginResultDto\` | Throw nếu chưa login |  
| \`GetCurrentUserId\` | none | int | Dùng audit |  
| \`GetCurrentRole\` | none | string | Dùng permission |  
| \`IsLoggedIn\` | none | bool | Kiểm tra login |  
| \`IsAdmin\` | none | bool | Role check |  
| \`Clear\` | none | void | Gọi khi logout |

**\#\# 13.4. \`IPermissionService\`**

| Method | Input | Output | Ghi chú |  
|---|---|---|---|  
| \`HasPermission\` | role, permissionKey | bool | Check không throw |  
| \`RequirePermission\` | permissionKey | void | Throw nếu không có quyền |  
| \`GetPermissionsByRole\` | role | List string | Dùng MainWindow binding menu |  
| \`CanAccessMenu\` | menuKey | bool | Dùng WPF Shell |

**\#\# 13.5. \`IPasswordHasher\`**

| Method | Input | Output | Rule |  
|---|---|---|---|  
| \`Hash\` | plain password | password hash | Không trả plain password |  
| \`Verify\` | plain password, hash | bool | Dùng login/change password |

\---

**\#\# 14\. Repository contract**

**\#\# 14.1. \`IUserRepository\`**

| Method | Input | Output | Ghi chú |  
|---|---|---|---|  
| \`GetAll\` | none | List User | Không trả password cho UI, chỉ entity nội bộ |  
| \`GetById\` | userId | User? | Dùng service |  
| \`GetByUsername\` | username | User? | Dùng login |  
| \`Search\` | keyword | List User | Search username/fullname/role |  
| \`Add\` | User | User | Lưu user mới |  
| \`Update\` | User | void | Update user |  
| \`SetActive\` | userId, bool | void | Khóa/mở user |  
| \`IsUsernameExists\` | username | bool | Validate create |  
| \`CountActiveAdmins\` | none | int | Chống khóa Admin cuối cùng |

Repository không được:

\`\`\`text  
Hiển thị MessageBox.  
Gọi WPF.  
Validate form UI.  
Hash password nếu logic này thuộc Service.  
\`\`\`

\---

**\#\# 15\. DAO contract**

**\#\# 15.1. \`UserDAO\`**

| Method | Input | Output | Ghi chú |  
|---|---|---|---|  
| \`GetAll\` | none | List User | Query Users |  
| \`GetById\` | userId | User? | Query by PK |  
| \`GetByUsername\` | username | User? | Query unique username |  
| \`Search\` | keyword | List User | Query theo keyword |  
| \`Add\` | User | User | SaveChanges |  
| \`Update\` | User | void | SaveChanges |  
| \`SetActive\` | userId, isActive | void | Update IsActive |  
| \`IsUsernameExists\` | username | bool | Any username |  
| \`CountActiveAdmins\` | none | int | Count active admin |

DAO chỉ làm database access, không xử lý role permission.

\---

**\#\# 16\. WPF UI contract**

**\#\# 16.1. \`LoginWindow\`**

Mục tiêu:

\`\`\`text  
Nhập username/password.  
Gọi AuthService.Login.  
Login thành công thì mở MainWindow.  
Login thất bại thì hiển thị lỗi.  
\`\`\`

Input UI:

\`\`\`text  
Username textbox  
Password passwordbox  
Login button  
\`\`\`

Rule:

\`\`\`text  
Không query database trực tiếp.  
Không gọi UserDAO.  
Không gọi AppDbContext.  
Không lưu password sau khi login.  
Có try-catch khi gọi AuthService.  
\`\`\`

**\#\# 16.2. \`MainWindow\`**

Mục tiêu:

\`\`\`text  
Hiển thị shell chính của WPF.  
Hiển thị tên user và role.  
Hiển thị menu theo permission.  
Điều hướng sang màn của các member khác.  
Logout.  
\`\`\`

Menu contract:

| Menu | Permission key | Admin | Staff |  
|---|---|---:|---:|  
| Dashboard | \`Dashboard.View\` | Show | Show |  
| Bàn / Session | \`Tables.View\` | Show | Show |  
| Quản lý bàn / QR | \`Tables.Manage\` | Show | Hide |  
| Category | \`Categories.Manage\` | Show | Hide |  
| Món ăn / Stock | \`Menu.View\` | Show | Show |  
| Order | \`Orders.View\` | Show | Show |  
| Gọi nhân viên | \`Requests.Handle\` | Show | Show |  
| Yêu cầu thanh toán | \`Requests.Handle\` | Show | Show |  
| Bill / Thanh toán | \`Bills.View\` | Show | Show |  
| Lịch sử bill | \`Bills.View\` | Show | Show |  
| Người dùng | \`Users.View\` | Show | Hide |

Rule:

\`\`\`text  
Menu bị hide với role không có quyền.  
Khi click menu vẫn nên check permission lần nữa.  
Không để Staff mở màn Admin-only bằng shortcut/call nội bộ.  
\`\`\`

**\#\# 16.3. \`UserManagementWindow\`**

Mục tiêu:

\`\`\`text  
Admin quản lý user.  
Staff không được mở.  
\`\`\`

Chức năng:

\`\`\`text  
Load danh sách user.  
Search user.  
Create user.  
Update full name / role / active.  
Lock user.  
Unlock user.  
Reset password.  
Clear form.  
\`\`\`

Rule UI:

\`\`\`text  
Username chỉ nhập khi tạo mới.  
Username read-only khi update.  
Không hiển thị PasswordHash.  
PasswordBox chỉ dùng khi tạo/reset password.  
Có DataGrid.  
Có try-catch khi gọi service.  
\`\`\`

\---

**\#\# 17\. API contract**

Nếu WPF gọi trực tiếp Service trong cùng solution, API này có thể dùng cho ASP.NET Core hoặc tương lai. Nếu project triển khai API thật, API phải gọi cùng Service layer với WPF.

**\#\# 17.1. \`POST /api/auth/login\`**

Mục tiêu: đăng nhập Admin/Staff.

Request:

\`\`\`json  
{  
  "username": "admin",  
  "password": "123456"  
}  
\`\`\`

Success response:

\`\`\`json  
{  
  "userId": 1,  
  "username": "admin",  
  "fullName": "Quản trị viên",  
  "role": "Admin",  
  "loginAt": "2026-06-11T10:00:00",  
  "permissions": \[  
    "Users.View",  
    "Tables.Manage",  
    "Orders.View"  
  \]  
}  
\`\`\`

Error response:

\`\`\`json  
{  
  "errorCode": "AUTH\_INVALID\_CREDENTIALS",  
  "message": "Sai tài khoản hoặc mật khẩu."  
}  
\`\`\`

**\#\# 17.2. \`POST /api/auth/logout\`**

Request:

\`\`\`json  
{}  
\`\`\`

Response:

\`\`\`json  
{  
  "success": true  
}  
\`\`\`

**\#\# 17.3. \`GET /api/auth/me\`**

Response:

\`\`\`json  
{  
  "userId": 1,  
  "username": "admin",  
  "fullName": "Quản trị viên",  
  "role": "Admin",  
  "permissions": \[\]  
}  
\`\`\`

**\#\# 17.4. \`GET /api/admin/users\`**

Permission:

\`\`\`text  
Users.View  
\`\`\`

Query optional:

\`\`\`text  
keyword  
role  
isActive  
\`\`\`

Response:

\`\`\`json  
{  
  "items": \[  
    {  
      "userId": 1,  
      "username": "admin",  
      "fullName": "Quản trị viên",  
      "role": "Admin",  
      "isActive": true,  
      "createdAt": "2026-06-11T10:00:00",  
      "updatedAt": null  
    }  
  \]  
}  
\`\`\`

**\#\# 17.5. \`POST /api/admin/users\`**

Permission:

\`\`\`text  
Users.Create  
\`\`\`

Request:

\`\`\`json  
{  
  "username": "staff01",  
  "password": "123456",  
  "fullName": "Nhân viên 01",  
  "role": "Staff"  
}  
\`\`\`

**\#\# 17.6. \`PUT /api/admin/users/{id}\`**

Permission:

\`\`\`text  
Users.Update  
\`\`\`

Request:

\`\`\`json  
{  
  "fullName": "Nhân viên 01",  
  "role": "Staff",  
  "isActive": true  
}  
\`\`\`

**\#\# 17.7. \`PUT /api/admin/users/{id}/lock\`**

Permission:

\`\`\`text  
Users.LockUnlock  
\`\`\`

Response:

\`\`\`json  
{  
  "success": true,  
  "message": "Đã khóa user."  
}  
\`\`\`

**\#\# 17.8. \`PUT /api/admin/users/{id}/unlock\`**

Permission:

\`\`\`text  
Users.LockUnlock  
\`\`\`

**\#\# 17.9. \`PUT /api/admin/users/{id}/reset-password\`**

Permission:

\`\`\`text  
Users.ResetPassword  
\`\`\`

Request:

\`\`\`json  
{  
  "newPassword": "123456",  
  "confirmPassword": "123456"  
}  
\`\`\`

**\#\# 17.10. \`PUT /api/auth/change-password\`**

Permission:

\`\`\`text  
Logged in  
\`\`\`

Request:

\`\`\`json  
{  
  "currentPassword": "old123456",  
  "newPassword": "new123456",  
  "confirmPassword": "new123456"  
}  
\`\`\`

\---

**\#\# 18\. Error contract**

| Error code | Message | Khi nào xảy ra |  
|---|---|---|  
| \`AUTH\_USERNAME\_REQUIRED\` | Username không được để trống. | Login thiếu username |  
| \`AUTH\_PASSWORD\_REQUIRED\` | Password không được để trống. | Login thiếu password |  
| \`AUTH\_INVALID\_CREDENTIALS\` | Sai tài khoản hoặc mật khẩu. | Username không tồn tại hoặc password sai |  
| \`AUTH\_USER\_INACTIVE\` | Tài khoản đã bị khóa hoặc không còn hoạt động. | IsActive \= false |  
| \`AUTH\_NOT\_LOGGED\_IN\` | Bạn cần đăng nhập để sử dụng chức năng này. | Gọi service khi chưa login |  
| \`AUTH\_PERMISSION\_DENIED\` | Bạn không có quyền thực hiện chức năng này. | Role không đủ quyền |  
| \`USER\_NOT\_FOUND\` | Không tìm thấy user. | UserId sai |  
| \`USER\_USERNAME\_REQUIRED\` | Username không được để trống. | Tạo user thiếu username |  
| \`USER\_USERNAME\_DUPLICATED\` | Username đã tồn tại. | Tạo user trùng username |  
| \`USER\_PASSWORD\_TOO\_SHORT\` | Password phải có ít nhất 6 ký tự. | Tạo/reset/change password |  
| \`USER\_FULLNAME\_REQUIRED\` | Họ tên không được để trống. | Tạo/sửa user |  
| \`USER\_ROLE\_INVALID\` | Role không hợp lệ. | Role khác Admin/Staff |  
| \`USER\_CANNOT\_LOCK\_LAST\_ADMIN\` | Không thể khóa Admin active cuối cùng. | Lock/update active admin cuối |  
| \`USER\_CONFIRM\_PASSWORD\_NOT\_MATCH\` | Xác nhận mật khẩu không khớp. | Reset/change password |  
| \`USER\_CURRENT\_PASSWORD\_INVALID\` | Mật khẩu hiện tại không đúng. | Change own password |

\---

**\#\# 19\. Integration contract với các member khác**

**\#\# 19.1. Contract cung cấp cho Member 2 \- Table & QR**

Member 2 cần dùng:

\`\`\`text  
CurrentUserService.GetCurrentUserId()  
PermissionService.RequirePermission("Tables.Manage")  
PermissionService.RequirePermission("Tables.ResetQr")  
\`\`\`

Ứng dụng:

\`\`\`text  
Tạo/sửa/ẩn bàn: Admin only.  
Reset QR: Admin only.  
Xem bàn: Admin/Staff.  
\`\`\`

**\#\# 19.2. Contract cung cấp cho Member 3 \- Menu & Stock**

Member 3 cần dùng:

\`\`\`text  
PermissionService.RequirePermission("Menu.ManageMasterData")  
PermissionService.RequirePermission("Menu.UpdateStock")  
PermissionService.RequirePermission("Menu.SetAvailability")  
CurrentUserService.GetCurrentUserId()  
\`\`\`

Ứng dụng:

\`\`\`text  
Admin thêm/sửa/ẩn món.  
Staff chỉ cập nhật stock, sold out, reopen, availability.  
\`\`\`

**\#\# 19.3. Contract cung cấp cho Member 4 \- Session / Order / Print**

Member 4 cần dùng:

\`\`\`text  
CurrentUserService.GetCurrentUserId()  
PermissionService.RequirePermission("Orders.View")  
PermissionService.RequirePermission("Orders.Cancel")  
PermissionService.RequirePermission("Orders.Reprint")  
\`\`\`

Ứng dụng:

\`\`\`text  
CreatedBy khi Staff tạo order.  
CancelledBy khi cancel order.  
Audit khi mark printed / failed nếu cần.  
\`\`\`

**\#\# 19.4. Contract cung cấp cho Member 5 \- Bill / Payment / Dashboard**

Member 5 cần dùng:

\`\`\`text  
CurrentUserService.GetCurrentUserId()  
PermissionService.RequirePermission("Bills.Split")  
PermissionService.RequirePermission("Payments.Confirm")  
PermissionService.RequirePermission("Payments.UpdatePaidMethod")  
PermissionService.RequirePermission("Dashboard.View")  
\`\`\`

Ứng dụng:

\`\`\`text  
Bill.CreatedBy  
Bill.CancelledBy  
Payment.ConfirmedBy  
Payment.UpdatedBy  
Admin-only sửa payment method sau Paid  
\`\`\`

\---

**\#\# 20\. Security rules**

\`\`\`text  
Không lưu password plain text.  
Không log password.  
Không trả PasswordHash ra DTO/API/UI.  
Không commit password production vào appsettings.json.  
Không hard-code tài khoản thật trong code.  
Seed data chỉ dùng cho development/demo.  
Sau login chỉ lưu thông tin tối thiểu: UserId, Username, FullName, Role.  
\`\`\`

\---

**\#\# 21\. WPF validation rules**

**\#\# 21.1. LoginWindow validation**

\`\`\`text  
Username không rỗng.  
Password không rỗng.  
Enter có thể trigger login.  
Login thất bại không clear username.  
Login thất bại nên clear password hoặc focus lại password.  
\`\`\`

**\#\# 21.2. UserManagementWindow validation**

\`\`\`text  
Username không rỗng khi tạo.  
Username ít nhất 3 ký tự.  
Password ít nhất 6 ký tự khi tạo/reset.  
FullName không rỗng.  
Role bắt buộc chọn Admin/Staff.  
Không cho update nếu chưa chọn user.  
Không cho lock nếu chưa chọn user.  
Không cho reset password nếu password confirm không khớp.  
\`\`\`

\---

**\#\# 22\. Acceptance criteria**

**\#\# 22.1. Auth**

\`\`\`text  
\[ \] Admin login đúng thì vào MainWindow.  
\[ \] Staff login đúng thì vào MainWindow.  
\[ \] Sai username/password báo lỗi.  
\[ \] User inactive không login được.  
\[ \] Password được verify bằng hash.  
\[ \] Sau login lưu CurrentUser.  
\[ \] Logout clear CurrentUser.  
\`\`\`

**\#\# 22.2. Permission**

\`\`\`text  
\[ \] Admin thấy đầy đủ menu.  
\[ \] Staff không thấy menu User Management.  
\[ \] Staff không thấy menu Admin-only.  
\[ \] Service vẫn chặn Staff nếu gọi chức năng Admin-only.  
\[ \] PermissionKey dùng thống nhất cho các module.  
\`\`\`

**\#\# 22.3. User management**

\`\`\`text  
\[ \] Admin xem được danh sách user.  
\[ \] Staff không mở được UserManagementWindow.  
\[ \] Admin tạo user thành công.  
\[ \] Username trùng bị chặn.  
\[ \] Password tạo mới được hash.  
\[ \] Admin sửa FullName/Role/IsActive được.  
\[ \] Admin khóa user được.  
\[ \] User bị khóa không login được.  
\[ \] Admin mở khóa user được.  
\[ \] Admin reset password user khác được.  
\[ \] Không trả PasswordHash ra UI.  
\[ \] Không xóa cứng user.  
\`\`\`

**\#\# 22.4. Integration**

\`\`\`text  
\[ \] Member 2 gọi được PermissionService cho Table/QR.  
\[ \] Member 3 gọi được PermissionService cho Menu/Stock.  
\[ \] Member 4 lấy được CurrentUserId khi cancel order.  
\[ \] Member 5 lấy được CurrentUserId khi confirm payment.  
\[ \] Mọi module dùng chung PermissionKey, không tự tạo role checker riêng.  
\`\`\`

\---

**\#\# 23\. Definition of Done cho Member 1**

\`\`\`text  
\[ \] Có Users table đúng constraint.  
\[ \] Có seed Admin/Staff demo.  
\[ \] Có BusinessObjects/Auth đầy đủ DTO/constants.  
\[ \] Có UserDAO.  
\[ \] Có IUserRepository/UserRepository.  
\[ \] Có IAuthService/AuthService.  
\[ \] Có IUserService/UserService.  
\[ \] Có ICurrentUserService/CurrentUserService.  
\[ \] Có IPermissionService/PermissionService.  
\[ \] Có PasswordHasher contract.  
\[ \] Có LoginWindow chạy được.  
\[ \] Có MainWindow shell và menu theo role.  
\[ \] Có UserManagementWindow cho Admin.  
\[ \] WPF chỉ gọi Service.  
\[ \] Service kiểm tra business rule và permission.  
\[ \] Repository chỉ gọi DAO.  
\[ \] DAO chỉ query database.  
\[ \] Không MessageBox trong Service/Repository/DAO.  
\[ \] Không trả PasswordHash ra UI.  
\[ \] Build không lỗi.  
\[ \] Demo được flow Login → MainWindow → UserManagement → Logout.  
\`\`\`

\---

**\#\# 24\. Branch / Git contract**

Branch đề xuất:

\`\`\`text  
feature/auth-user-permission-shell  
\`\`\`

Commit message đề xuất:

\`\`\`text  
feat: add auth login business objects  
feat: add user repository and service contracts  
feat: add permission service contract  
ui: add login and main shell windows  
ui: add user management window  
fix: validate inactive user login  
\`\`\`

Không commit:

\`\`\`text  
bin/  
obj/  
password thật  
connection string production  
API key thật  
file test tạm  
comment rác  
\`\`\`

\---

**\#\# 25\. Tóm tắt trách nhiệm Member 1**

Member 1 là người xây nền móng bảo mật và phân quyền cho toàn bộ WPF Staff App.

Kết quả cần bàn giao:

\`\`\`text  
User đăng nhập được.  
Role được xác định đúng.  
Menu WPF hiển thị đúng quyền.  
Service chặn quyền đúng.  
User management chỉ Admin dùng được.  
Các module khác lấy được CurrentUserId và PermissionService.  
\`\`\`

Nếu Member 1 làm đúng contract này, các Member 2/3/4/5 chỉ cần gọi:

\`\`\`text  
CurrentUserService.GetCurrentUserId()  
PermissionService.RequirePermission(permissionKey)  
\`\`\`

là có thể xử lý audit và phân quyền thống nhất toàn hệ thống.

# Minh

**\# Member 2 — Table & QR Management**

\> Tài liệu nghiệp vụ, business rules, service contracts, API contracts và WPF contracts cho **\*\*Member 2\*\*** trong dự án **\*\*QR Food Ordering Management System\*\***.  
\>  
\> Quyết định phạm vi sau khi chỉnh: **\*\*Member 2 chỉ phụ trách \`DiningTables\` \+ QR \+ trạng thái bàn ở mức hiển thị/cập nhật an toàn\*\***.    
\> \`TableSessions\` và \`TableSessionCustomers\` được chuyển sang **\*\*Member 4 — Session / Order / Print\*\***, vì session được tạo trong transaction của order đầu tiên.

\---

**\#\# 1\. Mục tiêu module**

Module của Member 2 đảm bảo hệ thống quản lý đúng dữ liệu bàn ăn và QR:

\`\`\`text  
Admin tạo/sửa/ẩn/bật lại bàn.  
Admin tạo/reset QR cho bàn.  
Customer Web xác thực QR token để biết đang ở bàn nào.  
WPF Staff/Admin xem trạng thái bàn.  
Các module khác có contract để lấy bàn, xác thực bàn active và cập nhật trạng thái bàn.  
\`\`\`

Module này là **\*\*master data \+ gateway cho QR\*\***, không phải module xử lý transaction order/session.

\---

**\#\# 2\. Phạm vi chính xác của Member 2**

**\#\#\# 2.1. Member 2 phụ trách**

\`\`\`text  
DiningTables  
QR token  
QR URL  
Table management  
Table status display  
Validate QR token  
Check table active/inactive  
Contract cập nhật trạng thái bàn cho Member 4/5  
WPF TableManagementWindow  
WPF TableStatusWindow hoặc TableOverviewWindow  
\`\`\`

**\#\#\# 2.2. Member 2 không phụ trách**

\`\`\`text  
Không tạo TableSession.  
Không tạo TableSessionCustomer.  
Không xử lý order đầu tiên.  
Không xử lý ClientToken theo session.  
Không xử lý OrderItems.  
Không xử lý Bill/BillDetails.  
Không xác nhận Payment.  
Không đóng session.  
Không tự quyết định khi nào bàn chuyển Occupied/WaitingPayment nếu nghiệp vụ đến từ order/payment.  
\`\`\`

**\#\#\# 2.3. Ranh giới module sau khi chỉnh**

\`\`\`text  
Member 2:  
\- DiningTables  
\- QR token  
\- QR URL  
\- Table management

Member 4:  
\- TableSessions  
\- TableSessionCustomers  
\- Orders  
\- OrderItems  
\- PrintStatus

Member 5:  
\- Bills  
\- BillDetails  
\- Payments  
\- Dashboard  
\`\`\`

\---

**\#\# 3\. Layer rule bắt buộc**

Member 2 vẫn phải đi đủ flow feature-based:

\`\`\`text  
WPF UI  
→ Service  
→ Repository  
→ DAO  
→ Entity / DTO  
→ SQL Server  
\`\`\`

Rule bắt buộc:

\`\`\`text  
WPF không gọi DAO.  
WPF không gọi DbContext.  
WPF không viết SQL.  
Service xử lý validate và business rule.  
Repository chỉ gọi DAO.  
DAO chỉ query database.  
BusinessObjects chỉ chứa Entity, DTO, Enum/constant.  
Service không hiển thị MessageBox.  
DAO không xử lý nghiệp vụ phức tạp.  
\`\`\`

\---

**\#\# 4\. Database ownership**

**\#\#\# 4.1. Bảng chính Member 2 sở hữu**

\`\`\`text  
DiningTables  
\`\`\`

**\#\#\# 4.2. Bảng Member 2 không sở hữu nhưng có liên quan**

\`\`\`text  
TableSessions       → owner: Member 4  
Orders              → owner: Member 4  
ServiceRequests     → owner: Member 4 hoặc Request submodule  
Bills               → owner: Member 5  
Payments            → owner: Member 5  
\`\`\`

Member 2 có thể **\*\*read summary\*\*** từ các module khác qua service contract, nhưng không query chéo trực tiếp nếu làm theo service boundary.

\---

**\#\# 5\. Entity: DiningTables**

**\#\#\# 5.1. Field contract**

| Field | Type | Required | Owner | Ý nghĩa |  
|---|---|---:|---|---|  
| \`TableId\` | int | Yes | DB | Khóa chính của bàn |  
| \`TableName\` | string | Yes | Member 2 | Tên bàn: \`Bàn 1\`, \`A01\`, \`VIP 2\` |  
| \`Area\` | string? | No | Member 2 | Khu vực bàn: \`Tầng 1\`, \`Sân vườn\`, \`VIP\` |  
| \`QrToken\` | string | Yes | Member 2 | Token duy nhất dùng trong QR |  
| \`Status\` | string | Yes | Member 2 \+ Member 4/5 contract | Trạng thái bàn |  
| \`IsActive\` | bool | Yes | Member 2 | Bàn còn dùng trong vận hành hay không |  
| \`CreatedAt\` | datetime | Yes | Member 2 | Thời điểm tạo bàn |  
| \`UpdatedAt\` | datetime? | No | Member 2 | Thời điểm cập nhật gần nhất |

**\#\#\# 5.2. Status của bàn**

\`\`\`text  
Available  
Occupied  
WaitingPayment  
\`\`\`

Ý nghĩa:

| Status | Ý nghĩa | Ai kích hoạt |  
|---|---|---|  
| \`Available\` | Bàn trống, có thể phục vụ lượt mới | Member 2 khi tạo bàn; Member 4/5 khi session đóng |  
| \`Occupied\` | Bàn đang có khách/session mở | Member 4 khi tạo/mở session |  
| \`WaitingPayment\` | Bàn/session đang chờ thanh toán | Member 4/5 khi có payment request hoặc session chờ thanh toán |

Không dùng \`Inactive\` trong \`Status\`. Bàn ngưng sử dụng dùng:

\`\`\`text  
IsActive \= false  
\`\`\`

\---

**\#\# 6\. Database constraints**

**\#\#\# 6.1. Primary key**

\`\`\`text  
PK: DiningTables.TableId  
\`\`\`

**\#\#\# 6.2. Unique constraints**

\`\`\`text  
UNIQUE: DiningTables.QrToken  
\`\`\`

Khuyến nghị thêm unique mềm hoặc service-level rule cho tên bàn:

\`\`\`text  
TableName không nên trùng trong cùng Area.  
\`\`\`

Tức là có thể áp dụng rule:

\`\`\`text  
UNIQUE: Area \+ TableName  
\`\`\`

Nếu nhóm muốn đơn giản hơn trong MVP, chỉ validate cảnh báo ở Service.

**\#\#\# 6.3. Check constraints**

\`\`\`text  
CHECK Status IN ('Available', 'Occupied', 'WaitingPayment')  
\`\`\`

**\#\#\# 6.4. Index đề xuất**

\`\`\`text  
IX\_DiningTables\_Status  
IX\_DiningTables\_IsActive  
IX\_DiningTables\_Area\_Status  
IX\_DiningTables\_QrToken  
\`\`\`

\---

**\#\# 7\. Business Rules — Table**

**\#\#\# BR-TABLE-001: Bàn mới phải có QR token**

Khi Admin tạo bàn mới:

\`\`\`text  
TableName bắt buộc.  
Area có thể null.  
QrToken tự sinh.  
Status \= Available.  
IsActive \= true.  
CreatedAt \= now.  
\`\`\`

Không cho nhập \`QrToken\` thủ công từ UI.

\---

**\#\#\# BR-TABLE-002: TableName không được rỗng**

Rule:

\`\`\`text  
TableName không null.  
TableName không toàn khoảng trắng.  
TableName tối đa 50 ký tự.  
TableName nên trim trước khi lưu.  
\`\`\`

Lỗi chuẩn:

\`\`\`text  
TABLE\_NAME\_REQUIRED: Tên bàn không được để trống.  
TABLE\_NAME\_TOO\_LONG: Tên bàn không được vượt quá 50 ký tự.  
\`\`\`

\---

**\#\#\# BR-TABLE-003: Area là optional nhưng phải hợp lệ nếu có**

Rule:

\`\`\`text  
Area có thể null.  
Nếu có Area thì trim trước khi lưu.  
Area tối đa 50 ký tự.  
\`\`\`

Ví dụ hợp lệ:

\`\`\`text  
Tầng 1  
Tầng 2  
Sân vườn  
VIP  
Mang đi  
\`\`\`

\---

**\#\#\# BR-TABLE-004: Bàn inactive không được phục vụ order**

Nếu:

\`\`\`text  
DiningTables.IsActive \= false  
\`\`\`

Thì:

\`\`\`text  
Customer Web không được order.  
Customer Web có thể báo: Bàn hiện không hoạt động.  
Staff/Admin vẫn có thể xem lịch sử.  
Admin có thể bật lại bàn nếu cần.  
\`\`\`

\---

**\#\#\# BR-TABLE-005: Không xóa cứng bàn đã phát sinh nghiệp vụ**

Không dùng delete cứng cho \`DiningTables\` nếu đã có session/order/bill liên quan.

Cách đúng:

\`\`\`text  
Ẩn bàn bằng IsActive \= false.  
\`\`\`

Lý do:

\`\`\`text  
Order/bill lịch sử vẫn cần biết bàn cũ.  
Dashboard/lịch sử không bị mất dữ liệu.  
Tránh lỗi FK khi xóa bàn.  
\`\`\`

\---

**\#\#\# BR-TABLE-006: Staff không được tạo/sửa/ẩn/reset QR**

Permission:

\`\`\`text  
Admin:  
\- Create table  
\- Update table  
\- Deactivate table  
\- Reactivate table  
\- Reset QR

Staff:  
\- View table list  
\- View table status  
\- Open session detail thông qua module Member 4  
\`\`\`

\---

**\#\#\# BR-TABLE-007: Không cho sửa trực tiếp Status từ màn quản lý bàn**

\`DiningTables.Status\` không nên cho Admin/Staff sửa trực tiếp bằng dropdown.

Status chỉ thay đổi bởi nghiệp vụ:

\`\`\`text  
Tạo session/open session      → Occupied  
Payment request/wait payment → WaitingPayment  
Close session                → Available  
Create new table             → Available  
\`\`\`

Member 2 chỉ cung cấp internal contract để module khác cập nhật status an toàn.

\---

**\#\#\# BR-TABLE-008: Không deactivate bàn đang có session hoạt động**

Nếu bàn đang:

\`\`\`text  
Status \= Occupied  
hoặc Status \= WaitingPayment  
\`\`\`

Thì không cho \`IsActive \= false\`, trừ khi Admin xử lý session trước.

Lỗi chuẩn:

\`\`\`text  
TABLE\_HAS\_ACTIVE\_SESSION: Không thể ẩn bàn đang có phiên phục vụ.  
\`\`\`

\---

**\#\#\# BR-TABLE-009: Reactivate table**

Khi bật lại bàn:

\`\`\`text  
IsActive \= true.  
UpdatedAt \= now.  
Status giữ nguyên nếu dữ liệu đang hợp lệ.  
Nếu không có session active thì status nên là Available.  
\`\`\`

Nếu hệ thống phát hiện status bất thường, gọi \`SyncTableStatus\` qua contract với Member 4\.

\---

**\#\# 8\. Business Rules — QR**

**\#\#\# BR-QR-001: Mỗi bàn có một QrToken duy nhất**

\`\`\`text  
DiningTables.QrToken phải unique.  
Không có 2 bàn dùng chung một token.  
\`\`\`

\---

**\#\#\# BR-QR-002: QR URL không chứa TableId trực tiếp**

QR URL đúng:

\`\`\`text  
https://customer-web/menu?t={QrToken}  
\`\`\`

Không dùng:

\`\`\`text  
https://customer-web/menu?tableId=5  
\`\`\`

Lý do:

\`\`\`text  
TableId dễ đoán.  
Khách có thể sửa URL để nhảy bàn.  
QrToken khó đoán và có thể reset khi cần.  
\`\`\`

\---

**\#\#\# BR-QR-003: QR URL được generate từ config**

QR URL không hard-code trong nhiều nơi.

Nguồn chuẩn:

\`\`\`text  
CustomerWebBaseUrl \+ '?t=' \+ QrToken  
\`\`\`

Ví dụ config:

\`\`\`json  
{  
  "CustomerWeb": {  
    "BaseUrl": "https://localhost:3000/menu"  
  }  
}  
\`\`\`

\---

**\#\#\# BR-QR-004: Reset QR chỉ dành cho Admin**

Khi reset QR:

\`\`\`text  
Sinh token mới.  
Token mới phải unique.  
Ghi UpdatedAt \= now.  
Token cũ mất hiệu lực ngay.  
QR URL mới được tạo từ token mới.  
\`\`\`

\---

**\#\#\# BR-QR-005: Reset QR khi bàn đang phục vụ cần bị chặn hoặc cảnh báo mạnh**

Rule chuẩn đề xuất cho MVP:

\`\`\`text  
Nếu bàn Available:  
    Cho reset QR.

Nếu bàn Occupied hoặc WaitingPayment:  
    Không cho reset QR.  
    Yêu cầu đóng session hoặc xử lý bàn trước.  
\`\`\`

Lý do:

\`\`\`text  
Khách đang dùng QR/token cũ có thể bị mất quyền gửi request.  
Dễ gây lỗi trải nghiệm Customer Web.  
\`\`\`

Lỗi chuẩn:

\`\`\`text  
QR\_RESET\_BLOCKED\_ACTIVE\_TABLE: Không thể reset QR khi bàn đang phục vụ hoặc chờ thanh toán.  
\`\`\`

\---

**\#\#\# BR-QR-006: Validate QR token cho Customer Web**

Khi Customer Web gọi API bằng token:

\`\`\`text  
Nếu token không tồn tại → invalid.  
Nếu bàn inactive → invalid hoặc canOrder=false.  
Nếu bàn active → trả thông tin bàn.  
Nếu bàn WaitingPayment → trả canOrder=false hoặc warning tùy rule order.  
\`\`\`

Rule MVP đề xuất:

\`\`\`text  
Available      → canOrder \= true  
Occupied       → canOrder \= true, vì khách có thể gọi thêm món  
WaitingPayment → canOrder \= false, vì bàn đang chờ thanh toán  
Inactive       → canOrder \= false  
\`\`\`

\---

**\#\# 9\. Contract DTO / Request / Response**

**\#\#\# 9.1. \`DiningTableDto\`**

Dùng cho Admin/Staff WPF.

| Field | Type | Required | Ghi chú |  
|---|---|---:|---|  
| \`TableId\` | int | Yes | ID bàn |  
| \`TableName\` | string | Yes | Tên bàn |  
| \`Area\` | string? | No | Khu vực |  
| \`QrToken\` | string | Yes | Token QR |  
| \`QrUrl\` | string | Yes | URL QR đã generate |  
| \`Status\` | string | Yes | \`Available\`, \`Occupied\`, \`WaitingPayment\` |  
| \`IsActive\` | bool | Yes | Bàn còn hoạt động hay không |  
| \`CreatedAt\` | datetime | Yes | Ngày tạo |  
| \`UpdatedAt\` | datetime? | No | Ngày cập nhật |

\---

**\#\#\# 9.2. \`CreateTableRequest\`**

Dùng khi Admin tạo bàn.

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`TableName\` | string | Yes | Không rỗng, max 50 |  
| \`Area\` | string? | No | Max 50 |

Response: \`DiningTableDto\`.

\---

**\#\#\# 9.3. \`UpdateTableRequest\`**

Dùng khi Admin sửa bàn.

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`TableName\` | string | Yes | Không rỗng, max 50 |  
| \`Area\` | string? | No | Max 50 |  
| \`IsActive\` | bool | Yes | Không cho inactive nếu bàn đang phục vụ |

Không có field:

\`\`\`text  
QrToken  
Status  
\`\`\`

Vì QR reset và status update phải đi qua nghiệp vụ riêng.

\---

**\#\#\# 9.4. \`TableQrDto\`**

Dùng khi hiển thị hoặc reset QR.

| Field | Type | Required | Ghi chú |  
|---|---|---:|---|  
| \`TableId\` | int | Yes | ID bàn |  
| \`TableName\` | string | Yes | Tên bàn |  
| \`QrToken\` | string | Yes | Token hiện tại |  
| \`QrUrl\` | string | Yes | URL đầy đủ cho Customer Web |  
| \`UpdatedAt\` | datetime? | No | Thời điểm reset gần nhất |

\---

**\#\#\# 9.5. \`ValidateQrTokenResponse\`**

Dùng cho Customer Web sau khi quét QR.

| Field | Type | Required | Ghi chú |  
|---|---|---:|---|  
| \`IsValid\` | bool | Yes | Token hợp lệ không |  
| \`TableId\` | int? | No | Có khi token hợp lệ |  
| \`TableName\` | string? | No | Tên bàn |  
| \`Area\` | string? | No | Khu vực |  
| \`TableStatus\` | string? | No | Trạng thái bàn |  
| \`CanOrder\` | bool | Yes | Có cho order không |  
| \`Message\` | string | Yes | Thông báo cho customer web |

Ví dụ token hợp lệ:

\`\`\`json  
{  
  "isValid": true,  
  "tableId": 5,  
  "tableName": "Bàn 5",  
  "area": "Tầng 1",  
  "tableStatus": "Occupied",  
  "canOrder": true,  
  "message": "QR hợp lệ."  
}  
\`\`\`

Ví dụ token sai:

\`\`\`json  
{  
  "isValid": false,  
  "tableId": null,  
  "tableName": null,  
  "area": null,  
  "tableStatus": null,  
  "canOrder": false,  
  "message": "QR không hợp lệ hoặc đã hết hiệu lực."  
}  
\`\`\`

\---

**\#\#\# 9.6. \`TableStatusSummaryDto\`**

Dùng cho WPF màn tổng quan bàn.

| Field | Type | Required | Owner dữ liệu |  
|---|---|---:|---|  
| \`TableId\` | int | Yes | Member 2 |  
| \`TableName\` | string | Yes | Member 2 |  
| \`Area\` | string? | No | Member 2 |  
| \`Status\` | string | Yes | Member 2 |  
| \`IsActive\` | bool | Yes | Member 2 |  
| \`CurrentSessionId\` | int? | No | Member 4 |  
| \`StartedAt\` | datetime? | No | Member 4 |  
| \`OrderCount\` | int | No | Member 4 |  
| \`BillCount\` | int | No | Member 5 |  
| \`UnpaidAmount\` | decimal | No | Member 5 |

Member 2 định nghĩa phần table, còn số liệu session/order/bill lấy qua contract của Member 4/5.

\---

**\#\# 10\. Service Contracts**

**\#\#\# 10.1. \`ITableService\`**

Service chính cho quản lý bàn.

| Method | Input | Output | Quyền | Mục đích |  
|---|---|---|---|---|  
| \`GetAllTables\` | filter optional | List \`DiningTableDto\` | Admin/Staff | Xem danh sách bàn |  
| \`GetActiveTables\` | none/filter | List \`DiningTableDto\` | Admin/Staff | Xem bàn đang hoạt động |  
| \`GetTableById\` | \`tableId\` | \`DiningTableDto\` | Admin/Staff/Internal | Lấy bàn theo ID |  
| \`CreateTable\` | \`CreateTableRequest\` | \`DiningTableDto\` | Admin | Tạo bàn mới |  
| \`UpdateTable\` | \`tableId\`, \`UpdateTableRequest\` | \`DiningTableDto\` | Admin | Sửa tên/khu vực/active |  
| \`DeactivateTable\` | \`tableId\` | success/fail | Admin | Ẩn bàn |  
| \`ReactivateTable\` | \`tableId\` | success/fail | Admin | Bật lại bàn |  
| \`EnsureTableCanServe\` | \`tableId\` | success/fail | Internal | Kiểm tra bàn active để order |  
| \`UpdateTableStatus\` | \`tableId\`, \`status\`, \`reason\` | success/fail | Internal | Cập nhật status bởi module nghiệp vụ |

\---

**\#\#\# 10.2. \`ITableQrService\`**

Service riêng cho QR.

| Method | Input | Output | Quyền | Mục đích |  
|---|---|---|---|---|  
| \`GenerateQrToken\` | none | string | Internal | Sinh token khó đoán |  
| \`GenerateQrUrl\` | \`qrToken\` | string | Internal/Admin/Staff | Tạo URL QR |  
| \`GetQrByTableId\` | \`tableId\` | \`TableQrDto\` | Admin | Lấy QR hiện tại |  
| \`ResetQrToken\` | \`tableId\` | \`TableQrDto\` | Admin | Reset QR |  
| \`ValidateQrToken\` | \`qrToken\` | \`ValidateQrTokenResponse\` | Public | Customer Web xác thực QR |

\---

**\#\#\# 10.3. \`ITableReadService\`**

Service đọc trạng thái bàn cho WPF.

| Method | Input | Output | Quyền | Mục đích |  
|---|---|---|---|---|  
| \`GetTableStatusOverview\` | filter optional | List \`TableStatusSummaryDto\` | Admin/Staff | Màn trạng thái bàn |  
| \`GetTablesByArea\` | \`area\` | List \`DiningTableDto\` | Admin/Staff | Lọc theo khu vực |  
| \`GetTablesByStatus\` | \`status\` | List \`DiningTableDto\` | Admin/Staff | Lọc theo trạng thái |  
| \`SearchTables\` | \`keyword\` | List \`DiningTableDto\` | Admin/Staff | Search bàn |

\---

**\#\#\# 10.4. \`ITableStatusPort\`**

Internal contract cho Member 4/5 gọi, không nên expose trực tiếp ra WPF.

| Method | Input | Output | Caller |  
|---|---|---|---|  
| \`SetTableOccupied\` | \`tableId\`, \`tableSessionId\` | success/fail | Member 4 |  
| \`SetTableWaitingPayment\` | \`tableId\`, \`tableSessionId\` | success/fail | Member 4/5 |  
| \`SetTableAvailable\` | \`tableId\`, \`tableSessionId\` | success/fail | Member 4/5 |  
| \`SyncTableStatus\` | \`tableId\` | \`DiningTableDto\` | Member 4/5/Admin tool |

Rule:

\`\`\`text  
Chỉ service nội bộ được gọi.  
Không cho WPF gọi trực tiếp các method status port.  
Phải ghi reason hoặc audit note nếu nhóm có audit.  
\`\`\`

\---

**\#\# 11\. API Contracts**

**\#\# 11.1. Customer API**

**\#\#\# GET \`/api/customer/tables/by-token/{token}\`**

Dùng khi khách quét QR.

Input:

\`\`\`text  
Path:  
\- token: string  
\`\`\`

Response success:

\`\`\`json  
{  
  "isValid": true,  
  "tableId": 5,  
  "tableName": "Bàn 5",  
  "area": "Tầng 1",  
  "tableStatus": "Available",  
  "canOrder": true,  
  "message": "QR hợp lệ."  
}  
\`\`\`

Response invalid:

\`\`\`json  
{  
  "isValid": false,  
  "tableId": null,  
  "tableName": null,  
  "area": null,  
  "tableStatus": null,  
  "canOrder": false,  
  "message": "QR không hợp lệ hoặc đã hết hiệu lực."  
}  
\`\`\`

Rule:

\`\`\`text  
Không yêu cầu đăng nhập.  
Không trả QrToken nội bộ nếu không cần.  
Không trả dữ liệu nhạy cảm.  
Không tạo TableSession tại API này.  
Chỉ validate QR và trả thông tin bàn.  
\`\`\`

\---

**\#\# 11.2. Admin API — Table Management**

**\#\#\# GET \`/api/admin/tables\`**

Dùng để Admin xem toàn bộ bàn.

Query optional:

\`\`\`text  
keyword  
area  
status  
isActive  
\`\`\`

Response:

\`\`\`json  
{  
  "items": \[  
    {  
      "tableId": 1,  
      "tableName": "Bàn 1",  
      "area": "Tầng 1",  
      "qrToken": "abcxyz",  
      "qrUrl": "https://customer-web/menu?t=abcxyz",  
      "status": "Available",  
      "isActive": true,  
      "createdAt": "2026-06-11T08:00:00",  
      "updatedAt": null  
    }  
  \]  
}  
\`\`\`

\---

**\#\#\# GET \`/api/admin/tables/{id}\`**

Lấy chi tiết một bàn.

Rule:

\`\`\`text  
Admin only.  
Nếu không tồn tại trả TABLE\_NOT\_FOUND.  
\`\`\`

\---

**\#\#\# POST \`/api/admin/tables\`**

Tạo bàn mới.

Request:

\`\`\`json  
{  
  "tableName": "Bàn 10",  
  "area": "Tầng 1"  
}  
\`\`\`

Response:

\`\`\`json  
{  
  "tableId": 10,  
  "tableName": "Bàn 10",  
  "area": "Tầng 1",  
  "qrToken": "generated-token",  
  "qrUrl": "https://customer-web/menu?t=generated-token",  
  "status": "Available",  
  "isActive": true  
}  
\`\`\`

Rules:

\`\`\`text  
Admin only.  
TableName bắt buộc.  
QrToken tự sinh.  
Status mặc định Available.  
IsActive mặc định true.  
\`\`\`

\---

**\#\#\# PUT \`/api/admin/tables/{id}\`**

Sửa thông tin bàn.

Request:

\`\`\`json  
{  
  "tableName": "Bàn VIP 1",  
  "area": "VIP",  
  "isActive": true  
}  
\`\`\`

Rules:

\`\`\`text  
Admin only.  
Không cho sửa QrToken ở endpoint này.  
Không cho sửa Status ở endpoint này.  
Không cho inactive nếu bàn đang Occupied/WaitingPayment.  
\`\`\`

\---

**\#\#\# PUT \`/api/admin/tables/{id}/deactivate\`**

Ẩn bàn khỏi vận hành.

Rules:

\`\`\`text  
Admin only.  
Không xóa cứng.  
Không cho deactivate nếu bàn đang Occupied/WaitingPayment.  
Set IsActive \= false.  
UpdatedAt \= now.  
\`\`\`

\---

**\#\#\# PUT \`/api/admin/tables/{id}/reactivate\`**

Bật lại bàn.

Rules:

\`\`\`text  
Admin only.  
Set IsActive \= true.  
Nếu không có session active thì Status \= Available.  
UpdatedAt \= now.  
\`\`\`

\---

**\#\#\# PUT \`/api/admin/tables/{id}/reset-qr\`**

Reset QR cho bàn.

Response:

\`\`\`json  
{  
  "tableId": 10,  
  "tableName": "Bàn 10",  
  "qrToken": "new-token",  
  "qrUrl": "https://customer-web/menu?t=new-token",  
  "updatedAt": "2026-06-11T10:20:00"  
}  
\`\`\`

Rules:

\`\`\`text  
Admin only.  
Bàn phải tồn tại.  
Bàn nên đang Available.  
Sinh token mới unique.  
Token cũ mất hiệu lực.  
Không reset khi bàn Occupied/WaitingPayment trong MVP.  
\`\`\`

\---

**\#\#\# GET \`/api/admin/tables/{id}/qr\`**

Lấy thông tin QR hiện tại của bàn.

Response:

\`\`\`json  
{  
  "tableId": 10,  
  "tableName": "Bàn 10",  
  "qrToken": "current-token",  
  "qrUrl": "https://customer-web/menu?t=current-token",  
  "updatedAt": "2026-06-11T10:20:00"  
}  
\`\`\`

\---

**\#\# 11.3. Staff API — Table Overview**

**\#\#\# GET \`/api/staff/tables/status\`**

Dùng cho WPF hiển thị trạng thái bàn.

Query optional:

\`\`\`text  
area  
status  
keyword  
\`\`\`

Response:

\`\`\`json  
{  
  "items": \[  
    {  
      "tableId": 1,  
      "tableName": "Bàn 1",  
      "area": "Tầng 1",  
      "status": "Occupied",  
      "isActive": true,  
      "currentSessionId": 1001,  
      "startedAt": "2026-06-11T09:30:00",  
      "orderCount": 3,  
      "billCount": 1,  
      "unpaidAmount": 250000  
    }  
  \]  
}  
\`\`\`

Rules:

\`\`\`text  
Admin/Staff đều xem được.  
Mặc định chỉ trả bàn IsActive \= true.  
currentSessionId/orderCount lấy từ Member 4\.  
billCount/unpaidAmount lấy từ Member 5\.  
Nếu chưa tích hợp Member 4/5, các field summary có thể để null/0 tạm thời.  
\`\`\`

\---

**\#\# 11.4. Internal API / Internal Service**

**\#\#\# PUT \`/api/internal/tables/{id}/status\`**

Endpoint/service nội bộ, không cho WPF gọi trực tiếp.

Request:

\`\`\`json  
{  
  "status": "Occupied",  
  "tableSessionId": 1001,  
  "reason": "Session created from first accepted order"  
}  
\`\`\`

Rules:

\`\`\`text  
Chỉ module nghiệp vụ gọi.  
Status phải hợp lệ.  
Không cập nhật status tùy tiện từ UI.  
Cần đảm bảo đồng bộ với TableSession owner là Member 4\.  
\`\`\`

\---

**\#\# 12\. WPF Contracts**

**\#\# 12.1. \`TableManagementWindow\`**

Owner: Member 2    
Quyền: Admin

Chức năng:

\`\`\`text  
Xem danh sách bàn.  
Tạo bàn.  
Sửa tên bàn/khu vực.  
Ẩn bàn.  
Bật lại bàn.  
Reset QR.  
Copy QR URL.  
Hiển thị QR token/QR URL.  
Có thể export/in QR nếu nhóm kịp làm.  
\`\`\`

UI fields:

\`\`\`text  
TableName textbox  
Area textbox/combobox  
IsActive checkbox  
QrToken readonly textbox  
QrUrl readonly textbox  
Status readonly display  
DataGrid danh sách bàn  
\`\`\`

Buttons:

\`\`\`text  
Create  
Update  
Deactivate  
Reactivate  
Reset QR  
Copy QR URL  
Clear Form  
Search  
Reload  
\`\`\`

Rules UI:

\`\`\`text  
Staff không mở được màn này.  
Status readonly, không sửa bằng combobox.  
QrToken readonly, không nhập tay.  
Reset QR phải có confirm.  
Deactivate bàn Occupied/WaitingPayment phải báo lỗi.  
WPF chỉ gọi TableService/TableQrService.  
WPF có try-catch khi gọi Service.  
\`\`\`

\---

**\#\# 12.2. \`TableOverviewWindow\` hoặc \`TableStatusWindow\`**

Owner: Member 2    
Quyền: Admin/Staff

Chức năng:

\`\`\`text  
Xem trạng thái tất cả bàn đang active.  
Lọc theo khu vực.  
Lọc theo trạng thái.  
Search theo tên bàn.  
Click bàn để mở session detail của Member 4\.  
\`\`\`

UI hiển thị:

\`\`\`text  
TableName  
Area  
Status  
CurrentSessionId nếu có  
StartedAt nếu có  
OrderCount nếu có  
BillCount nếu có  
UnpaidAmount nếu có  
\`\`\`

Rules UI:

\`\`\`text  
Available: hiển thị bàn trống.  
Occupied: hiển thị bàn đang phục vụ.  
WaitingPayment: hiển thị bàn chờ thanh toán.  
Inactive: mặc định không hiển thị với Staff.  
Staff chỉ xem, không sửa table metadata.  
\`\`\`

Click behavior:

\`\`\`text  
Nếu bàn Available:  
    Có thể hiện thông báo: Bàn chưa có phiên phục vụ.

Nếu bàn Occupied/WaitingPayment:  
    Gọi contract của Member 4 để mở TableSessionDetailWindow.  
\`\`\`

\---

**\#\# 12.3. \`QrPreviewDialog\`**

Owner: Member 2    
Quyền: Admin

Chức năng:

\`\`\`text  
Hiển thị QR URL.  
Hiển thị QR image nếu có thư viện QR.  
Copy URL.  
Export ảnh QR nếu kịp.  
\`\`\`

Không bắt buộc trong MVP nếu chưa làm QR image. Tối thiểu phải có \`QrUrl\` để copy/test.

\---

**\#\# 13\. Permission Contract**

**\#\#\# 13.1. Admin permissions**

\`\`\`text  
TABLE\_VIEW  
TABLE\_CREATE  
TABLE\_UPDATE  
TABLE\_DEACTIVATE  
TABLE\_REACTIVATE  
TABLE\_RESET\_QR  
TABLE\_VIEW\_QR  
TABLE\_COPY\_QR\_URL  
\`\`\`

**\#\#\# 13.2. Staff permissions**

\`\`\`text  
TABLE\_VIEW  
TABLE\_STATUS\_VIEW  
\`\`\`

Staff không có:

\`\`\`text  
TABLE\_CREATE  
TABLE\_UPDATE  
TABLE\_DEACTIVATE  
TABLE\_REACTIVATE  
TABLE\_RESET\_QR  
\`\`\`

**\#\#\# 13.3. Permission matrix**

| Chức năng | Admin | Staff |  
|---|---:|---:|  
| Xem danh sách bàn | Có | Có |  
| Xem trạng thái bàn | Có | Có |  
| Tạo bàn | Có | Không |  
| Sửa tên/khu vực bàn | Có | Không |  
| Ẩn bàn | Có | Không |  
| Bật lại bàn | Có | Không |  
| Reset QR | Có | Không |  
| Copy QR URL | Có | Không hoặc tùy nhóm |  
| Mở session detail | Có | Có |

\---

**\#\# 14\. Error Contract**

| Error Code | Message | Khi nào xảy ra |  
|---|---|---|  
| \`TABLE\_NOT\_FOUND\` | Không tìm thấy bàn. | \`tableId\` không tồn tại |  
| \`TABLE\_NAME\_REQUIRED\` | Tên bàn không được để trống. | Create/update table |  
| \`TABLE\_NAME\_TOO\_LONG\` | Tên bàn không được vượt quá 50 ký tự. | Create/update table |  
| \`TABLE\_AREA\_TOO\_LONG\` | Khu vực không được vượt quá 50 ký tự. | Create/update table |  
| \`TABLE\_DUPLICATED\` | Bàn đã tồn tại trong khu vực này. | Nếu áp dụng unique Area \+ TableName |  
| \`TABLE\_INACTIVE\` | Bàn hiện không còn hoạt động. | Validate cho customer order |  
| \`TABLE\_HAS\_ACTIVE\_SESSION\` | Không thể ẩn bàn đang có phiên phục vụ. | Deactivate table |  
| \`INVALID\_TABLE\_STATUS\` | Trạng thái bàn không hợp lệ. | Update status internal |  
| \`QR\_TOKEN\_INVALID\` | QR không hợp lệ hoặc đã hết hiệu lực. | Customer scan QR |  
| \`QR\_TOKEN\_DUPLICATED\` | QR token bị trùng, vui lòng thử lại. | Generate token bị trùng |  
| \`QR\_RESET\_BLOCKED\_ACTIVE\_TABLE\` | Không thể reset QR khi bàn đang phục vụ hoặc chờ thanh toán. | Reset QR |  
| \`PERMISSION\_DENIED\` | Bạn không có quyền thực hiện chức năng này. | Staff gọi API Admin |  
| \`CONFIG\_CUSTOMER\_WEB\_BASE\_URL\_MISSING\` | Chưa cấu hình CustomerWebBaseUrl. | Generate QR URL |

\---

**\#\# 15\. Integration Contract với các Member khác**

**\#\# 15.1. Với Member 1 — Auth / Permission**

Member 2 cần dùng:

\`\`\`text  
CurrentUserId  
CurrentRole  
RequireAdmin()  
HasPermission(permissionKey)  
\`\`\`

Dùng ở:

\`\`\`text  
CreateTable  
UpdateTable  
DeactivateTable  
ReactivateTable  
ResetQrToken  
\`\`\`

\---

**\#\# 15.2. Với Member 4 — Session / Order / Print**

Member 4 cần gọi Member 2:

\`\`\`text  
ValidateQrToken(qrToken)  
GetTableByQrToken(qrToken)  
EnsureTableCanServe(tableId)  
SetTableOccupied(tableId, tableSessionId)  
SetTableWaitingPayment(tableId, tableSessionId)  
SetTableAvailable(tableId, tableSessionId)  
\`\`\`

Flow order đầu tiên:

\`\`\`text  
Customer gửi order  
→ Member 4 nhận request  
→ Member 4 gọi Member 2 validate QR/table  
→ Member 4 tạo/lấy TableSession  
→ Nếu tạo session mới, Member 4 gọi Member 2 set table Occupied  
→ Member 4 tạo order/order items  
\`\`\`

Rule phối hợp:

\`\`\`text  
Member 2 không tạo session.  
Member 2 chỉ validate bàn và cập nhật trạng thái bàn theo lệnh nghiệp vụ hợp lệ.  
Member 4 là owner của TableSession.  
\`\`\`

\---

**\#\# 15.3. Với Member 5 — Bill / Payment / Dashboard**

Member 5 có thể cần Member 2 cho dashboard/table status:

\`\`\`text  
GetTablesByStatus(Occupied)  
GetTablesByStatus(WaitingPayment)  
SetTableAvailable(tableId, tableSessionId) thông qua close session flow của Member 4  
\`\`\`

Flow thanh toán xong:

\`\`\`text  
Member 5 confirm payment  
→ Member 5 kiểm tra còn bill unpaid không  
→ Member 5 gọi Member 4 CloseSessionIfCompleted  
→ Member 4 đóng session  
→ Member 4 gọi Member 2 SetTableAvailable  
\`\`\`

Rule:

\`\`\`text  
Member 5 không nên gọi trực tiếp SetTableAvailable nếu session owner là Member 4\.  
Đóng session phải đi qua Member 4 để đồng bộ TableSession và DiningTables.  
\`\`\`

\---

**\#\# 16\. Realtime Contract**

Member 2 phát hoặc hỗ trợ phát event khi trạng thái bàn thay đổi.

**\#\#\# 16.1. Event \`TableStatusChanged\`**

Khi gửi:

\`\`\`text  
Tạo session mới làm bàn Occupied.  
Payment request làm bàn WaitingPayment.  
Close session làm bàn Available.  
Admin deactivate/reactivate bàn.  
\`\`\`

Payload:

\`\`\`json  
{  
  "tableId": 1,  
  "tableName": "Bàn 1",  
  "oldStatus": "Available",  
  "newStatus": "Occupied",  
  "tableSessionId": 1001,  
  "changedAt": "2026-06-11T10:00:00"  
}  
\`\`\`

Group nhận:

\`\`\`text  
staff  
table-{tableId}  
\`\`\`

**\#\#\# 16.2. Event \`QrTokenReset\`**

Chỉ gửi cho staff/admin nếu cần realtime.

Payload:

\`\`\`json  
{  
  "tableId": 1,  
  "tableName": "Bàn 1",  
  "qrResetAt": "2026-06-11T10:00:00"  
}  
\`\`\`

Không gửi token mới cho customer cũ.

\---

**\#\# 17\. Acceptance Criteria**

**\#\# 17.1. Table management**

\`\`\`text  
\[ \] Admin tạo bàn thành công.  
\[ \] Bàn mới có Status \= Available.  
\[ \] Bàn mới có IsActive \= true.  
\[ \] Bàn mới tự sinh QrToken.  
\[ \] QrToken không trùng.  
\[ \] Admin sửa được TableName và Area.  
\[ \] Admin ẩn được bàn Available.  
\[ \] Admin không ẩn được bàn Occupied/WaitingPayment.  
\[ \] Admin bật lại được bàn inactive.  
\[ \] Staff không tạo/sửa/ẩn bàn được.  
\[ \] Không xóa cứng bàn đã có dữ liệu.  
\`\`\`

**\#\# 17.2. QR**

\`\`\`text  
\[ \] QR URL được generate từ CustomerWebBaseUrl và QrToken.  
\[ \] QR URL không chứa TableId trực tiếp.  
\[ \] Customer Web validate token hợp lệ trả đúng bàn.  
\[ \] Token sai trả invalid.  
\[ \] Bàn inactive trả canOrder=false.  
\[ \] Bàn Available trả canOrder=true.  
\[ \] Bàn Occupied trả canOrder=true.  
\[ \] Bàn WaitingPayment trả canOrder=false theo rule MVP.  
\[ \] Reset QR tạo token mới.  
\[ \] Token cũ mất hiệu lực.  
\[ \] Không reset QR khi bàn đang Occupied/WaitingPayment.  
\`\`\`

**\#\# 17.3. Table status overview**

\`\`\`text  
\[ \] Staff/Admin xem được danh sách bàn active.  
\[ \] Lọc được theo Area.  
\[ \] Lọc được theo Status.  
\[ \] Search được theo TableName.  
\[ \] Status hiển thị đúng Available/Occupied/WaitingPayment.  
\[ \] Click bàn Occupied/WaitingPayment mở session detail qua Member 4\.  
\[ \] Click bàn Available báo chưa có session hoặc cho thao tác phù hợp.  
\`\`\`

**\#\# 17.4. Layer compliance**

\`\`\`text  
\[ \] WPF chỉ gọi Service.  
\[ \] Service validate nghiệp vụ.  
\[ \] Repository chỉ gọi DAO.  
\[ \] DAO chỉ query DB.  
\[ \] Service không MessageBox.  
\[ \] WPF có try-catch khi gọi Service.  
\[ \] Không hard-code CustomerWebBaseUrl nhiều nơi.  
\`\`\`

\---

**\#\# 18\. Deliverables của Member 2**

**\#\#\# 18.1. BusinessObjects**

\`\`\`text  
BusinessObjects/Tables/DiningTable.cs  
BusinessObjects/Tables/DiningTableDto.cs  
BusinessObjects/Tables/CreateTableRequest.cs  
BusinessObjects/Tables/UpdateTableRequest.cs  
BusinessObjects/Tables/TableQrDto.cs  
BusinessObjects/Tables/ValidateQrTokenResponse.cs  
BusinessObjects/Tables/TableStatusSummaryDto.cs  
BusinessObjects/Tables/TableStatus.cs  
\`\`\`

**\#\#\# 18.2. DAO**

\`\`\`text  
DataAccessObjects/Tables/DiningTableDAO.cs  
\`\`\`

DAO method cần có:

\`\`\`text  
GetAll  
GetActive  
GetById  
GetByQrToken  
Search  
Add  
Update  
SetActive  
UpdateStatus  
IsQrTokenExists  
IsTableNameExistsInArea nếu áp dụng  
\`\`\`

**\#\#\# 18.3. Repository**

\`\`\`text  
Repositories/Tables/IDiningTableRepository.cs  
Repositories/Tables/DiningTableRepository.cs  
\`\`\`

**\#\#\# 18.4. Services**

\`\`\`text  
Services/Tables/ITableService.cs  
Services/Tables/TableService.cs  
Services/Tables/ITableQrService.cs  
Services/Tables/TableQrService.cs  
Services/Tables/ITableReadService.cs  
Services/Tables/TableReadService.cs  
Services/Tables/ITableStatusPort.cs  
Services/Tables/TableStatusPort.cs  
\`\`\`

**\#\#\# 18.5. WPF**

\`\`\`text  
WpfStaff/Views/TableManagementWindow.xaml  
WpfStaff/Views/TableManagementWindow.xaml.cs  
WpfStaff/Views/TableOverviewWindow.xaml  
WpfStaff/Views/TableOverviewWindow.xaml.cs  
WpfStaff/Views/QrPreviewDialog.xaml nếu kịp  
\`\`\`

**\#\#\# 18.6. API**

\`\`\`text  
GET  /api/customer/tables/by-token/{token}  
GET  /api/admin/tables  
GET  /api/admin/tables/{id}  
POST /api/admin/tables  
PUT  /api/admin/tables/{id}  
PUT  /api/admin/tables/{id}/deactivate  
PUT  /api/admin/tables/{id}/reactivate  
PUT  /api/admin/tables/{id}/reset-qr  
GET  /api/admin/tables/{id}/qr  
GET  /api/staff/tables/status  
PUT  /api/internal/tables/{id}/status  
\`\`\`

\---

**\#\# 19\. Branch và commit**

Branch đề xuất:

\`\`\`text  
feature/table-qr-management  
\`\`\`

Commit examples:

\`\`\`text  
feat: add dining table entity and contracts  
feat: add table qr validation service  
feat: add table management window  
fix: block qr reset for occupied table  
ui: add table status overview window  
\`\`\`

\---

**\#\# 20\. Definition of Done cho Member 2**

Module Member 2 chỉ tính là xong khi:

\`\`\`text  
\[ \] Có bảng DiningTables đúng constraint.  
\[ \] Có entity/DTO/status constants đầy đủ.  
\[ \] Có DAO đầy đủ CRUD/query.  
\[ \] Có repository interface \+ implementation.  
\[ \] Có service validate business rule.  
\[ \] Có QR token generation.  
\[ \] Có QR URL generation từ config.  
\[ \] Có validate QR token API/service.  
\[ \] Có TableManagementWindow cho Admin.  
\[ \] Có TableOverviewWindow cho Admin/Staff.  
\[ \] Staff không dùng được chức năng Admin.  
\[ \] Không reset QR khi bàn đang phục vụ.  
\[ \] Không inactive bàn đang phục vụ.  
\[ \] WPF không gọi DAO/DbContext.  
\[ \] Service không MessageBox.  
\[ \] Có error contract chuẩn.  
\[ \] Có acceptance test cơ bản.  
\[ \] Demo được: tạo bàn → copy QR URL → validate token → reset QR → token cũ invalid.  
\`\`\`

\---

**\#\# 21\. Demo flow bắt buộc**

**\#\#\# Demo 1: Admin tạo bàn**

\`\`\`text  
Admin login  
→ Mở TableManagementWindow  
→ Nhập TableName \= Bàn 10  
→ Area \= Tầng 1  
→ Create  
→ Hệ thống sinh QrToken  
→ Status \= Available  
→ IsActive \= true  
→ Hiển thị QrUrl  
\`\`\`

**\#\#\# Demo 2: Customer Web validate QR**

\`\`\`text  
Copy QrUrl  
→ Gọi /api/customer/tables/by-token/{token}  
→ Trả đúng TableName, Area, Status  
→ canOrder \= true nếu bàn active và không WaitingPayment  
\`\`\`

**\#\#\# Demo 3: Reset QR**

\`\`\`text  
Admin chọn bàn Available  
→ Reset QR  
→ Token mới sinh ra  
→ QR URL mới thay đổi  
→ Token cũ validate thất bại  
\`\`\`

**\#\#\# Demo 4: Staff xem trạng thái bàn**

\`\`\`text  
Staff login  
→ Mở TableOverviewWindow  
→ Thấy danh sách bàn active  
→ Không thấy nút Create/Update/Reset QR  
→ Có thể lọc theo Status/Area  
\`\`\`

\---

**\#\# 22\. Tóm tắt trách nhiệm Member 2**

\`\`\`text  
Member 2 \= Table & QR Management

Làm tốt:  
\- DiningTables sạch.  
\- QR token an toàn và unique.  
\- QR URL đúng config.  
\- Admin quản lý bàn được.  
\- Staff xem trạng thái bàn được.  
\- Customer Web xác thực QR được.  
\- Member 4 có contract validate table và update status.

Không làm:  
\- Không tạo TableSession.  
\- Không xử lý order.  
\- Không xử lý bill/payment.  
\`\`\`

# Thành

**\# Member 3 — Menu, Category & Stock Management**

\> **\*\*Dự án:\*\*** QR Food Ordering Management System    
\> **\*\*Member phụ trách:\*\*** Member 3    
\> **\*\*Module:\*\*** Menu / Category / Stock    
\> **\*\*Loại tài liệu:\*\*** Business Rules, Contracts, API, WPF Contracts    
\> **\*\*Kiến trúc áp dụng:\*\*** Feature-based WPF App    
\> **\*\*Phạm vi:\*\*** WPF Staff App \+ ASP.NET Core API \+ SQL Server  

\---

**\# 1\. Mục tiêu module**

Member 3 chịu trách nhiệm toàn bộ nghiệp vụ liên quan đến **\*\*dữ liệu menu\*\***, bao gồm:

\`\`\`text  
Category  
MenuItem  
Stock  
Availability  
Sold out  
Reopen item  
Customer menu data  
Menu data contract cho Order module  
\`\`\`

Module này là **\*\*dữ liệu gốc\*\*** cho nghiệp vụ order. Khách chỉ được order các món hợp lệ từ \`MenuItems\`. Khi order được tạo, module Order sẽ lấy snapshot \`ItemName\`, \`UnitPrice\`, \`TotalPrice\` từ dữ liệu của module Menu/Stock.

\---

**\# 2\. Ranh giới module**

**\#\# 2.1. Member 3 phụ trách**

\`\`\`text  
Categories  
MenuItems  
Menu display rules  
Stock display rules  
Admin menu management  
Staff stock management  
Availability management  
Sold-out / reopen item  
Menu contract cho Customer Web  
Menu contract cho Order module  
\`\`\`

**\#\# 2.2. Member 3 không phụ trách**

\`\`\`text  
Không tạo Order.  
Không tạo OrderItems.  
Không tạo TableSession.  
Không tạo BillDetails.  
Không xử lý split bill.  
Không xác nhận payment.  
Không tính doanh thu.  
Không xử lý in order.  
\`\`\`

**\#\# 2.3. Điểm quan trọng**

Member 3 **\*\*quản lý trạng thái món và tồn kho\*\***, nhưng thao tác **\*\*trừ stock khi khách order\*\*** phải được thực hiện trong transaction của flow Order.

Vì vậy Member 3 cần cung cấp contract nội bộ cho Member 4:

\`\`\`text  
ValidateOrderableItems  
ReserveStockForOrder  
RollbackStockForCancelledOrder  
GetMenuItemSnapshot  
\`\`\`

\---

**\# 3\. Bảng dữ liệu phụ trách**

**\#\# 3.1. Categories**

Bảng \`Categories\` lưu loại món.

| Field | Type | Required | Ý nghĩa |  
|---|---|---:|---|  
| \`CategoryId\` | int | Yes | Khóa chính category |  
| \`CategoryName\` | string | Yes | Tên loại món |  
| \`Description\` | string | No | Mô tả loại món |  
| \`DisplayOrder\` | int | Yes | Thứ tự hiển thị |  
| \`IsActive\` | bool | Yes | Category còn hiển thị hay đã ẩn |  
| \`CreatedAt\` | datetime | Yes | Ngày tạo |  
| \`UpdatedAt\` | datetime? | No | Ngày cập nhật gần nhất |

**\#\# 3.2. MenuItems**

Bảng \`MenuItems\` lưu món ăn/đồ uống.

| Field | Type | Required | Ý nghĩa |  
|---|---|---:|---|  
| \`MenuItemId\` | int | Yes | Khóa chính món |  
| \`CategoryId\` | int | Yes | FK tới \`Categories\` |  
| \`ItemName\` | string | Yes | Tên món |  
| \`Description\` | string | No | Mô tả món |  
| \`Price\` | decimal | Yes | Giá hiện tại |  
| \`ImageUrl\` | string | No | Ảnh món |  
| \`IsActive\` | bool | Yes | Món còn nằm trong menu vận hành |  
| \`IsAvailable\` | bool | Yes | Món hiện đang bán được |  
| \`TrackStock\` | bool | Yes | Có quản lý số lượng tồn hay không |  
| \`AvailableQuantity\` | int? | Conditional | Số lượng còn lại nếu \`TrackStock \= true\` |  
| \`RowVersion\` | binary | Yes | Chống ghi đè khi nhiều request cùng cập nhật stock |  
| \`CreatedAt\` | datetime | Yes | Ngày tạo |  
| \`UpdatedAt\` | datetime? | No | Ngày cập nhật gần nhất |

\---

**\# 4\. Database constraints bắt buộc**

**\#\# 4.1. Categories**

\`\`\`text  
PK: CategoryId  
CHECK: DisplayOrder \>= 0  
\`\`\`

Khuyến nghị thêm:

\`\`\`text  
CategoryName NOT NULL  
CategoryName length \<= 100  
IsActive default \= true  
CreatedAt default \= current datetime  
\`\`\`

**\#\# 4.2. MenuItems**

\`\`\`text  
PK: MenuItemId  
FK: CategoryId \-\> Categories.CategoryId  
CHECK: Price \>= 0  
CHECK: TrackStock \= false OR AvailableQuantity IS NOT NULL  
CHECK: TrackStock \= false OR AvailableQuantity \>= 0  
CHECK: TrackStock \= false OR AvailableQuantity \> 0 OR IsAvailable \= false  
\`\`\`

Khuyến nghị thêm:

\`\`\`text  
ItemName NOT NULL  
ItemName length \<= 150  
Price decimal(18,2)  
IsActive default \= true  
IsAvailable default \= true  
TrackStock default \= false  
RowVersion dùng concurrency token  
\`\`\`

**\#\# 4.3. Index khuyến nghị**

\`\`\`text  
IX\_MenuItems\_CategoryId\_IsActive\_IsAvailable  
IX\_MenuItems\_ItemName  
IX\_Categories\_DisplayOrder\_IsActive  
\`\`\`

Mục đích:

\`\`\`text  
Tải menu theo category nhanh hơn.  
Lọc món đang bán nhanh hơn.  
Search món trong WPF nhanh hơn.  
Hiển thị Customer Web menu nhanh hơn.  
\`\`\`

\---

**\# 5\. Quyền thao tác**

**\#\# 5.1. Admin**

Admin được:

\`\`\`text  
Tạo category.  
Sửa category.  
Ẩn category.  
Tạo món.  
Sửa thông tin gốc của món.  
Sửa giá món.  
Ẩn món.  
Cập nhật stock.  
Bật/tắt món đang bán.  
Đánh dấu hết món.  
Bán lại món.  
\`\`\`

**\#\# 5.2. Staff**

Staff được:

\`\`\`text  
Xem danh sách category.  
Xem danh sách món.  
Cập nhật stock.  
Bật/tắt món đang bán.  
Đánh dấu hết món.  
Bán lại món nếu còn stock.  
\`\`\`

Staff không được:

\`\`\`text  
Tạo category.  
Sửa category.  
Ẩn category.  
Tạo món mới.  
Sửa tên món.  
Sửa giá món.  
Sửa mô tả món.  
Sửa category của món.  
Ẩn món khỏi hệ thống.  
\`\`\`

\---

**\# 6\. Business Rules — Category**

**\#\# BR-CAT-001: CategoryName bắt buộc**

\`CategoryName\` không được rỗng hoặc toàn khoảng trắng.

Thông báo lỗi chuẩn:

\`\`\`text  
Tên loại món không được để trống.  
\`\`\`

**\#\# BR-CAT-002: DisplayOrder không âm**

\`DisplayOrder \>= 0\`.

Thông báo lỗi chuẩn:

\`\`\`text  
Thứ tự hiển thị không được âm.  
\`\`\`

**\#\# BR-CAT-003: Category active mới hiển thị trên Customer Web**

Customer Web chỉ hiển thị category có:

\`\`\`text  
Categories.IsActive \= true  
\`\`\`

**\#\# BR-CAT-004: Ẩn category không xóa món lịch sử**

Không được xóa cứng category nếu đã có món/order/bill lịch sử.

Khi không dùng nữa:

\`\`\`text  
IsActive \= false  
\`\`\`

**\#\# BR-CAT-005: Category inactive không cho thêm món mới**

Không được tạo món mới vào category đã inactive.

Thông báo lỗi chuẩn:

\`\`\`text  
Không thể thêm món vào loại món đã bị ẩn.  
\`\`\`

**\#\# BR-CAT-006: Khi category inactive, Customer Web không hiển thị category đó**

Nếu category inactive, Customer Web không hiển thị category và cũng không hiển thị món thuộc category đó.

**\#\# BR-CAT-007: Không tự động xóa món khi ẩn category**

Ẩn category không làm mất dữ liệu món.

Rule đề xuất:

\`\`\`text  
Ẩn category chỉ ẩn nhóm trên Customer Web.  
Các món thuộc category đó không hiển thị trên Customer Web.  
WPF Admin vẫn xem được để quản lý/lịch sử.  
\`\`\`

\---

**\# 7\. Business Rules — MenuItem**

**\#\# BR-ITEM-001: ItemName bắt buộc**

\`ItemName\` không được rỗng hoặc toàn khoảng trắng.

Thông báo lỗi chuẩn:

\`\`\`text  
Tên món không được để trống.  
\`\`\`

**\#\# BR-ITEM-002: Price không âm**

\`Price \>= 0\`.

Thông báo lỗi chuẩn:

\`\`\`text  
Giá món không được âm.  
\`\`\`

**\#\# BR-ITEM-003: CategoryId phải hợp lệ**

Khi tạo/sửa món, \`CategoryId\` phải tồn tại và category phải active nếu đang tạo món mới.

Thông báo lỗi chuẩn:

\`\`\`text  
Loại món không hợp lệ.  
\`\`\`

**\#\# BR-ITEM-004: IsActive biểu diễn món còn nằm trong menu vận hành**

\`\`\`text  
IsActive \= true  \-\> món còn thuộc menu vận hành  
IsActive \= false \-\> món đã bị ẩn khỏi menu vận hành  
\`\`\`

Món inactive:

\`\`\`text  
Không hiển thị trên Customer Web.  
Không được thêm vào giỏ.  
Không được order mới.  
Vẫn giữ trong order/bill lịch sử nhờ snapshot.  
\`\`\`

**\#\# BR-ITEM-005: IsAvailable biểu diễn món đang bán được**

\`\`\`text  
IsAvailable \= true  \-\> khách được thêm món vào giỏ  
IsAvailable \= false \-\> khách không được thêm món vào giỏ  
\`\`\`

Món unavailable có thể vẫn hiển thị với trạng thái:

\`\`\`text  
Hết món  
Tạm ngưng bán  
\`\`\`

**\#\# BR-ITEM-006: TrackStock xác định có quản lý số lượng tồn**

\`\`\`text  
TrackStock \= true  \-\> hệ thống kiểm tra AvailableQuantity khi order  
TrackStock \= false \-\> hệ thống không kiểm tra số lượng tồn  
\`\`\`

**\#\# BR-ITEM-007: AvailableQuantity bắt buộc khi TrackStock \= true**

Nếu \`TrackStock \= true\`:

\`\`\`text  
AvailableQuantity IS NOT NULL  
AvailableQuantity \>= 0  
\`\`\`

Nếu \`TrackStock \= false\`:

\`\`\`text  
AvailableQuantity có thể null hoặc 0 theo thống nhất của nhóm  
Không dùng AvailableQuantity để chặn order  
\`\`\`

**\#\# BR-ITEM-008: AvailableQuantity \= 0 thì IsAvailable \= false**

Nếu món có \`TrackStock \= true\` và số lượng còn lại bằng 0:

\`\`\`text  
IsAvailable phải tự động chuyển false  
\`\`\`

Thông báo hoặc trạng thái hiển thị:

\`\`\`text  
Hết món  
\`\`\`

**\#\# BR-ITEM-009: Không cho bật bán lại nếu hết stock**

Nếu:

\`\`\`text  
TrackStock \= true  
AvailableQuantity \<= 0  
\`\`\`

thì không được set:

\`\`\`text  
IsAvailable \= true  
\`\`\`

Thông báo lỗi chuẩn:

\`\`\`text  
Không thể bán lại món đã hết số lượng.  
\`\`\`

**\#\# BR-ITEM-010: Admin sửa giá không ảnh hưởng order/bill cũ**

Khi Admin sửa \`MenuItems.Price\`, các order/bill đã phát sinh không bị thay đổi.

Lý do:

\`\`\`text  
OrderItems lưu snapshot ItemName, UnitPrice, TotalPrice.  
BillDetails lưu snapshot ItemName, UnitPrice, TotalPrice.  
\`\`\`

**\#\# BR-ITEM-011: Admin sửa tên món không ảnh hưởng order/bill cũ**

Khi Admin sửa \`MenuItems.ItemName\`, lịch sử order/bill vẫn hiển thị tên món theo snapshot lúc order/bill được tạo.

**\#\# BR-ITEM-012: Ẩn món là soft delete**

Không xóa cứng món đã từng phát sinh order/bill.

Khi ẩn món:

\`\`\`text  
IsActive \= false  
IsAvailable \= false  
UpdatedAt \= now  
\`\`\`

**\#\# BR-ITEM-013: Customer Web chỉ hiển thị món đủ điều kiện**

Customer Web chỉ lấy món có:

\`\`\`text  
Category.IsActive \= true  
MenuItem.IsActive \= true  
\`\`\`

Nếu:

\`\`\`text  
MenuItem.IsAvailable \= true  
\`\`\`

thì cho thêm vào giỏ.

Nếu:

\`\`\`text  
MenuItem.IsAvailable \= false  
\`\`\`

thì hiển thị trạng thái hết món/tạm ngưng bán và không cho thêm vào giỏ.

\---

**\# 8\. Business Rules — Stock**

**\#\# BR-STOCK-001: Stock chỉ áp dụng với món TrackStock**

Chỉ món có:

\`\`\`text  
TrackStock \= true  
\`\`\`

mới cần kiểm tra/trừ/cộng lại stock.

**\#\# BR-STOCK-002: AvailableQuantity luôn không âm**

Không được để:

\`\`\`text  
AvailableQuantity \< 0  
\`\`\`

Thông báo lỗi chuẩn:

\`\`\`text  
Số lượng tồn không được âm.  
\`\`\`

**\#\# BR-STOCK-003: Cập nhật stock thủ công**

Admin/Staff được cập nhật stock.

Input:

\`\`\`text  
MenuItemId  
NewQuantity  
\`\`\`

Rule:

\`\`\`text  
NewQuantity \>= 0  
MenuItem phải tồn tại  
MenuItem.TrackStock phải true  
Nếu NewQuantity \= 0 \-\> IsAvailable \= false  
Nếu NewQuantity \> 0 \-\> không tự động bật IsAvailable nếu trước đó bị tạm ngưng bán, trừ khi user chọn bán lại  
\`\`\`

**\#\# BR-STOCK-004: Đánh dấu hết món**

Khi nhân viên bấm \`Đánh dấu hết\`:

\`\`\`text  
IsAvailable \= false  
Nếu TrackStock \= true thì AvailableQuantity có thể set \= 0 theo rule MVP  
UpdatedAt \= now  
\`\`\`

**\#\# BR-STOCK-005: Bán lại món**

Khi nhân viên bấm \`Bán lại\`:

Điều kiện:

\`\`\`text  
MenuItem.IsActive \= true  
Nếu TrackStock \= true thì AvailableQuantity \> 0  
\`\`\`

Kết quả:

\`\`\`text  
IsAvailable \= true  
UpdatedAt \= now  
\`\`\`

**\#\# BR-STOCK-006: Trừ stock khi order accepted**

Khi order hợp lệ được accepted, module Order gọi contract của module Menu/Stock để trừ stock.

Rule:

\`\`\`text  
Chỉ trừ stock với món TrackStock \= true  
Quantity order phải \<= AvailableQuantity  
AvailableQuantity sau khi trừ không được âm  
Nếu AvailableQuantity sau khi trừ \= 0 \-\> IsAvailable \= false  
Thao tác trừ stock phải nằm trong transaction tạo order  
\`\`\`

**\#\# BR-STOCK-007: Rollback stock khi cancel order**

Khi order bị cancel, module Order gọi contract của module Menu/Stock để cộng lại stock.

Rule:

\`\`\`text  
Chỉ cộng lại stock với món TrackStock \= true  
AvailableQuantity \+= OrderItem.Quantity  
Không tự động bật IsAvailable nếu món đang bị tạm ngưng bán thủ công, trừ khi nhóm thống nhất bật lại khi AvailableQuantity \> 0  
\`\`\`

Rule MVP đề xuất:

\`\`\`text  
Nếu món bị hết do stock \= 0 và sau rollback AvailableQuantity \> 0:  
    Có thể set IsAvailable \= true.  
Nếu món bị tắt bán thủ công:  
    Không tự động bật lại.  
\`\`\`

Để làm rõ hơn, có thể bổ sung field nâng cao sau MVP:

\`\`\`text  
UnavailableReason \= SoldOut | ManualDisabled  
\`\`\`

Trong MVP hiện tại, chỉ cần dùng \`IsAvailable\`.

**\#\# BR-STOCK-008: Concurrency bằng RowVersion**

Khi nhiều khách order cùng một món gần như đồng thời:

\`\`\`text  
Request A đọc AvailableQuantity \= 1  
Request B đọc AvailableQuantity \= 1  
Request A trừ còn 0 thành công  
Request B phải bị chặn hoặc retry và báo hết món  
\`\`\`

\`RowVersion\` dùng để chống ghi đè dữ liệu stock.

\---

**\# 9\. DTO Contracts**

**\#\# 9.1. CategoryDto**

Dùng cho WPF và API.

| Field | Type | Required | Ghi chú |  
|---|---|---:|---|  
| \`CategoryId\` | int | Yes | ID category |  
| \`CategoryName\` | string | Yes | Tên category |  
| \`Description\` | string? | No | Mô tả |  
| \`DisplayOrder\` | int | Yes | Thứ tự hiển thị |  
| \`IsActive\` | bool | Yes | Trạng thái active |  
| \`CreatedAt\` | datetime | Yes | Ngày tạo |  
| \`UpdatedAt\` | datetime? | No | Ngày cập nhật |  
| \`ItemCount\` | int | No | Số món thuộc category |

**\#\# 9.2. CreateCategoryRequest**

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`CategoryName\` | string | Yes | Không rỗng, tối đa 100 ký tự |  
| \`Description\` | string? | No | Tối đa 500 ký tự |  
| \`DisplayOrder\` | int | Yes | \>= 0 |

**\#\# 9.3. UpdateCategoryRequest**

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`CategoryName\` | string | Yes | Không rỗng |  
| \`Description\` | string? | No | Tối đa 500 ký tự |  
| \`DisplayOrder\` | int | Yes | \>= 0 |  
| \`IsActive\` | bool | Yes | true/false |

**\#\# 9.4. MenuItemDto**

| Field | Type | Required | Ghi chú |  
|---|---|---:|---|  
| \`MenuItemId\` | int | Yes | ID món |  
| \`CategoryId\` | int | Yes | ID category |  
| \`CategoryName\` | string | No | Tên category để hiển thị |  
| \`ItemName\` | string | Yes | Tên món |  
| \`Description\` | string? | No | Mô tả món |  
| \`Price\` | decimal | Yes | Giá hiện tại |  
| \`ImageUrl\` | string? | No | Ảnh món |  
| \`IsActive\` | bool | Yes | Còn trong menu vận hành |  
| \`IsAvailable\` | bool | Yes | Đang bán được |  
| \`TrackStock\` | bool | Yes | Có quản lý stock |  
| \`AvailableQuantity\` | int? | Conditional | Số lượng còn lại |  
| \`StatusText\` | string | No | Đang bán / Hết món / Tạm ngưng / Đã ẩn |  
| \`CreatedAt\` | datetime | Yes | Ngày tạo |  
| \`UpdatedAt\` | datetime? | No | Ngày cập nhật |

**\#\# 9.5. CreateMenuItemRequest**

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`CategoryId\` | int | Yes | Category active |  
| \`ItemName\` | string | Yes | Không rỗng |  
| \`Description\` | string? | No | Tối đa 1000 ký tự |  
| \`Price\` | decimal | Yes | \>= 0 |  
| \`ImageUrl\` | string? | No | URL hợp lệ nếu nhập |  
| \`IsAvailable\` | bool | Yes | Mặc định true nếu còn bán |  
| \`TrackStock\` | bool | Yes | true/false |  
| \`AvailableQuantity\` | int? | Conditional | Bắt buộc nếu TrackStock \= true |

**\#\# 9.6. UpdateMenuItemRequest**

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`CategoryId\` | int | Yes | Category hợp lệ |  
| \`ItemName\` | string | Yes | Không rỗng |  
| \`Description\` | string? | No | Mô tả |  
| \`Price\` | decimal | Yes | \>= 0 |  
| \`ImageUrl\` | string? | No | Ảnh |  
| \`IsActive\` | bool | Yes | Admin only |  
| \`IsAvailable\` | bool | Yes | Admin/Staff tùy action |  
| \`TrackStock\` | bool | Yes | Admin only nếu đổi logic quản lý stock |  
| \`AvailableQuantity\` | int? | Conditional | \>= 0 nếu TrackStock \= true |

**\#\# 9.7. UpdateStockRequest**

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`MenuItemId\` | int | Yes | Món tồn tại |  
| \`AvailableQuantity\` | int | Yes | \>= 0 |  
| \`Reason\` | string? | No | Lý do cập nhật, nếu nhóm muốn audit |

**\#\# 9.8. SetAvailabilityRequest**

| Field | Type | Required | Rule |  
|---|---|---:|---|  
| \`MenuItemId\` | int | Yes | Món tồn tại |  
| \`IsAvailable\` | bool | Yes | true/false |  
| \`Reason\` | string? | No | Lý do tắt/bật bán |

**\#\# 9.9. CustomerMenuDto**

Dùng cho Customer Web.

| Field | Type | Ghi chú |  
|---|---|---|  
| \`Categories\` | List CustomerCategoryDto | Danh sách category active |

**\#\#\# CustomerCategoryDto**

| Field | Type | Ghi chú |  
|---|---|---|  
| \`CategoryId\` | int | ID category |  
| \`CategoryName\` | string | Tên category |  
| \`DisplayOrder\` | int | Thứ tự hiển thị |  
| \`Items\` | List CustomerMenuItemDto | Món thuộc category |

**\#\#\# CustomerMenuItemDto**

| Field | Type | Ghi chú |  
|---|---|---|  
| \`MenuItemId\` | int | ID món |  
| \`ItemName\` | string | Tên món |  
| \`Description\` | string? | Mô tả |  
| \`Price\` | decimal | Giá hiện tại |  
| \`ImageUrl\` | string? | Ảnh |  
| \`IsAvailable\` | bool | Có được thêm vào giỏ không |  
| \`TrackStock\` | bool | Có quản lý stock không |  
| \`AvailableQuantity\` | int? | Có thể hiển thị số lượng còn lại nếu cần |  
| \`StatusText\` | string | Đang bán / Hết món / Tạm ngưng |

**\#\# 9.10. MenuItemSnapshotDto**

Dùng cho Member 4 khi tạo OrderItems.

| Field | Type | Required | Ghi chú |  
|---|---|---:|---|  
| \`MenuItemId\` | int | Yes | ID món |  
| \`ItemName\` | string | Yes | Snapshot tên món |  
| \`UnitPrice\` | decimal | Yes | Snapshot giá |  
| \`TrackStock\` | bool | Yes | Có cần trừ stock không |  
| \`AvailableQuantity\` | int? | No | Số lượng còn lại tại thời điểm kiểm tra |

\---

**\# 10\. Service Contracts**

**\#\# 10.1. ICategoryService**

| Method | Input | Output | Quyền |  
|---|---|---|---|  
| \`GetAllCategories\` | filter optional | List \`CategoryDto\` | Admin/Staff |  
| \`GetActiveCategories\` | none | List \`CategoryDto\` | Admin/Staff/Customer |  
| \`GetCategoryById\` | \`categoryId\` | \`CategoryDto\` | Admin/Staff |  
| \`CreateCategory\` | \`CreateCategoryRequest\` | \`CategoryDto\` | Admin |  
| \`UpdateCategory\` | \`categoryId\`, \`UpdateCategoryRequest\` | \`CategoryDto\` | Admin |  
| \`DeactivateCategory\` | \`categoryId\` | success/fail | Admin |  
| \`ReactivateCategory\` | \`categoryId\` | success/fail | Admin |  
| \`SearchCategories\` | \`keyword\` | List \`CategoryDto\` | Admin/Staff |

**\#\# 10.2. IMenuItemService**

| Method | Input | Output | Quyền |  
|---|---|---|---|  
| \`GetAllMenuItems\` | filter optional | List \`MenuItemDto\` | Admin/Staff |  
| \`GetMenuItemsByCategory\` | \`categoryId\` | List \`MenuItemDto\` | Admin/Staff |  
| \`GetMenuItemById\` | \`menuItemId\` | \`MenuItemDto\` | Admin/Staff |  
| \`SearchMenuItems\` | \`keyword\`, filters | List \`MenuItemDto\` | Admin/Staff |  
| \`CreateMenuItem\` | \`CreateMenuItemRequest\` | \`MenuItemDto\` | Admin |  
| \`UpdateMenuItem\` | \`menuItemId\`, \`UpdateMenuItemRequest\` | \`MenuItemDto\` | Admin |  
| \`HideMenuItem\` | \`menuItemId\` | success/fail | Admin |  
| \`SetAvailability\` | \`SetAvailabilityRequest\` | \`MenuItemDto\` | Admin/Staff |  
| \`MarkSoldOut\` | \`menuItemId\`, reason optional | \`MenuItemDto\` | Admin/Staff |  
| \`ReopenItem\` | \`menuItemId\` | \`MenuItemDto\` | Admin/Staff |

**\#\# 10.3. IStockService**

| Method | Input | Output | Dùng bởi |  
|---|---|---|---|  
| \`UpdateStock\` | \`UpdateStockRequest\` | \`MenuItemDto\` | WPF Admin/Staff |  
| \`ValidateStockForOrder\` | List order item request | validation result | Member 4 |  
| \`ReserveStockForOrder\` | List menu item \+ quantity | success/fail | Member 4 |  
| \`RollbackStockForCancelledOrder\` | List order item snapshot | success/fail | Member 4 |  
| \`GetStockStatus\` | \`menuItemId\` | stock status DTO | WPF/Order |

**\#\# 10.4. ICustomerMenuService**

| Method | Input | Output | Dùng bởi |  
|---|---|---|---|  
| \`GetCustomerMenu\` | none/filter | \`CustomerMenuDto\` | Customer Web |  
| \`GetAvailableMenuItems\` | optional category | List \`CustomerMenuItemDto\` | Customer Web |  
| \`GetMenuItemSnapshot\` | \`menuItemId\` | \`MenuItemSnapshotDto\` | Member 4 |

\---

**\# 11\. API Contracts**

**\#\# 11.1. Customer API — xem menu**

**\#\#\# GET \`/api/customer/menu\`**

Dùng cho Customer Web hiển thị menu.

Query optional:

\`\`\`text  
categoryId  
keyword  
\`\`\`

Response:

\`\`\`json  
{  
  "categories": \[  
    {  
      "categoryId": 1,  
      "categoryName": "Món chính",  
      "displayOrder": 1,  
      "items": \[  
        {  
          "menuItemId": 10,  
          "itemName": "Cơm gà",  
          "description": "Cơm gà đặc biệt",  
          "price": 45000,  
          "imageUrl": "/images/com-ga.jpg",  
          "isAvailable": true,  
          "trackStock": true,  
          "availableQuantity": 12,  
          "statusText": "Đang bán"  
        }  
      \]  
    }  
  \]  
}  
\`\`\`

Rule:

\`\`\`text  
Chỉ trả category IsActive \= true.  
Chỉ trả menu item IsActive \= true.  
Món IsAvailable \= false vẫn có thể trả về để hiển thị hết món/tạm ngưng bán.  
Không trả món inactive.  
Không cho Customer Web tự gửi ItemName/UnitPrice để ghi database.  
\`\`\`

\---

**\#\# 11.2. Admin API — quản lý category**

**\#\#\# GET \`/api/admin/categories\`**

Response:

\`\`\`json  
{  
  "items": \[  
    {  
      "categoryId": 1,  
      "categoryName": "Đồ uống",  
      "description": "Các loại nước uống",  
      "displayOrder": 1,  
      "isActive": true,  
      "itemCount": 8  
    }  
  \]  
}  
\`\`\`

**\#\#\# POST \`/api/admin/categories\`**

Request:

\`\`\`json  
{  
  "categoryName": "Món chính",  
  "description": "Các món chính trong menu",  
  "displayOrder": 1  
}  
\`\`\`

Response:

\`\`\`json  
{  
  "categoryId": 1,  
  "categoryName": "Món chính",  
  "description": "Các món chính trong menu",  
  "displayOrder": 1,  
  "isActive": true  
}  
\`\`\`

**\#\#\# PUT \`/api/admin/categories/{id}\`**

Request:

\`\`\`json  
{  
  "categoryName": "Món chính",  
  "description": "Món chính cập nhật",  
  "displayOrder": 2,  
  "isActive": true  
}  
\`\`\`

**\#\#\# PUT \`/api/admin/categories/{id}/hide\`**

Rule:

\`\`\`text  
Set IsActive \= false.  
Không xóa category.  
Không xóa món trong category.  
Customer Web không hiển thị category này.  
\`\`\`

**\#\#\# PUT \`/api/admin/categories/{id}/reactivate\`**

Rule:

\`\`\`text  
Set IsActive \= true.  
Category xuất hiện lại trên Customer Web nếu có món active.  
\`\`\`

\---

**\#\# 11.3. Admin API — quản lý món**

**\#\#\# GET \`/api/admin/menu-items\`**

Query optional:

\`\`\`text  
categoryId  
keyword  
isActive  
isAvailable  
trackStock  
\`\`\`

Response:

\`\`\`json  
{  
  "items": \[  
    {  
      "menuItemId": 10,  
      "categoryId": 1,  
      "categoryName": "Món chính",  
      "itemName": "Cơm gà",  
      "description": "Cơm gà đặc biệt",  
      "price": 45000,  
      "imageUrl": "/images/com-ga.jpg",  
      "isActive": true,  
      "isAvailable": true,  
      "trackStock": true,  
      "availableQuantity": 12,  
      "statusText": "Đang bán"  
    }  
  \]  
}  
\`\`\`

**\#\#\# POST \`/api/admin/menu-items\`**

Request:

\`\`\`json  
{  
  "categoryId": 1,  
  "itemName": "Cơm gà",  
  "description": "Cơm gà đặc biệt",  
  "price": 45000,  
  "imageUrl": "/images/com-ga.jpg",  
  "isAvailable": true,  
  "trackStock": true,  
  "availableQuantity": 20  
}  
\`\`\`

Rule:

\`\`\`text  
Admin only.  
ItemName bắt buộc.  
Price \>= 0\.  
CategoryId phải hợp lệ.  
Nếu TrackStock \= true, AvailableQuantity bắt buộc \>= 0\.  
Nếu AvailableQuantity \= 0, IsAvailable \= false.  
\`\`\`

**\#\#\# PUT \`/api/admin/menu-items/{id}\`**

Request:

\`\`\`json  
{  
  "categoryId": 1,  
  "itemName": "Cơm gà đặc biệt",  
  "description": "Cập nhật mô tả",  
  "price": 50000,  
  "imageUrl": "/images/com-ga-new.jpg",  
  "isActive": true,  
  "isAvailable": true,  
  "trackStock": true,  
  "availableQuantity": 15  
}  
\`\`\`

Rule:

\`\`\`text  
Admin only.  
Sửa tên/giá không ảnh hưởng order/bill cũ.  
Nếu set IsActive \= false thì IsAvailable phải false.  
\`\`\`

**\#\#\# PUT \`/api/admin/menu-items/{id}/hide\`**

Rule:

\`\`\`text  
Admin only.  
Set IsActive \= false.  
Set IsAvailable \= false.  
Không xóa dữ liệu.  
\`\`\`

\---

**\#\# 11.4. Staff/Admin API — stock & availability**

**\#\#\# PUT \`/api/menu-items/{id}/stock\`**

Request:

\`\`\`json  
{  
  "availableQuantity": 10,  
  "reason": "Nhập thêm hàng"  
}  
\`\`\`

Rule:

\`\`\`text  
Admin/Staff.  
Chỉ áp dụng TrackStock \= true.  
Quantity \>= 0\.  
Quantity \= 0 \-\> IsAvailable \= false.  
Quantity \> 0 \-\> không bắt buộc tự bật IsAvailable nếu đang tạm ngưng bán.  
\`\`\`

**\#\#\# PUT \`/api/menu-items/{id}/availability\`**

Request:

\`\`\`json  
{  
  "isAvailable": false,  
  "reason": "Tạm ngưng bán"  
}  
\`\`\`

Rule:

\`\`\`text  
Admin/Staff.  
Nếu bật IsAvailable \= true và TrackStock \= true thì AvailableQuantity phải \> 0\.  
Nếu IsActive \= false thì không được bật IsAvailable \= true.  
\`\`\`

**\#\#\# PUT \`/api/menu-items/{id}/sold-out\`**

Rule:

\`\`\`text  
Admin/Staff.  
Set IsAvailable \= false.  
Nếu TrackStock \= true thì có thể set AvailableQuantity \= 0 theo MVP rule.  
\`\`\`

Response:

\`\`\`json  
{  
  "menuItemId": 10,  
  "itemName": "Cơm gà",  
  "isAvailable": false,  
  "availableQuantity": 0,  
  "statusText": "Hết món"  
}  
\`\`\`

**\#\#\# PUT \`/api/menu-items/{id}/reopen\`**

Rule:

\`\`\`text  
Admin/Staff.  
MenuItem phải IsActive \= true.  
Nếu TrackStock \= true thì AvailableQuantity \> 0\.  
Set IsAvailable \= true.  
\`\`\`

\---

**\# 12\. Internal Contracts cho Member 4 — Order/Session**

**\#\# 12.1. Validate orderable items**

Member 4 gọi trước khi tạo order.

Input:

\`\`\`json  
{  
  "items": \[  
    {  
      "menuItemId": 10,  
      "quantity": 2  
    }  
  \]  
}  
\`\`\`

Output:

\`\`\`json  
{  
  "isValid": true,  
  "validItems": \[  
    {  
      "menuItemId": 10,  
      "itemName": "Cơm gà",  
      "quantity": 2,  
      "unitPrice": 45000,  
      "totalPrice": 90000,  
      "trackStock": true  
    }  
  \],  
  "invalidItems": \[\]  
}  
\`\`\`

Invalid example:

\`\`\`json  
{  
  "isValid": false,  
  "validItems": \[\],  
  "invalidItems": \[  
    {  
      "menuItemId": 10,  
      "reason": "Món đã hết hoặc tạm ngưng bán."  
    }  
  \]  
}  
\`\`\`

Rule:

\`\`\`text  
Món phải tồn tại.  
Món phải IsActive \= true.  
Món phải IsAvailable \= true.  
Category phải IsActive \= true.  
Quantity \> 0\.  
Nếu TrackStock \= true, AvailableQuantity phải đủ.  
\`\`\`

**\#\# 12.2. Reserve stock for order**

Member 4 gọi trong transaction tạo order.

Input:

\`\`\`text  
List of MenuItemId \+ Quantity  
\`\`\`

Rule:

\`\`\`text  
Chỉ trừ món TrackStock \= true.  
Kiểm tra lại AvailableQuantity ngay trước khi trừ.  
Dùng RowVersion/concurrency để tránh oversell.  
Nếu không đủ stock, throw lỗi và rollback transaction.  
Nếu trừ về 0, set IsAvailable \= false.  
\`\`\`

**\#\# 12.3. Rollback stock for cancelled order**

Member 4 gọi khi cancel order.

Input:

\`\`\`text  
List of OrderItem snapshot: MenuItemId, Quantity  
\`\`\`

Rule:

\`\`\`text  
Chỉ rollback món TrackStock \= true.  
AvailableQuantity \+= Quantity.  
Không chỉnh giá/tên món.  
Không chỉnh OrderItems snapshot.  
\`\`\`

**\#\# 12.4. Get menu item snapshot**

Member 4 gọi để tạo \`OrderItems\`.

Output gồm:

\`\`\`text  
MenuItemId  
ItemName  
UnitPrice  
TrackStock  
\`\`\`

Rule:

\`\`\`text  
Client không được tự gửi ItemName/UnitPrice để lưu DB.  
Service phải lấy snapshot từ MenuItems tại thời điểm order.  
\`\`\`

\---

**\# 13\. Integration Contract với Member 1 — Auth/Permission**

Member 3 cần dùng permission từ Member 1\.

**\#\# 13.1. Permission keys cần dùng**

\`\`\`text  
ManageCategories  
ManageMenuItems  
UpdateStock  
SetMenuAvailability  
\`\`\`

**\#\# 13.2. Mapping quyền**

| Action | Admin | Staff |  
|---|---:|---:|  
| Xem category | Có | Có |  
| Tạo category | Có | Không |  
| Sửa category | Có | Không |  
| Ẩn category | Có | Không |  
| Xem menu item | Có | Có |  
| Tạo menu item | Có | Không |  
| Sửa thông tin gốc món | Có | Không |  
| Sửa giá món | Có | Không |  
| Ẩn món | Có | Không |  
| Cập nhật stock | Có | Có |  
| Đánh dấu hết món | Có | Có |  
| Bán lại món | Có | Có |  
| Bật/tắt món đang bán | Có | Có |

\---

**\# 14\. Integration Contract với Member 5 — Bill/Payment/Dashboard**

Member 5 dùng dữ liệu \`MenuItems\` gián tiếp qua \`BillDetails\` snapshot.

Rule:

\`\`\`text  
Dashboard top món bán chạy không nên lấy trực tiếp từ MenuItems.Price hiện tại.  
Top món bán chạy lấy từ BillDetails join Bills Paid.  
MenuItemId trong BillDetails dùng để group theo món.  
ItemName trong BillDetails là snapshot để hiển thị lịch sử.  
\`\`\`

Nếu món bị đổi tên hoặc bị ẩn:

\`\`\`text  
Lịch sử bill vẫn hiển thị ItemName snapshot.  
Dashboard vẫn có thể group theo MenuItemId.  
\`\`\`

\---

**\# 15\. WPF Contracts**

**\#\# 15.1. CategoryWindow**

Chức năng:

\`\`\`text  
Load danh sách category.  
Search category.  
Tạo category.  
Sửa category.  
Ẩn category.  
Bật lại category.  
\`\`\`

Rule UI:

\`\`\`text  
Chỉ Admin được Create/Update/Hide/Reactivate.  
Staff có thể không thấy màn này hoặc chỉ read-only.  
CategoryName bắt buộc.  
DisplayOrder không âm.  
Ẩn category phải confirm.  
\`\`\`

DataGrid columns:

\`\`\`text  
CategoryId  
CategoryName  
Description  
DisplayOrder  
IsActive  
ItemCount  
CreatedAt  
UpdatedAt  
\`\`\`

**\#\# 15.2. MenuItemManagementWindow**

Chức năng Admin:

\`\`\`text  
Load danh sách món.  
Filter theo category.  
Search theo tên món.  
Tạo món.  
Sửa món.  
Sửa giá.  
Ẩn món.  
Cập nhật stock.  
Bật/tắt đang bán.  
Đánh dấu hết.  
Bán lại.  
\`\`\`

Chức năng Staff:

\`\`\`text  
Load danh sách món.  
Filter/search.  
Cập nhật stock.  
Bật/tắt đang bán.  
Đánh dấu hết.  
Bán lại.  
Không được sửa tên/giá/category/mô tả.  
\`\`\`

DataGrid columns:

\`\`\`text  
MenuItemId  
ItemName  
CategoryName  
Price  
TrackStock  
AvailableQuantity  
IsAvailable  
IsActive  
StatusText  
UpdatedAt  
\`\`\`

**\#\# 15.3. StockManagementWindow**

Chức năng:

\`\`\`text  
Hiển thị món TrackStock \= true.  
Nhập nhanh số lượng tồn.  
Đánh dấu hết món.  
Bán lại món.  
Filter món sắp hết.  
Filter món hết.  
\`\`\`

Rule UI:

\`\`\`text  
Quantity không được âm.  
Quantity \= 0 thì hiển thị Hết món.  
Reopen chỉ enable khi AvailableQuantity \> 0\.  
Sold out phải confirm.  
\`\`\`

\---

**\# 16\. Error Contract**

| Error Code | Message | Khi nào xảy ra |  
|---|---|---|  
| \`CATEGORY\_NOT\_FOUND\` | Không tìm thấy loại món. | CategoryId không tồn tại |  
| \`CATEGORY\_NAME\_REQUIRED\` | Tên loại món không được để trống. | Tạo/sửa category |  
| \`CATEGORY\_INACTIVE\` | Loại món đã bị ẩn. | Thêm món vào category inactive |  
| \`DISPLAY\_ORDER\_INVALID\` | Thứ tự hiển thị không hợp lệ. | DisplayOrder \< 0 |  
| \`MENU\_ITEM\_NOT\_FOUND\` | Không tìm thấy món. | MenuItemId không tồn tại |  
| \`ITEM\_NAME\_REQUIRED\` | Tên món không được để trống. | Tạo/sửa món |  
| \`PRICE\_INVALID\` | Giá món không hợp lệ. | Price \< 0 |  
| \`STOCK\_REQUIRED\` | Món quản lý tồn kho cần có số lượng tồn. | TrackStock \= true nhưng quantity null |  
| \`STOCK\_INVALID\` | Số lượng tồn không hợp lệ. | Quantity \< 0 |  
| \`ITEM\_INACTIVE\` | Món đã bị ẩn khỏi menu. | Order món inactive |  
| \`ITEM\_UNAVAILABLE\` | Món đã hết hoặc tạm ngưng bán. | Order món unavailable |  
| \`INSUFFICIENT\_STOCK\` | Số lượng món còn lại không đủ. | Quantity order \> AvailableQuantity |  
| \`CANNOT\_REOPEN\_NO\_STOCK\` | Không thể bán lại món đã hết số lượng. | Reopen khi stock \<= 0 |  
| \`PERMISSION\_DENIED\` | Bạn không có quyền thực hiện chức năng này. | Staff gọi chức năng Admin |  
| \`CONCURRENCY\_CONFLICT\` | Dữ liệu món vừa được cập nhật, vui lòng thử lại. | RowVersion conflict |

\---

**\# 17\. Acceptance Criteria**

**\#\# 17.1. Category**

\`\`\`text  
\[ \] Admin tạo category thành công.  
\[ \] CategoryName rỗng bị chặn.  
\[ \] DisplayOrder âm bị chặn.  
\[ \] Admin sửa category thành công.  
\[ \] Admin ẩn category thành công.  
\[ \] Category inactive không hiển thị trên Customer Web.  
\[ \] Staff không tạo/sửa/ẩn category được.  
\`\`\`

**\#\# 17.2. MenuItem**

\`\`\`text  
\[ \] Admin tạo món thành công.  
\[ \] ItemName rỗng bị chặn.  
\[ \] Price âm bị chặn.  
\[ \] CategoryId không hợp lệ bị chặn.  
\[ \] TrackStock \= true bắt buộc AvailableQuantity \>= 0\.  
\[ \] AvailableQuantity \= 0 thì IsAvailable \= false.  
\[ \] Admin sửa tên/giá/mô tả món thành công.  
\[ \] Staff không sửa tên/giá/category/mô tả món được.  
\[ \] Admin ẩn món thì IsActive \= false và IsAvailable \= false.  
\[ \] Món inactive không hiển thị trên Customer Web.  
\`\`\`

**\#\# 17.3. Stock / Availability**

\`\`\`text  
\[ \] Admin/Staff cập nhật stock thành công.  
\[ \] Quantity âm bị chặn.  
\[ \] Quantity \= 0 tự chuyển IsAvailable \= false.  
\[ \] Đánh dấu hết món chuyển IsAvailable \= false.  
\[ \] Bán lại món TrackStock cần AvailableQuantity \> 0\.  
\[ \] Không thể bán lại món inactive.  
\[ \] Order không được lấy món unavailable.  
\[ \] Order không được lấy món không đủ stock.  
\[ \] Cancel order cộng lại stock đúng.  
\[ \] RowVersion/concurrency ngăn oversell khi nhiều khách order cùng lúc.  
\`\`\`

**\#\# 17.4. Customer Menu**

\`\`\`text  
\[ \] Customer Web chỉ thấy category active.  
\[ \] Customer Web chỉ thấy menu item active.  
\[ \] Món available cho phép thêm vào giỏ.  
\[ \] Món unavailable hiển thị hết món/tạm ngưng bán.  
\[ \] Customer Web không tự gửi ItemName/UnitPrice để ghi database.  
\`\`\`

**\#\# 17.5. WPF**

\`\`\`text  
\[ \] CategoryWindow load/search/create/update/hide đúng quyền.  
\[ \] MenuItemManagementWindow load/filter/search đúng.  
\[ \] Admin thấy đầy đủ chức năng quản lý món.  
\[ \] Staff chỉ thấy chức năng stock/availability.  
\[ \] StockManagementWindow cập nhật số lượng nhanh được.  
\[ \] WPF chỉ gọi Service, không gọi DAO/DbContext.  
\[ \] WPF có try-catch khi gọi Service.  
\`\`\`

\---

**\# 18\. Deliverables của Member 3**

Member 3 cần bàn giao:

\`\`\`text  
BusinessObjects/Menu  
\- Category  
\- MenuItem  
\- CategoryDto  
\- MenuItemDto  
\- CustomerMenuDto  
\- MenuItemSnapshotDto  
\- CreateCategoryRequest  
\- UpdateCategoryRequest  
\- CreateMenuItemRequest  
\- UpdateMenuItemRequest  
\- UpdateStockRequest  
\- SetAvailabilityRequest

DataAccessObjects/Menu  
\- CategoryDAO  
\- MenuItemDAO

Repositories/Menu  
\- ICategoryRepository  
\- CategoryRepository  
\- IMenuItemRepository  
\- MenuItemRepository

Services/Menu  
\- ICategoryService  
\- CategoryService  
\- IMenuItemService  
\- MenuItemService  
\- IStockService  
\- StockService  
\- ICustomerMenuService  
\- CustomerMenuService

WPF  
\- CategoryWindow  
\- MenuItemManagementWindow  
\- StockManagementWindow

API contracts  
\- Customer menu API  
\- Admin category API  
\- Admin menu item API  
\- Staff/Admin stock API

Integration contracts  
\- Auth/Permission với Member 1  
\- Order/Stock transaction với Member 4  
\- Bill/Dashboard snapshot với Member 5  
\`\`\`

\---

**\# 19\. Definition of Done**

Member 3 hoàn thành khi:

\`\`\`text  
\[ \] Có đủ entity/dto/contract cho Category và MenuItem.  
\[ \] Có đủ business rule trong Service.  
\[ \] Có phân quyền Admin/Staff đúng.  
\[ \] Có API contract rõ cho Customer menu.  
\[ \] Có internal contract cho Member 4 tạo order/trừ stock/rollback stock.  
\[ \] Có WPF contract cho quản lý category/menu/stock.  
\[ \] Có error contract chuẩn.  
\[ \] Có acceptance criteria đầy đủ.  
\[ \] Không vi phạm rule layer WPF.  
\[ \] Demo được flow:  
    Admin tạo category  
    Admin tạo món  
    Staff cập nhật stock  
    Customer Web thấy menu  
    Order module validate món qua contract  
\`\`\`

\---

**\# 20\. Flow demo chuẩn của Member 3**

\`\`\`text  
1\. Admin login.  
2\. Admin tạo category "Đồ uống".  
3\. Admin tạo món "Trà đào" giá 30000, TrackStock \= true, AvailableQuantity \= 10\.  
4\. Staff login.  
5\. Staff cập nhật stock Trà đào còn 5\.  
6\. Customer Web gọi /api/customer/menu và thấy Trà đào đang bán.  
7\. Member 4 gọi ValidateOrderableItems với Trà đào x2.  
8\. Member 3 trả snapshot ItemName \+ UnitPrice \+ stock hợp lệ.  
9\. Member 4 tạo order và gọi ReserveStockForOrder.  
10\. Stock còn 3\.  
11\. Staff đánh dấu hết món.  
12\. Customer Web thấy Trà đào hết món và không thêm được vào giỏ.  
\`\`\`

# Huấn

**\# SRS — Member 4: Session, Order, Request, Bill, Payment & Split Bill**

**\*\*Project:\*\*** QR Food Ordering Management System    
**\*\*Owner:\*\*** Member 4    
**\*\*Document type:\*\*** Module SRS / Business Rules / Contracts    
**\*\*Version:\*\*** 2.0 — After scope reassignment    
**\*\*Main change:\*\*** Member 4 now owns the full operation flow from session/order to bill, payment and split bill.

\---

**\#\# 1\. Module Goal**

Member 4 is responsible for the operational core of the restaurant system:

\`\`\`text  
Customer scans QR  
→ Customer sends order / call staff / payment request  
→ System creates or gets TableSession  
→ System creates Order and OrderItems  
→ System coordinates stock reservation  
→ System creates or updates Bill and BillDetails  
→ WPF receives print/request notification  
→ Staff handles split bill and payment  
→ System closes session when payment is completed  
\`\`\`

This module must protect transaction consistency because it touches session, order, stock, bill and payment flow.

\---

**\#\# 2\. Module Scope**

**\#\#\# 2.1. Member 4 owns**

\`\`\`text  
TableSessions  
TableSessionCustomers  
Orders  
OrderItems  
ServiceRequests  
Order print status  
Order reprint  
Order cancel  
Bills  
BillDetails  
Split bill  
Move bill item  
Payment confirmation  
Session close after payment  
Realtime events for order/request/payment/session  
\`\`\`

\#\#\# 2.2. Member 4 does not own

\`\`\`text  
Users / login / role / permission master data          → Member 1  
DiningTables / QR master data                         → Member 2  
Categories / MenuItems / Stock master data / Add-ons  → Member 3  
Dashboard / revenue / statistics / top selling report → Member 5  
Admin correction of paid payment method               → Member 5  
\`\`\`

\#\#\# 2.3. Important boundary

Member 4 owns the \*\*creation and confirmation\*\* of bills and payments.

Member 5 can read \`Bills\`, \`BillDetails\`, \`Payments\` for reports and can update only payment method fields when Admin corrects a paid payment method.

\---

\#\# 3\. Database Ownership

Member 4 owns these tables:

\`\`\`text  
TableSessions  
TableSessionCustomers  
Orders  
OrderItems  
ServiceRequests  
Bills  
BillDetails  
Payments  
\`\`\`

Member 4 reads from these tables through contracts:

\`\`\`text  
Users           → for current user and audit  
DiningTables    → for table status through Member 2 contract  
MenuItems       → for item snapshot and stock through Member 3 contract  
Categories      → indirectly through menu display only  
\`\`\`

\---

\#\# 4\. Main Entities

\#\# 4.1. TableSessions

Represents one serving session at one dining table.

| Field | Type | Rule |  
|---|---|---|  
| \`TableSessionId\` | int PK | Primary key |  
| \`TableId\` | int FK | FK to \`DiningTables\` |  
| \`StartedAt\` | datetime | Set when session starts |  
| \`EndedAt\` | datetime? | Set when session closes |  
| \`Status\` | string | \`Open\`, \`WaitingPayment\`, \`Closed\`, \`Cancelled\` |  
| \`OpenedBy\` | int? | UserId, nullable for Customer Web |  
| \`ClosedBy\` | int? | UserId who closed session |

Important rule:

\`\`\`text  
One table can have only one active session.  
Active statuses: Open, WaitingPayment.  
\`\`\`

Recommended database constraint:

\`\`\`sql  
CREATE UNIQUE INDEX UX\_TableSessions\_OneActiveSessionPerTable  
ON TableSessions(TableId)  
WHERE Status IN ('Open', 'WaitingPayment');  
\`\`\`

\---

\#\# 4.2. TableSessionCustomers

Represents a customer device/browser inside a table session.

| Field | Type | Rule |  
|---|---|---|  
| \`SessionCustomerId\` | int PK | Primary key |  
| \`TableSessionId\` | int FK | FK to \`TableSessions\` |  
| \`ClientToken\` | string | Required for Customer Web |  
| \`DisplayName\` | string | 1–50 characters |  
| \`CreatedAt\` | datetime | Created time |

Rule:

\`\`\`text  
UNIQUE(TableSessionId, ClientToken)  
\`\`\`

\---

\#\# 4.3. Orders

Represents one submitted order.

| Field | Type | Rule |  
|---|---|---|  
| \`OrderId\` | int PK | Primary key |  
| \`TableSessionId\` | int FK | Order belongs to session |  
| \`SessionCustomerId\` | int? FK | Customer device if from Customer Web |  
| \`OrderCode\` | string | Unique order code |  
| \`OrderSource\` | string | \`CustomerWeb\`, \`StaffApp\` |  
| \`ClientToken\` | string? | Required if source is Customer Web |  
| \`Status\` | string | \`Accepted\`, \`Cancelled\` |  
| \`PrintStatus\` | string | \`PendingPrint\`, \`Printed\`, \`PrintFailed\` |  
| \`CustomerNote\` | string? | General customer note |  
| \`SystemNote\` | string? | Internal note |  
| \`CancelReason\` | string? | Required when cancelled |  
| \`CreatedAt\` | datetime | Created time |  
| \`UpdatedAt\` | datetime? | Updated time |  
| \`CancelledAt\` | datetime? | Cancel time |  
| \`PrintedAt\` | datetime? | Print success time |  
| \`PrintError\` | string? | Print failure message |  
| \`PrintRetryCount\` | int | Must be \>= 0 |  
| \`CreatedBy\` | int? FK | Staff user if created from WPF |  
| \`CancelledBy\` | int? FK | Staff/Admin user |

\---

\#\# 4.4. OrderItems

Represents item snapshots in an order. Main dishes and add-ons are both stored here.

| Field | Type | Rule |  
|---|---|---|  
| \`OrderItemId\` | int PK | Primary key |  
| \`OrderId\` | int FK | FK to \`Orders\` |  
| \`MenuItemId\` | int FK | FK to \`MenuItems\` |  
| \`SessionCustomerId\` | int? FK | Customer owner |  
| \`ParentOrderItemId\` | int? FK | Null for main item, points to main item for add-on |  
| \`LineType\` | string | \`Main\`, \`Addon\` |  
| \`ItemName\` | string | Snapshot from \`MenuItems.ItemName\` |  
| \`Quantity\` | int | Must be \> 0 |  
| \`UnitPrice\` | decimal | Must be \>= 0 |  
| \`TotalPrice\` | decimal | \`Quantity \* UnitPrice\` |  
| \`Note\` | string? | Item note |  
| \`CreatedAt\` | datetime | Created time |

Snapshot rule:

\`\`\`text  
Customer Web must not send ItemName, UnitPrice or TotalPrice for saving.  
Member 4 must get snapshot from Member 3\.  
Old orders must not change when Admin edits menu name or price.  
\`\`\`

Add-on rule:

\`\`\`text  
Main item: ParentOrderItemId \= null, LineType \= Main.  
Add-on item: ParentOrderItemId \= main OrderItemId, LineType \= Addon.  
Add-on must belong to the selected main dish through Member 3 add-on contract.  
\`\`\`

\---

\#\# 4.5. ServiceRequests

Represents customer requests from Customer Web.

| Field | Type | Rule |  
|---|---|---|  
| \`RequestId\` | int PK | Primary key |  
| \`TableSessionId\` | int FK | FK to session |  
| \`SessionCustomerId\` | int? FK | Customer device |  
| \`ClientToken\` | string? | Required if from Customer Web |  
| \`RequestType\` | string | \`CallStaff\`, \`PaymentRequest\` |  
| \`Reason\` | string? | Request reason |  
| \`PaymentMethod\` | string? | Required for \`PaymentRequest\` |  
| \`Message\` | string? | Customer message |  
| \`Status\` | string | \`Pending\`, \`Confirmed\`, \`Completed\` |  
| \`CreatedAt\` | datetime | Created time |  
| \`ConfirmedAt\` | datetime? | Confirmed time |  
| \`CompletedAt\` | datetime? | Completed time |  
| \`ConfirmedBy\` | int? FK | Staff/Admin user |  
| \`CompletedBy\` | int? FK | Staff/Admin user |

Status flow:

\`\`\`text  
Pending → Confirmed → Completed  
\`\`\`

\---

\#\# 4.6. Bills

Represents one bill inside a table session.

| Field | Type | Rule |  
|---|---|---|  
| \`BillId\` | int PK | Primary key |  
| \`BillCode\` | string | Unique bill code |  
| \`TableSessionId\` | int FK | FK to \`TableSessions\` |  
| \`BillNo\` | int | Sequence number inside session |  
| \`BillName\` | string | Example: \`Bill mặc định\`, \`Bill 2\`, \`Khách A\` |  
| \`IsDefault\` | bool | Default bill receives new order items |  
| \`Status\` | string | \`Unpaid\`, \`Paid\`, \`Cancelled\` |  
| \`SubTotal\` | decimal | Sum of details |  
| \`DiscountAmount\` | decimal | MVP can be 0 |  
| \`FinalAmount\` | decimal | \`SubTotal \- DiscountAmount\` |  
| \`CreatedAt\` | datetime | Created time |  
| \`CreatedBy\` | int? FK | User who created bill |  
| \`PaidAt\` | datetime? | Paid time |  
| \`CancelledAt\` | datetime? | Cancel time |  
| \`CancelledBy\` | int? FK | User who cancelled |  
| \`CancelReason\` | string? | Required when cancelled |

Rules:

\`\`\`text  
Each active session must have one default unpaid bill.  
Order items are added to default bill.  
Paid bill is locked.  
Cancelled bill is not included in revenue.  
\`\`\`

\---

\#\# 4.7. BillDetails

Represents item rows inside a bill.

| Field | Type | Rule |  
|---|---|---|  
| \`BillDetailId\` | int PK | Primary key |  
| \`BillId\` | int FK | FK to \`Bills\` |  
| \`OrderItemId\` | int FK | Source order item |  
| \`MenuItemId\` | int FK | Menu item id |  
| \`SessionCustomerId\` | int? FK | Customer owner |  
| \`CustomerDisplayName\` | string? | Snapshot customer name |  
| \`ItemName\` | string | Snapshot item name |  
| \`Quantity\` | int | Must be \> 0 |  
| \`UnitPrice\` | decimal | Must be \>= 0 |  
| \`TotalPrice\` | decimal | \`Quantity \* UnitPrice\` |  
| \`CreatedAt\` | datetime | Created time |

Rule:

\`\`\`text  
BillDetails use snapshots from OrderItems, not latest MenuItems.  
BillDetails must not be edited after bill is Paid.  
\`\`\`

\---

\#\# 4.8. Payments

Represents payment information of a bill.

| Field | Type | Rule |  
|---|---|---|  
| \`PaymentId\` | int PK | Primary key |  
| \`BillId\` | int FK unique | One bill has at most one payment |  
| \`PaymentMethod\` | string | \`Cash\`, \`BankTransfer\`, \`Card\`, \`EWallet\` |  
| \`Amount\` | decimal | Must equal \`Bill.FinalAmount\` |  
| \`PaidAt\` | datetime | Payment time |  
| \`ConfirmedBy\` | int FK | Staff/Admin who confirmed |  
| \`UpdatedAt\` | datetime? | Used by Member 5 Admin correction |  
| \`UpdatedBy\` | int? FK | Admin who corrected method |  
| \`ChangeReason\` | string? | Required when method is corrected |

Rule:

\`\`\`text  
Member 4 creates payment and confirms bill as Paid.  
Member 5 may update only PaymentMethod, UpdatedAt, UpdatedBy, ChangeReason if Admin correction is needed.  
\`\`\`

\---

\# 5\. Functional Requirements

\#\# FR-M4-001: Create or get active session

Input:

\`\`\`text  
TableId or QrToken  
CurrentUserId optional  
\`\`\`

Rules:

\`\`\`text  
If active session exists, use it.  
If not, create TableSession with Status \= Open.  
When new session is created, call Member 2 to set table status \= Occupied.  
\`\`\`

\---

\#\# FR-M4-002: Create customer order

Input:

\`\`\`text  
TableToken  
ClientToken  
DisplayName optional  
CustomerNote optional  
Items:  
\- MenuItemId  
\- Quantity  
\- Note optional  
\- Addons optional  
\`\`\`

Rules:

\`\`\`text  
Validate QR through Member 2\.  
Validate menu item and add-ons through Member 3\.  
Get snapshots from Member 3\.  
Reserve stock through Member 3\.  
Create Order and OrderItems.  
Add OrderItems to default Bill.  
Commit transaction.  
Send realtime events after commit.  
\`\`\`

Output:

\`\`\`text  
OrderId  
OrderCode  
TableSessionId  
AcceptedItems  
RejectedItems if partial accept is used  
PrintStatus \= PendingPrint  
\`\`\`

\---

\#\# FR-M4-003: Create staff order

Staff/Admin can create order from WPF.

Rules:

\`\`\`text  
CurrentUserId is required.  
Table must be active.  
Session is created if needed.  
OrderSource \= StaffApp.  
CreatedBy \= CurrentUserId.  
\`\`\`

\---

\#\# FR-M4-004: Print order

Flow:

\`\`\`text  
Order created with PrintStatus \= PendingPrint  
→ API sends OrderPrintRequested event  
→ WPF receives event  
→ WPF prints using local printer  
→ WPF calls MarkPrinted or MarkPrintFailed  
\`\`\`

Rules:

\`\`\`text  
API does not print directly.  
WPF is responsible for local printer.  
PrintRetryCount increases on failed print.  
\`\`\`

\---

\#\# FR-M4-005: Cancel order

Rules:

\`\`\`text  
Only Accepted order can be cancelled.  
Cancel reason is required.  
Rollback stock through Member 3\.  
Remove or reduce related BillDetails.  
Recalculate affected bills.  
Transaction is required.  
\`\`\`

\---

\#\# FR-M4-006: Call staff request

Customer Web can send call staff request.

Rules:

\`\`\`text  
RequestType \= CallStaff.  
PaymentMethod must be null.  
Status \= Pending.  
WPF receives realtime notification.  
\`\`\`

\---

\#\# FR-M4-007: Payment request

Customer Web can request payment.

Rules:

\`\`\`text  
RequestType \= PaymentRequest.  
PaymentMethod must be valid.  
Status \= Pending.  
TableSession.Status \= WaitingPayment.  
DiningTables.Status \= WaitingPayment through Member 2 contract.  
\`\`\`

\---

\#\# FR-M4-008: Add order items to default bill

After order is accepted, order items must be added to default unpaid bill.

Rules:

\`\`\`text  
Get or create default bill.  
Create BillDetails from OrderItems snapshot.  
Recalculate bill totals.  
Default bill must be Unpaid.  
\`\`\`

\---

\#\# FR-M4-009: View bills in session

WPF must show:

\`\`\`text  
List of bills in current session  
Bill details  
Total amount  
Bill status  
Payment information if paid  
Group by customer if needed  
\`\`\`

\---

\#\# FR-M4-010: Split bill

Staff/Admin can split a bill by moving quantity from source bill to new or existing target bill.

Rules:

\`\`\`text  
Only Unpaid bill can be split.  
Source and target bills must be in same TableSession.  
QuantityToMove \> 0\.  
QuantityToMove \<= source detail quantity.  
Recalculate both bills after split.  
Paid bill cannot be split.  
\`\`\`

\---

\#\# FR-M4-011: Move item between bills

Rules:

\`\`\`text  
Both bills must be Unpaid.  
Both bills must belong to same TableSession.  
If target bill already has same OrderItemId, merge quantity.  
If source detail quantity becomes 0, remove source detail.  
\`\`\`

\---

\#\# FR-M4-012: Confirm payment

Input:

\`\`\`text  
BillId  
PaymentMethod  
CurrentUserId  
\`\`\`

Rules:

\`\`\`text  
Bill must be Unpaid.  
Bill must have at least one detail.  
PaymentMethod must be valid.  
Payment.Amount \= Bill.FinalAmount.  
Create Payment.  
Set Bill.Status \= Paid.  
Set Bill.PaidAt \= now.  
If all active bills in session are Paid or Cancelled, close session.  
\`\`\`

\---

\#\# FR-M4-013: Close session after payment

Rules:

\`\`\`text  
Session can close only when no Unpaid bill remains.  
When session closes:  
TableSessions.Status \= Closed.  
TableSessions.EndedAt \= now.  
DiningTables.Status \= Available through Member 2 contract.  
\`\`\`

\---

\# 6\. Business Rules Summary

\`\`\`text  
BR-M4-001: One table has at most one active session.  
BR-M4-002: Customer Web must send ClientToken.  
BR-M4-003: Customer Web must not send price/name for database saving.  
BR-M4-004: OrderItems and BillDetails must use snapshot data.  
BR-M4-005: Stock reservation and order creation must be in one transaction.  
BR-M4-006: Order accepted must be added to default bill.  
BR-M4-007: Bill Paid is locked.  
BR-M4-008: Only Unpaid bill can be split or paid.  
BR-M4-009: Payment amount must equal Bill.FinalAmount.  
BR-M4-010: One bill has at most one payment.  
BR-M4-011: Revenue data is generated by Member 5 but based on paid bills from Member 4\.  
BR-M4-012: Member 5 can correct only PaymentMethod after payment if Admin.  
\`\`\`

\---

\# 7\. Service Contracts

\#\# 7.1. ITableSessionService

\`\`\`csharp  
TableSessionDto GetById(int tableSessionId);  
TableSessionDto? GetCurrentSessionByTableId(int tableId);  
TableSessionDto GetOrCreateActiveSessionByTableId(int tableId, int? openedBy);  
TableSessionDto GetOrCreateActiveSessionByQrToken(string qrToken, int? openedBy);  
TableSessionDto MarkWaitingPayment(int tableSessionId);  
bool CloseSessionIfCompleted(int tableSessionId, int closedBy);  
List\<TableSessionDto\> GetActiveSessions();  
TableSessionDetailDto GetSessionDetail(int tableSessionId);  
\`\`\`

\#\# 7.2. IOrderService

\`\`\`csharp  
CreateOrderResponse CreateCustomerOrder(CreateCustomerOrderRequest request);  
CreateOrderResponse CreateStaffOrder(CreateStaffOrderRequest request, int currentUserId);  
List\<OrderSummaryDto\> GetOrders(OrderFilter filter);  
OrderDetailDto GetOrderDetail(int orderId);  
List\<OrderSummaryDto\> GetOrdersBySession(int tableSessionId);  
void CancelOrder(int orderId, CancelOrderRequest request, int currentUserId);  
\`\`\`

\#\# 7.3. IOrderPrintService

\`\`\`csharp  
void MarkPrinted(int orderId, int currentUserId);  
void MarkPrintFailed(int orderId, MarkPrintFailedRequest request, int currentUserId);  
void RequestReprint(int orderId, int currentUserId);  
List\<OrderSummaryDto\> GetPendingPrintOrders();  
\`\`\`

\#\# 7.4. IServiceRequestService

\`\`\`csharp  
ServiceRequestDto CreateServiceRequest(CreateServiceRequestRequest request);  
ServiceRequestDto ConfirmRequest(int requestId, int currentUserId);  
ServiceRequestDto CompleteRequest(int requestId, int currentUserId);  
List\<ServiceRequestDto\> GetPendingRequests();  
\`\`\`

\#\# 7.5. IBillService

\`\`\`csharp  
BillDto GetOrCreateDefaultBill(int tableSessionId, int? createdBy);  
BillDto GetBillById(int billId);  
List\<BillSummaryDto\> GetBillsBySession(int tableSessionId);  
BillDto AddOrderItemsToDefaultBill(AddOrderItemsToBillRequest request);  
void RemoveOrReduceBillDetailsForCancelledOrder(int orderId);  
BillDto RecalculateBillTotal(int billId);  
void CancelUnpaidBill(int billId, string reason, int currentUserId);  
\`\`\`

\#\# 7.6. ISplitBillService

\`\`\`csharp  
BillDto CreateSplitBill(SplitBillRequest request, int currentUserId);  
BillDto MoveItemToBill(MoveBillItemRequest request, int currentUserId);  
BillDto CreateEmptyBillForSession(int tableSessionId, string billName, int currentUserId);  
bool ValidateSplitQuantity(int billDetailId, int quantityToMove);  
\`\`\`

\#\# 7.7. IPaymentService

\`\`\`csharp  
PaymentDto ConfirmPayment(ConfirmPaymentRequest request, int currentUserId);  
PaymentDto GetPaymentByBillId(int billId);  
bool HasUnpaidBills(int tableSessionId);  
\`\`\`

\#\# 7.8. IPaymentReadPortForMember5

Read-only and limited correction port for Member 5\.

\`\`\`csharp  
List\<PaidBillReportRowDto\> GetPaidBillsForReport(DateTime from, DateTime to);  
PaymentDto GetPaymentForCorrection(int billId);  
PaymentDto UpdatePaymentMethodByAdmin(UpdatePaymentMethodRequest request, int adminUserId);  
\`\`\`

\---

\# 8\. API Contracts

\#\# Customer APIs

\`\`\`text  
POST /api/customer/orders  
POST /api/customer/service-requests/call-staff  
POST /api/customer/service-requests/payment-request  
GET  /api/customer/orders/{orderCode}  
\`\`\`

\#\# Staff APIs

\`\`\`text  
GET  /api/staff/orders  
GET  /api/staff/orders/{id}  
POST /api/staff/orders  
PUT  /api/staff/orders/{id}/cancel  
PUT  /api/staff/orders/{id}/mark-printed  
PUT  /api/staff/orders/{id}/mark-print-failed  
POST /api/staff/orders/{id}/reprint

GET  /api/staff/requests  
PUT  /api/staff/requests/{id}/confirm  
PUT  /api/staff/requests/{id}/complete

GET  /api/staff/sessions/{id}  
GET  /api/staff/sessions/active

GET  /api/staff/bills/session/{tableSessionId}  
GET  /api/staff/bills/{billId}  
POST /api/staff/bills/split  
POST /api/staff/bills/move-item  
POST /api/staff/payments/confirm  
\`\`\`

\---

\# 9\. WPF Screens

Member 4 should implement:

\`\`\`text  
TableSessionDetailView  
OrderManagementView  
OrderDetailView  
PrintQueueView  
ServiceRequestView  
PaymentRequestView  
BillView  
BillDetailView  
SplitBillView  
PaymentConfirmView  
\`\`\`

UI rules:

\`\`\`text  
WPF calls Service only.  
WPF must not call DAO or DbContext.  
WPF can show MessageBox.  
Service must throw business errors.  
WPF catches exceptions and displays friendly messages.  
\`\`\`

\---

\# 10\. Acceptance Criteria

\`\`\`text  
\[ \] Customer can send valid order.  
\[ \] Order creates session if needed.  
\[ \] Order items save snapshot.  
\[ \] Add-ons are linked to main order item.  
\[ \] Stock is reserved correctly.  
\[ \] Default bill is created and updated.  
\[ \] WPF receives print request.  
\[ \] Staff can mark print success/failure.  
\[ \] Staff can cancel order and rollback stock/bill.  
\[ \] Customer can call staff.  
\[ \] Customer can request payment.  
\[ \] Staff can split bill.  
\[ \] Staff can move item between bills.  
\[ \] Staff can confirm payment.  
\[ \] Session closes when all bills are completed.  
\[ \] Paid bill cannot be modified.  
\`\`\`

# Linh

\# SRS — Member 5: Dashboard, Revenue Report, Top Selling Items & Admin Payment Method Correction

\*\*Project:\*\* QR Food Ordering Management System    
\*\*Owner:\*\* Member 5    
\*\*Document type:\*\* Module SRS / Business Rules / Contracts    
\*\*Version:\*\* 2.0 — After scope reassignment    
\*\*Main change:\*\* Bill, payment confirmation and split bill are moved to Member 4\. Member 5 focuses on reports and statistics.

\---

\#\# 1\. Module Goal

Member 5 is responsible for business reporting and revenue visibility.

The module answers questions such as:

\`\`\`text  
How much revenue did the restaurant make today?  
How many bills were paid today?  
Which menu items sell the most?  
Which payment methods are used most?  
How many orders were created today?  
How many tables are serving or waiting for payment?  
Which payments were corrected by Admin?  
\`\`\`

Member 5 is mostly \*\*read-only\*\*. The only write operation is Admin payment method correction after a bill has already been paid.

\---

\#\# 2\. Module Scope

\#\#\# 2.1. Member 5 owns

\`\`\`text  
Dashboard  
Revenue statistics  
Top selling item report  
Revenue by payment method  
Paid bill history display  
Payment correction history display  
Admin payment method correction for paid payment  
Report filters by date range  
Report export if team has time  
\`\`\`

\#\#\# 2.2. Member 5 does not own

\`\`\`text  
TableSession creation / closing  
Order creation / cancellation  
Print order  
Service request handling  
Bill creation  
Bill detail editing  
Split bill  
Move item between bills  
Payment confirmation  
Stock reservation / rollback  
Menu master data  
Table / QR master data  
User / role master data  
\`\`\`

\#\#\# 2.3. Important boundary

Member 5 reads from operational tables owned by Member 4:

\`\`\`text  
Bills  
BillDetails  
Payments  
Orders  
OrderItems  
TableSessions  
ServiceRequests  
\`\`\`

Member 5 can update only these fields in \`Payments\` when Admin corrects paid payment method:

\`\`\`text  
PaymentMethod  
UpdatedAt  
UpdatedBy  
ChangeReason  
\`\`\`

Member 5 must not update:

\`\`\`text  
Payments.Amount  
Payments.PaidAt  
Payments.ConfirmedBy  
Bills.FinalAmount  
Bills.PaidAt  
BillDetails  
Orders  
\`\`\`

\---

\# 3\. Data Used by Member 5

\#\# 3.1. Read tables

| Table | Usage |  
|---|---|  
| \`Bills\` | Revenue, paid bill count, bill history |  
| \`BillDetails\` | Top selling items, item revenue |  
| \`Payments\` | Revenue by payment method, payment correction |  
| \`Orders\` | Order count, cancelled order count |  
| \`OrderItems\` | Optional order item analysis |  
| \`TableSessions\` | Session count, table operation status |  
| \`DiningTables\` | Table name/area in report |  
| \`MenuItems\` | Current menu reference only, not for historical price |  
| \`Users\` | Staff/Admin name in audit |  
| \`ServiceRequests\` | Pending request count if shown in dashboard |

\#\# 3.2. Write table

Member 5 has limited write access to:

\`\`\`text  
Payments  
\`\`\`

Only for Admin payment method correction.

\---

\# 4\. Functional Requirements

\#\# FR-M5-001: Dashboard today

Dashboard must show today's key numbers.

Data:

\`\`\`text  
RevenueToday  
PaidBillCountToday  
AverageBillValue  
OrderCountToday  
ServingTableCount  
WaitingPaymentTableCount  
PrintFailedOrderCount  
TopSellingItemsToday  
RevenueByPaymentMethodToday  
\`\`\`

Main rule:

\`\`\`text  
Revenue is calculated from Bills where Status \= Paid.  
Today revenue uses Bills.PaidAt, not Bills.CreatedAt.  
\`\`\`

\---

\#\# FR-M5-002: Revenue by date range

User can filter revenue by date range.

Input:

\`\`\`text  
FromDate  
ToDate  
\`\`\`

Rules:

\`\`\`text  
FromDate \<= ToDate.  
Use Bills.PaidAt for date filter.  
Only Bills.Status \= Paid are counted.  
Cancelled and Unpaid bills are excluded.  
\`\`\`

Output:

\`\`\`text  
TotalRevenue  
PaidBillCount  
AverageBillValue  
RevenueByDay  
\`\`\`

\---

\#\# FR-M5-003: Top selling items

Report must show best-selling menu items.

Source:

\`\`\`text  
BillDetails JOIN Bills  
WHERE Bills.Status \= Paid  
\`\`\`

Group by:

\`\`\`text  
MenuItemId  
ItemName snapshot  
\`\`\`

Metrics:

\`\`\`text  
TotalQuantity \= SUM(BillDetails.Quantity)  
TotalRevenue \= SUM(BillDetails.TotalPrice)  
\`\`\`

Important rule:

\`\`\`text  
Use BillDetails.ItemName snapshot, not current MenuItems.ItemName.  
\`\`\`

Reason:

\`\`\`text  
If Admin changes menu item name later, historical report should still show sold item snapshot correctly.  
\`\`\`

\---

\#\# FR-M5-004: Revenue by payment method

Report must group revenue by payment method.

Source:

\`\`\`text  
Payments JOIN Bills  
WHERE Bills.Status \= Paid  
\`\`\`

Group by:

\`\`\`text  
PaymentMethod  
\`\`\`

Metrics:

\`\`\`text  
PaymentCount \= COUNT(Payments.PaymentId)  
TotalAmount \= SUM(Payments.Amount)  
\`\`\`

\---

\#\# FR-M5-005: Paid bill history

WPF must show paid bill history.

Filters:

\`\`\`text  
FromDate  
ToDate  
PaymentMethod  
TableName / Area optional  
Keyword optional: BillCode, TableName  
\`\`\`

Displayed fields:

\`\`\`text  
BillCode  
TableName  
BillName  
FinalAmount  
PaymentMethod  
PaidAt  
ConfirmedByName  
UpdatedAt if corrected  
UpdatedByName if corrected  
ChangeReason if corrected  
\`\`\`

Rule:

\`\`\`text  
Paid bill history is read-only.  
\`\`\`

\---

\#\# FR-M5-006: Admin payment method correction

Admin can correct the payment method of a paid bill if staff selected the wrong method.

Input:

\`\`\`text  
BillId  
NewPaymentMethod  
ChangeReason  
AdminUserId  
\`\`\`

Rules:

\`\`\`text  
Only Admin can do this.  
Bill must be Paid.  
Payment must exist.  
NewPaymentMethod must be Cash, BankTransfer, Card, or EWallet.  
ChangeReason is required.  
Only PaymentMethod and correction audit fields can be updated.  
Amount must not change.  
Bill total must not change.  
BillDetails must not change.  
\`\`\`

Allowed updates:

\`\`\`text  
Payments.PaymentMethod \= NewPaymentMethod  
Payments.UpdatedAt \= now  
Payments.UpdatedBy \= AdminUserId  
Payments.ChangeReason \= ChangeReason  
\`\`\`

Blocked updates:

\`\`\`text  
Payments.Amount  
Payments.PaidAt  
Payments.ConfirmedBy  
Bills.FinalAmount  
Bills.PaidAt  
Bills.Status  
BillDetails.Quantity  
BillDetails.UnitPrice  
BillDetails.TotalPrice  
\`\`\`

\---

\#\# FR-M5-007: Payment correction history

Member 5 should show corrected payments.

Filters:

\`\`\`text  
FromDate  
ToDate  
UpdatedBy  
PaymentMethod  
\`\`\`

Displayed fields:

\`\`\`text  
BillCode  
Old/New payment method if audit supports it  
Current PaymentMethod  
Amount  
PaidAt  
UpdatedAt  
UpdatedByName  
ChangeReason  
\`\`\`

MVP note:

\`\`\`text  
If no separate PaymentCorrectionLogs table exists, show only latest correction stored in Payments.  
\`\`\`

Optional improvement:

\`\`\`text  
Create PaymentCorrectionLogs for full correction history.  
\`\`\`

\---

\#\# FR-M5-008: Export report optional

If the team has time, Member 5 can export reports.

Formats:

\`\`\`text  
CSV  
Excel  
PDF optional  
\`\`\`

This is optional and not required for MVP.

\---

\# 5\. Business Rules

\`\`\`text  
BR-M5-001: Dashboard is read-only.  
BR-M5-002: Revenue only counts Bills.Status \= Paid.  
BR-M5-003: Revenue date is based on Bills.PaidAt.  
BR-M5-004: Cancelled bills do not count as revenue.  
BR-M5-005: Unpaid bills do not count as revenue.  
BR-M5-006: Top selling items use BillDetails snapshot data.  
BR-M5-007: Revenue by payment method uses Payments.Amount.  
BR-M5-008: Admin correction can update only PaymentMethod and correction audit fields.  
BR-M5-009: Staff cannot correct payment method after paid.  
BR-M5-010: Payment correction must require ChangeReason.  
BR-M5-011: Report module must not modify orders, bill details, stock, or session.  
BR-M5-012: Average bill value \= TotalRevenue / PaidBillCount; if count \= 0, average \= 0\.  
\`\`\`

\---

\# 6\. DTO Contracts

\#\# 6.1. DashboardDto

| Field | Type | Meaning |  
|---|---|---|  
| \`Date\` | date | Dashboard date |  
| \`RevenueToday\` | decimal | Total paid revenue today |  
| \`PaidBillCountToday\` | int | Number of paid bills today |  
| \`AverageBillValue\` | decimal | Revenue / paid bill count |  
| \`OrderCountToday\` | int | Orders created today |  
| \`ServingTableCount\` | int | Tables with active serving session |  
| \`WaitingPaymentTableCount\` | int | Tables waiting payment |  
| \`PrintFailedOrderCount\` | int | Orders with failed print |  
| \`TopSellingItems\` | List | Top items |  
| \`RevenueByPaymentMethods\` | List | Payment method summary |

\#\# 6.2. RevenueSummaryDto

| Field | Type |  
|---|---|  
| \`FromDate\` | date |  
| \`ToDate\` | date |  
| \`TotalRevenue\` | decimal |  
| \`PaidBillCount\` | int |  
| \`AverageBillValue\` | decimal |  
| \`RevenueByDays\` | List\<RevenueByDayDto\> |

\#\# 6.3. RevenueByDayDto

| Field | Type |  
|---|---|  
| \`Date\` | date |  
| \`Revenue\` | decimal |  
| \`PaidBillCount\` | int |

\#\# 6.4. TopSellingItemDto

| Field | Type |  
|---|---|  
| \`MenuItemId\` | int |  
| \`ItemName\` | string |  
| \`TotalQuantity\` | int |  
| \`TotalRevenue\` | decimal |

\#\# 6.5. PaymentMethodRevenueDto

| Field | Type |  
|---|---|  
| \`PaymentMethod\` | string |  
| \`PaymentCount\` | int |  
| \`TotalAmount\` | decimal |

\#\# 6.6. PaidBillHistoryDto

| Field | Type |  
|---|---|  
| \`BillId\` | int |  
| \`BillCode\` | string |  
| \`TableSessionId\` | int |  
| \`TableName\` | string |  
| \`BillName\` | string |  
| \`FinalAmount\` | decimal |  
| \`PaymentMethod\` | string |  
| \`PaidAt\` | datetime |  
| \`ConfirmedByName\` | string |  
| \`UpdatedAt\` | datetime? |  
| \`UpdatedByName\` | string? |  
| \`ChangeReason\` | string? |

\#\# 6.7. UpdatePaymentMethodRequest

| Field | Type | Rule |  
|---|---|---|  
| \`BillId\` | int | Required, bill must be Paid |  
| \`NewPaymentMethod\` | string | Cash / BankTransfer / Card / EWallet |  
| \`ChangeReason\` | string | Required |

\---

\# 7\. Service Contracts

\#\# 7.1. IDashboardService

\`\`\`csharp  
DashboardDto GetTodayDashboard();  
DashboardDto GetDashboardByDate(DateTime date);  
\`\`\`

\#\# 7.2. IRevenueReportService

\`\`\`csharp  
RevenueSummaryDto GetRevenueSummary(DateTime fromDate, DateTime toDate);  
List\<RevenueByDayDto\> GetRevenueByDay(DateTime fromDate, DateTime toDate);  
List\<PaymentMethodRevenueDto\> GetRevenueByPaymentMethod(DateTime fromDate, DateTime toDate);  
\`\`\`

\#\# 7.3. ITopSellingItemReportService

\`\`\`csharp  
List\<TopSellingItemDto\> GetTopSellingItems(DateTime fromDate, DateTime toDate, int top);  
\`\`\`

\#\# 7.4. IBillHistoryReadService

\`\`\`csharp  
List\<PaidBillHistoryDto\> GetPaidBillHistory(PaidBillHistoryFilter filter);  
PaidBillHistoryDto GetPaidBillDetail(int billId);  
\`\`\`

\#\# 7.5. IPaymentCorrectionService

\`\`\`csharp  
PaymentDto UpdatePaidPaymentMethod(UpdatePaymentMethodRequest request, int adminUserId);  
List\<PaidBillHistoryDto\> GetCorrectedPayments(DateTime fromDate, DateTime toDate);  
\`\`\`

Service rule:

\`\`\`text  
IPaymentCorrectionService must check Admin permission before update.  
\`\`\`

\---

\# 8\. API Contracts

\#\# Staff/Admin report APIs

\`\`\`text  
GET /api/reports/dashboard/today  
GET /api/reports/dashboard?date=yyyy-MM-dd  
GET /api/reports/revenue?fromDate=yyyy-MM-dd\&toDate=yyyy-MM-dd  
GET /api/reports/revenue/by-payment-method?fromDate=yyyy-MM-dd\&toDate=yyyy-MM-dd  
GET /api/reports/top-selling-items?fromDate=yyyy-MM-dd\&toDate=yyyy-MM-dd\&top=10  
GET /api/reports/paid-bills?fromDate=yyyy-MM-dd\&toDate=yyyy-MM-dd\&paymentMethod=Cash  
GET /api/reports/paid-bills/{billId}  
\`\`\`

\#\# Admin payment correction API

\`\`\`text  
PUT /api/admin/payments/{billId}/method  
\`\`\`

Request:

\`\`\`json  
{  
  "newPaymentMethod": "BankTransfer",  
  "changeReason": "Staff selected Cash by mistake"  
}  
\`\`\`

Rules:

\`\`\`text  
Admin only.  
Bill must be Paid.  
Payment must exist.  
Amount must not change.  
\`\`\`

\---

\# 9\. WPF Screens

Member 5 should implement:

\`\`\`text  
DashboardView  
RevenueReportView  
TopSellingItemsReportView  
RevenueByPaymentMethodView  
PaidBillHistoryView  
PaymentCorrectionView  
\`\`\`

UI rules:

\`\`\`text  
Report screens are read-only.  
Only PaymentCorrectionView allows update.  
PaymentCorrectionView must be visible only for Admin.  
Service must check Admin again even if UI hides the button.  
\`\`\`

\---

\# 10\. Acceptance Criteria

\`\`\`text  
\[ \] Dashboard shows revenue today correctly.  
\[ \] Revenue uses Bills.PaidAt and Bills.Status \= Paid.  
\[ \] Cancelled/Unpaid bills are excluded.  
\[ \] Top selling items are calculated from BillDetails joined with Paid bills.  
\[ \] Revenue by payment method is calculated from Payments joined with Paid bills.  
\[ \] Paid bill history can be filtered by date and payment method.  
\[ \] Staff cannot correct paid payment method.  
\[ \] Admin can correct paid payment method with reason.  
\[ \] Payment amount does not change after correction.  
\[ \] Bill final amount does not change after correction.  
\[ \] Report screens do not modify operational data.  
\`\`\`

# ERD

**\# Database & ERD Mermaid — QR Food Ordering System After Member 4/5 Reassignment**

**\*\*Project:\*\*** QR Food Ordering Management System    
**\*\*Document type:\*\*** Database contract \+ Mermaid ERD for draw.io import    
**\*\*Version:\*\*** 2.0    
**\*\*Scope:\*\*** Full database after adding Menu Add-on and moving Bill/Payment/Split Bill to Member 4\.

\---

**\#\# 1\. How to import Mermaid into draw.io / diagrams.net**

Use this file when you want to draw the database quickly in draw.io.

Steps:

\`\`\`text  
1\. Open https://app.diagrams.net/ or draw.io desktop.  
2\. Create a blank diagram.  
3\. Go to Insert → Advanced → Mermaid.  
4\. Copy only the Mermaid code inside the erDiagram block below.  
5\. Paste into draw.io.  
6\. Click Insert.  
7\. Arrange the diagram manually if needed.  
\`\`\`

Important:

\`\`\`text  
Do not copy the markdown fence \`\`\`mermaid.  
Copy from erDiagram to the last relationship line only.  
\`\`\`

\---

**\# 2\. Full ERD Mermaid**

\`\`\`mermaid  
erDiagram  
    USERS {  
        INT UserId PK  
        STRING Username UK  
        STRING PasswordHash  
        STRING FullName  
        STRING Role  
        BOOL IsActive  
        DATETIME CreatedAt  
        DATETIME UpdatedAt  
    }

    DINING\_TABLES {  
        INT TableId PK  
        STRING TableName  
        STRING Area  
        STRING QrToken UK  
        STRING Status  
        BOOL IsActive  
        DATETIME CreatedAt  
        DATETIME UpdatedAt  
    }

    CATEGORIES {  
        INT CategoryId PK  
        STRING CategoryName  
        STRING Description  
        INT DisplayOrder  
        BOOL IsActive  
        DATETIME CreatedAt  
        DATETIME UpdatedAt  
    }

    MENU\_ITEMS {  
        INT MenuItemId PK  
        INT CategoryId FK  
        STRING ItemName  
        STRING Description  
        DECIMAL Price  
        STRING ImageUrl  
        BOOL IsActive  
        BOOL IsAvailable  
        BOOL TrackStock  
        INT AvailableQuantity  
        BOOL CanOrderStandalone  
        BINARY RowVersion  
        DATETIME CreatedAt  
        DATETIME UpdatedAt  
    }

    MENU\_ADDON\_GROUPS {  
        INT MenuAddonGroupId PK  
        INT ParentMenuItemId FK  
        STRING GroupName  
        BOOL IsRequired  
        INT MinSelect  
        INT MaxSelect  
        INT DisplayOrder  
        BOOL IsActive  
        DATETIME CreatedAt  
        DATETIME UpdatedAt  
    }

    MENU\_ADDON\_OPTIONS {  
        INT MenuAddonOptionId PK  
        INT MenuAddonGroupId FK  
        INT AddonMenuItemId FK  
        DECIMAL ExtraPriceOverride  
        BOOL IsDefault  
        INT DisplayOrder  
        BOOL IsActive  
        DATETIME CreatedAt  
        DATETIME UpdatedAt  
    }

    TABLE\_SESSIONS {  
        INT TableSessionId PK  
        INT TableId FK  
        DATETIME StartedAt  
        DATETIME EndedAt  
        STRING Status  
        INT OpenedBy FK  
        INT ClosedBy FK  
    }

    TABLE\_SESSION\_CUSTOMERS {  
        INT SessionCustomerId PK  
        INT TableSessionId FK  
        STRING ClientToken  
        STRING DisplayName  
        DATETIME CreatedAt  
    }

    ORDERS {  
        INT OrderId PK  
        INT TableSessionId FK  
        INT SessionCustomerId FK  
        STRING OrderCode UK  
        STRING OrderSource  
        STRING ClientToken  
        STRING Status  
        STRING PrintStatus  
        STRING CustomerNote  
        STRING SystemNote  
        STRING CancelReason  
        DATETIME CreatedAt  
        DATETIME UpdatedAt  
        DATETIME CancelledAt  
        DATETIME PrintedAt  
        STRING PrintError  
        INT PrintRetryCount  
        INT CreatedBy FK  
        INT CancelledBy FK  
    }

    ORDER\_ITEMS {  
        INT OrderItemId PK  
        INT OrderId FK  
        INT MenuItemId FK  
        INT SessionCustomerId FK  
        INT ParentOrderItemId FK  
        STRING LineType  
        STRING ItemName  
        INT Quantity  
        DECIMAL UnitPrice  
        DECIMAL TotalPrice  
        STRING Note  
        DATETIME CreatedAt  
        DATETIME UpdatedAt  
    }

    SERVICE\_REQUESTS {  
        INT RequestId PK  
        INT TableSessionId FK  
        INT SessionCustomerId FK  
        STRING ClientToken  
        STRING RequestType  
        STRING Reason  
        STRING PaymentMethod  
        STRING Message  
        STRING Status  
        DATETIME CreatedAt  
        DATETIME ConfirmedAt  
        DATETIME CompletedAt  
        INT ConfirmedBy FK  
        INT CompletedBy FK  
    }

    BILLS {  
        INT BillId PK  
        STRING BillCode UK  
        INT TableSessionId FK  
        INT BillNo  
        STRING BillName  
        BOOL IsDefault  
        STRING Status  
        DECIMAL SubTotal  
        DECIMAL DiscountAmount  
        DECIMAL FinalAmount  
        DATETIME CreatedAt  
        INT CreatedBy FK  
        DATETIME PaidAt  
        DATETIME CancelledAt  
        INT CancelledBy FK  
        STRING CancelReason  
    }

    BILL\_DETAILS {  
        INT BillDetailId PK  
        INT BillId FK  
        INT OrderItemId FK  
        INT MenuItemId FK  
        INT SessionCustomerId FK  
        STRING CustomerDisplayName  
        STRING ItemName  
        INT Quantity  
        DECIMAL UnitPrice  
        DECIMAL TotalPrice  
        DATETIME CreatedAt  
    }

    PAYMENTS {  
        INT PaymentId PK  
        INT BillId FK\_UK  
        STRING PaymentMethod  
        DECIMAL Amount  
        DATETIME PaidAt  
        INT ConfirmedBy FK  
        DATETIME UpdatedAt  
        INT UpdatedBy FK  
        STRING ChangeReason  
    }

    USERS ||--o{ TABLE\_SESSIONS : opens  
    USERS ||--o{ TABLE\_SESSIONS : closes  
    USERS ||--o{ ORDERS : creates  
    USERS ||--o{ ORDERS : cancels  
    USERS ||--o{ SERVICE\_REQUESTS : confirms  
    USERS ||--o{ SERVICE\_REQUESTS : completes  
    USERS ||--o{ BILLS : creates  
    USERS ||--o{ BILLS : cancels  
    USERS ||--o{ PAYMENTS : confirms  
    USERS ||--o{ PAYMENTS : corrects

    DINING\_TABLES ||--o{ TABLE\_SESSIONS : has

    CATEGORIES ||--o{ MENU\_ITEMS : contains  
    MENU\_ITEMS ||--o{ MENU\_ADDON\_GROUPS : parent\_item\_has  
    MENU\_ADDON\_GROUPS ||--o{ MENU\_ADDON\_OPTIONS : contains  
    MENU\_ITEMS ||--o{ MENU\_ADDON\_OPTIONS : addon\_item

    TABLE\_SESSIONS ||--o{ TABLE\_SESSION\_CUSTOMERS : has  
    TABLE\_SESSIONS ||--o{ ORDERS : has  
    TABLE\_SESSION\_CUSTOMERS ||--o{ ORDERS : places  
    TABLE\_SESSIONS ||--o{ SERVICE\_REQUESTS : has  
    TABLE\_SESSION\_CUSTOMERS ||--o{ SERVICE\_REQUESTS : sends

    ORDERS ||--o{ ORDER\_ITEMS : contains  
    MENU\_ITEMS ||--o{ ORDER\_ITEMS : ordered\_as  
    TABLE\_SESSION\_CUSTOMERS ||--o{ ORDER\_ITEMS : owns  
    ORDER\_ITEMS ||--o{ ORDER\_ITEMS : parent\_addon

    TABLE\_SESSIONS ||--o{ BILLS : has  
    BILLS ||--o{ BILL\_DETAILS : contains  
    ORDER\_ITEMS ||--o{ BILL\_DETAILS : billed\_from  
    MENU\_ITEMS ||--o{ BILL\_DETAILS : billed\_item  
    TABLE\_SESSION\_CUSTOMERS ||--o{ BILL\_DETAILS : customer\_group  
    BILLS ||--o| PAYMENTS : paid\_by  
\`\`\`

\---

**\# 3\. Table Explanation**

**\#\# 3.1. Users**

Owned by Member 1\.

Purpose:

\`\`\`text  
Login, role, permission and audit user.  
\`\`\`

Used by other tables for audit:

\`\`\`text  
OpenedBy, ClosedBy, CreatedBy, CancelledBy, ConfirmedBy, CompletedBy, UpdatedBy  
\`\`\`

\---

**\#\# 3.2. DiningTables**

Owned by Member 2\.

Purpose:

\`\`\`text  
Restaurant table master data and QR token.  
\`\`\`

Important rule:

\`\`\`text  
Orders, ServiceRequests and Bills do not store TableId directly.  
They reach table through TableSessionId → TableSessions.TableId.  
\`\`\`

\---

**\#\# 3.3. Categories**

Owned by Member 3\.

Purpose:

\`\`\`text  
Group menu items for WPF and Customer Web.  
\`\`\`

Examples:

\`\`\`text  
Món chính  
Đồ uống  
Món phụ  
Combo  
\`\`\`

\---

**\#\# 3.4. MenuItems**

Owned by Member 3\.

Purpose:

\`\`\`text  
Store both main dishes and add-on dishes.  
\`\`\`

Important fields:

\`\`\`text  
IsActive: item exists in menu operation.  
IsAvailable: item can be ordered now.  
TrackStock: item uses stock quantity.  
AvailableQuantity: stock count if TrackStock \= true.  
CanOrderStandalone: true for normal menu item, false for add-on-only item.  
\`\`\`

Example:

| ItemName | CanOrderStandalone | Meaning |  
|---|---:|---|  
| Cơm gà | true | Customer can order directly |  
| Trứng ốp la | false | Only selectable as add-on |  
| Trà đào | true | Customer can order directly |

\---

**\#\# 3.5. MenuAddonGroups**

Owned by Member 3\.

Purpose:

\`\`\`text  
Define add-on groups for a main dish.  
\`\`\`

Example:

\`\`\`text  
ParentMenuItem \= Cơm gà  
GroupName \= Món phụ thêm  
MinSelect \= 0  
MaxSelect \= 3  
\`\`\`

\---

**\#\# 3.6. MenuAddonOptions**

Owned by Member 3\.

Purpose:

\`\`\`text  
Define which menu items are allowed as add-ons in a group.  
\`\`\`

Example:

\`\`\`text  
Cơm gà → Món phụ thêm → Trứng ốp la  
Cơm gà → Món phụ thêm → Canh thêm  
\`\`\`

\`ExtraPriceOverride\` rule:

\`\`\`text  
null: use MenuItems.Price.  
0: free add-on, useful for combo.  
positive value: override add-on price.  
\`\`\`

\---

**\#\# 3.7. TableSessions**

Owned by Member 4\.

Purpose:

\`\`\`text  
One serving session at one table.  
\`\`\`

Important rule:

\`\`\`text  
One table can have at most one active session.  
Active session \= Open or WaitingPayment.  
\`\`\`

\---

**\#\# 3.8. TableSessionCustomers**

Owned by Member 4\.

Purpose:

\`\`\`text  
Identify customer browser/device inside a table session.  
\`\`\`

Important rule:

\`\`\`text  
One ClientToken maps to one TableSessionCustomer inside one session.  
\`\`\`

\---

**\#\# 3.9. Orders**

Owned by Member 4\.

Purpose:

\`\`\`text  
Order header for one order submission.  
\`\`\`

Important rule:

\`\`\`text  
Order.Status \= Accepted or Cancelled.  
Order.PrintStatus \= PendingPrint, Printed, or PrintFailed.  
\`\`\`

\---

**\#\# 3.10. OrderItems**

Owned by Member 4\.

Purpose:

\`\`\`text  
Snapshot lines of ordered items.  
\`\`\`

Main/add-on structure:

\`\`\`text  
Main item: ParentOrderItemId \= null, LineType \= Main.  
Add-on item: ParentOrderItemId points to main OrderItemId, LineType \= Addon.  
\`\`\`

Example:

| OrderItemId | ParentOrderItemId | LineType | ItemName |  
|---:|---:|---|---|  
| 1 | null | Main | Cơm gà |  
| 2 | 1 | Addon | Trứng ốp la |  
| 3 | 1 | Addon | Canh thêm |

\---

**\#\# 3.11. ServiceRequests**

Owned by Member 4\.

Purpose:

\`\`\`text  
Customer calls staff or requests payment.  
\`\`\`

Request types:

\`\`\`text  
CallStaff  
PaymentRequest  
\`\`\`

Status flow:

\`\`\`text  
Pending → Confirmed → Completed  
\`\`\`

\---

**\#\# 3.12. Bills**

Owned by Member 4\.

Purpose:

\`\`\`text  
Bill header for one bill inside a TableSession.  
\`\`\`

Important rules:

\`\`\`text  
Each active session has a default unpaid bill.  
Only Unpaid bill can be split or paid.  
Paid bill is locked.  
Cancelled bill is excluded from revenue.  
\`\`\`

\---

**\#\# 3.13. BillDetails**

Owned by Member 4\.

Purpose:

\`\`\`text  
Snapshot item rows inside a bill.  
\`\`\`

Important rule:

\`\`\`text  
BillDetails are created from OrderItems snapshot.  
They must not use latest MenuItems price/name.  
\`\`\`

\---

**\#\# 3.14. Payments**

Core creation is owned by Member 4\. Admin correction is owned by Member 5\.

Purpose:

\`\`\`text  
Payment record for one paid bill.  
\`\`\`

Important rules:

\`\`\`text  
One bill has at most one payment.  
Payment.Amount \= Bills.FinalAmount.  
Member 5 can correct only PaymentMethod if Admin.  
\`\`\`

\---

**\# 4\. Database Ownership Summary**

| Table | Owner | Write permission |  
|---|---|---|  
| \`Users\` | Member 1 | Member 1 only |  
| \`DiningTables\` | Member 2 | Member 2, status via contract |  
| \`Categories\` | Member 3 | Member 3 |  
| \`MenuItems\` | Member 3 | Member 3 |  
| \`MenuAddonGroups\` | Member 3 | Member 3 |  
| \`MenuAddonOptions\` | Member 3 | Member 3 |  
| \`TableSessions\` | Member 4 | Member 4 |  
| \`TableSessionCustomers\` | Member 4 | Member 4 |  
| \`Orders\` | Member 4 | Member 4 |  
| \`OrderItems\` | Member 4 | Member 4 |  
| \`ServiceRequests\` | Member 4 | Member 4 |  
| \`Bills\` | Member 4 | Member 4 |  
| \`BillDetails\` | Member 4 | Member 4 |  
| \`Payments\` | Member 4 core / Member 5 correction | Member 4 creates; Member 5 corrects method only |

\---

**\# 5\. Important Constraints**

\`\`\`text  
UNIQUE Users.Username  
UNIQUE DiningTables.QrToken  
UNIQUE Orders.OrderCode  
UNIQUE Bills.BillCode  
UNIQUE Payments.BillId  
UNIQUE TableSessionCustomers(TableSessionId, ClientToken)  
UNIQUE active TableSession per TableId where Status in Open/WaitingPayment  
CHECK Price \>= 0  
CHECK Quantity \> 0  
CHECK TotalPrice \= Quantity \* UnitPrice should be enforced by service  
CHECK Payment.Amount \= Bill.FinalAmount should be enforced by service  
\`\`\`

\---

**\# 6\. Import-ready Mermaid only**

The file below can be created as \`.mmd\` if needed. Copy everything from \`erDiagram\` downward.

