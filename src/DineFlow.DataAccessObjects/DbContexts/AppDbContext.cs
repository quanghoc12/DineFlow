using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Bills;
using DineFlow.BusinessObjects.Common;
using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Requests;
using DineFlow.BusinessObjects.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DineFlow.DataAccessObjects.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<DiningTable> DiningTables => Set<DiningTable>();
    public DbSet<TableSession> TableSessions => Set<TableSession>();
    public DbSet<TableSessionCustomer> TableSessionCustomers => Set<TableSessionCustomer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillDetail> BillDetails => Set<BillDetail>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.example.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=DineFlowDb;Trusted_Connection=True;TrustServerCertificate=True";

        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureTables(modelBuilder);
        ConfigureMenu(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureRequests(modelBuilder);
        ConfigureBills(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        });
    }

    private static void ConfigureTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiningTable>(entity =>
        {
            entity.HasKey(x => x.TableId);
            entity.HasIndex(x => x.QrToken).IsUnique();
            entity.Property(x => x.TableName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Area).HasMaxLength(50);
            entity.Property(x => x.QrToken).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<TableSession>(entity =>
        {
            entity.HasKey(x => x.TableSessionId);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Table)
                .WithMany(x => x.TableSessions)
                .HasForeignKey(x => x.TableId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OpenedByUser)
                .WithMany()
                .HasForeignKey(x => x.OpenedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TableId)
                .IsUnique()
                .HasFilter("[Status] IN ('Open', 'WaitingPayment')")
                .HasDatabaseName("UX_TableSessions_OneOpenSessionPerTable");
        });

        modelBuilder.Entity<TableSessionCustomer>(entity =>
        {
            entity.HasKey(x => x.SessionCustomerId);
            entity.Property(x => x.ClientToken).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(50);
            entity.HasIndex(x => new { x.TableSessionId, x.ClientToken }).IsUnique();
            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.TableSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureMenu(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.CategoryId);
            entity.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(x => x.MenuItemId);
            entity.Property(x => x.ItemName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.CategoryId, x.IsActive, x.IsAvailable });
            entity.HasOne(x => x.Category)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.OrderId);
            entity.HasIndex(x => x.OrderCode).IsUnique();
            entity.HasIndex(x => new { x.TableSessionId, x.CreatedAt });
            entity.HasIndex(x => new { x.PrintStatus, x.CreatedAt });
            entity.Property(x => x.OrderCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.OrderSource).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.PrintStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.ClientToken).HasMaxLength(100);
            entity.Property(x => x.CustomerNote).HasMaxLength(500);
            entity.Property(x => x.SystemNote).HasMaxLength(500);
            entity.Property(x => x.CancelReason).HasMaxLength(500);
            entity.Property(x => x.PrintError).HasMaxLength(1000);
            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.TableSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SessionCustomer)
                .WithMany()
                .HasForeignKey(x => x.SessionCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(x => x.OrderItemId);
            entity.Property(x => x.ItemName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.HasIndex(x => new { x.OrderId, x.SessionCustomerId });
            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.MenuItem)
                .WithMany()
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SessionCustomer)
                .WithMany()
                .HasForeignKey(x => x.SessionCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRequests(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(x => x.RequestId);
            entity.Property(x => x.RequestType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.ClientToken).HasMaxLength(100);
            entity.Property(x => x.Reason).HasMaxLength(255);
            entity.Property(x => x.Message).HasMaxLength(500);
            entity.HasIndex(x => new { x.TableSessionId, x.Status });
            entity.HasIndex(x => new { x.RequestType, x.Status });
            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.ServiceRequests)
                .HasForeignKey(x => x.TableSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SessionCustomer)
                .WithMany()
                .HasForeignKey(x => x.SessionCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ConfirmedByUser)
                .WithMany()
                .HasForeignKey(x => x.ConfirmedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CompletedByUser)
                .WithMany()
                .HasForeignKey(x => x.CompletedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(x => x.BillId);
            entity.HasIndex(x => x.BillCode).IsUnique();
            entity.HasIndex(x => new { x.TableSessionId, x.BillNo })
                .IsUnique()
                .HasFilter("[Status] <> 'Cancelled'")
                .HasDatabaseName("UX_Bills_TableSessionId_BillNo_Active");
            entity.HasIndex(x => x.TableSessionId)
                .IsUnique()
                .HasFilter("[Status] = 'Unpaid' AND [IsDefault] = 1")
                .HasDatabaseName("UX_Bills_OneDefaultUnpaidBillPerSession");
            entity.Property(x => x.BillCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.BillName).HasMaxLength(100);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.SubTotal).HasColumnType("decimal(18,2)");
            entity.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.FinalAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.Bills)
                .HasForeignKey(x => x.TableSessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CancelledByUser)
                .WithMany()
                .HasForeignKey(x => x.CancelledBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BillDetail>(entity =>
        {
            entity.HasKey(x => x.BillDetailId);
            entity.Property(x => x.CustomerDisplayName).HasMaxLength(50);
            entity.Property(x => x.ItemName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
            entity.HasIndex(x => new { x.BillId, x.SessionCustomerId });
            entity.HasIndex(x => x.OrderItemId);
            entity.HasOne(x => x.Bill)
                .WithMany(x => x.BillDetails)
                .HasForeignKey(x => x.BillId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.OrderItem)
                .WithMany()
                .HasForeignKey(x => x.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.MenuItem)
                .WithMany()
                .HasForeignKey(x => x.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SessionCustomer)
                .WithMany()
                .HasForeignKey(x => x.SessionCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.PaymentId);
            entity.HasIndex(x => x.BillId).IsUnique();
            entity.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.ChangeReason).HasMaxLength(500);
            entity.HasOne(x => x.Bill)
                .WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.BillId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ConfirmedByUser)
                .WithMany()
                .HasForeignKey(x => x.ConfirmedBy)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.UpdatedByUser)
                .WithMany()
                .HasForeignKey(x => x.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
