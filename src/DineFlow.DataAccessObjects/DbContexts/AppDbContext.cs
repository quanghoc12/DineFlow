using DineFlow.BusinessObjects.Auth;
using DineFlow.BusinessObjects.Bills;
using DineFlow.BusinessObjects.Menu;
using DineFlow.BusinessObjects.Orders;
using DineFlow.BusinessObjects.Requests;
using DineFlow.BusinessObjects.Tables;
using Microsoft.EntityFrameworkCore;

namespace DineFlow.DataAccessObjects.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<DiningTable> DiningTables => Set<DiningTable>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<ChoiceGroup> ChoiceGroups => Set<ChoiceGroup>();
    public DbSet<ChoiceItem> ChoiceItems => Set<ChoiceItem>();
    public DbSet<MenuItemChoiceGroup> MenuItemChoiceGroups => Set<MenuItemChoiceGroup>();
    public DbSet<SalesChannel> SalesChannels => Set<SalesChannel>();
    public DbSet<MenuItemChannelPrice> MenuItemChannelPrices => Set<MenuItemChannelPrice>();
    public DbSet<ChoiceItemChannelPrice> ChoiceItemChannelPrices => Set<ChoiceItemChannelPrice>();
    public DbSet<TableSession> TableSessions => Set<TableSession>();
    public DbSet<TableSessionCustomer> TableSessionCustomers => Set<TableSessionCustomer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemSelectedChoice> OrderItemSelectedChoices => Set<OrderItemSelectedChoice>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<Bill> Bills => Set<Bill>();
    public DbSet<BillDetail> BillDetails => Set<BillDetail>();
    public DbSet<BillDetailAdjustment> BillDetailAdjustments => Set<BillDetailAdjustment>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureAuth(modelBuilder);
        ConfigureTables(modelBuilder);
        ConfigureMenu(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureRequests(modelBuilder);
        ConfigureBills(modelBuilder);
    }

    private static void ConfigureAuth(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.UserId);
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(30).IsRequired();
        });
    }

    private static void ConfigureTables(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Area>(entity =>
        {
            entity.HasKey(x => x.AreaId);
            entity.HasIndex(x => x.AreaName).IsUnique();
            entity.Property(x => x.AreaName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<DiningTable>(entity =>
        {
            entity.HasKey(x => x.TableId);
            entity.HasIndex(x => x.QrToken).IsUnique();
            entity.Property(x => x.TableName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Area).HasMaxLength(100).IsRequired();
            entity.Property(x => x.QrToken).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.DisplayOrder).HasDefaultValue(0);

            entity.HasOne(x => x.AreaEntity)
                .WithMany(x => x.DiningTables)
                .HasForeignKey(x => x.AreaId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureMenu(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.CategoryId);
            entity.Property(x => x.CategoryName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(x => x.MenuItemId);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.BasePrice).HasPrecision(18, 2);
            entity.Property(x => x.ImageUrl).HasMaxLength(1000);
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);

            entity.HasOne(x => x.Category)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.CategoryId);
        });

        modelBuilder.Entity<ChoiceGroup>(entity =>
        {
            entity.HasKey(x => x.ChoiceGroupId);
            entity.Property(x => x.GroupName).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<ChoiceItem>(entity =>
        {
            entity.HasKey(x => x.ChoiceItemId);
            entity.Property(x => x.ChoiceName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ExtraPrice).HasPrecision(18, 2);

            entity.HasOne(x => x.ChoiceGroup)
                .WithMany(x => x.ChoiceItems)
                .HasForeignKey(x => x.ChoiceGroupId);
        });

        modelBuilder.Entity<SalesChannel>(entity =>
        {
            entity.HasKey(x => x.SalesChannelId);
            entity.HasIndex(x => x.ChannelCode).IsUnique().HasFilter("\"IsDeleted\" = FALSE");
            entity.Property(x => x.ChannelCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ChannelName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
        });

        modelBuilder.Entity<MenuItemChannelPrice>(entity =>
        {
            entity.HasKey(x => new { x.MenuItemId, x.SalesChannelId });
            entity.Property(x => x.ChannelExtraPrice).HasPrecision(18, 2);

            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.ChannelPrices)
                .HasForeignKey(x => x.MenuItemId);

            entity.HasOne(x => x.SalesChannel)
                .WithMany(x => x.MenuItemChannelPrices)
                .HasForeignKey(x => x.SalesChannelId);
        });

        modelBuilder.Entity<ChoiceItemChannelPrice>(entity =>
        {
            entity.HasKey(x => new { x.ChoiceItemId, x.SalesChannelId });
            entity.Property(x => x.ChannelExtraPrice).HasPrecision(18, 2);

            entity.HasOne(x => x.ChoiceItem)
                .WithMany(x => x.ChannelPrices)
                .HasForeignKey(x => x.ChoiceItemId);

            entity.HasOne(x => x.SalesChannel)
                .WithMany(x => x.ChoiceItemChannelPrices)
                .HasForeignKey(x => x.SalesChannelId);
        });

        modelBuilder.Entity<MenuItemChoiceGroup>(entity =>
        {
            entity.HasKey(x => new { x.MenuItemId, x.ChoiceGroupId });

            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.MenuItemChoiceGroups)
                .HasForeignKey(x => x.MenuItemId);

            entity.HasOne(x => x.ChoiceGroup)
                .WithMany(x => x.MenuItemChoiceGroups)
                .HasForeignKey(x => x.ChoiceGroupId);
        });
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TableSession>(entity =>
        {
            entity.HasKey(x => x.TableSessionId);
            entity.HasIndex(x => x.TableId)
                .IsUnique()
                .HasFilter("\"Status\" IN ('Browsing', 'Open', 'WaitingPayment')");
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(x => x.Table)
                .WithMany(x => x.TableSessions)
                .HasForeignKey(x => x.TableId);
        });

        modelBuilder.Entity<TableSessionCustomer>(entity =>
        {
            entity.HasKey(x => x.SessionCustomerId);
            entity.HasIndex(x => new { x.TableSessionId, x.ClientToken }).IsUnique();
            entity.Property(x => x.ClientToken).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(150);

            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.Customers)
                .HasForeignKey(x => x.TableSessionId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.OrderId);
            entity.HasIndex(x => x.OrderCode).IsUnique();
            entity.Property(x => x.OrderCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.OrderSource).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ExternalOrderCode).HasMaxLength(100);
            entity.Property(x => x.ClientToken).HasMaxLength(200);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PrintStatus).HasMaxLength(30);
            entity.Property(x => x.CustomerNote).HasMaxLength(500);
            entity.Property(x => x.SystemNote).HasMaxLength(500);
            entity.Property(x => x.CancelReason).HasMaxLength(500);
            entity.Property(x => x.PrintError).HasMaxLength(1000);

            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.TableSessionId);

            entity.HasOne(x => x.SessionCustomer)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.SessionCustomerId);

            entity.HasOne(x => x.SalesChannel)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.SalesChannelId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(x => x.OrderItemId);
            entity.HasIndex(x => x.OrderId);
            entity.Property(x => x.MenuItemNameSnapshot).HasMaxLength(150).IsRequired();
            entity.Property(x => x.BasePriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.ChannelExtraPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.FinalUnitPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.Note).HasMaxLength(300);

            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.OrderId);

            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.MenuItemId);
        });

        modelBuilder.Entity<OrderItemSelectedChoice>(entity =>
        {
            entity.HasKey(x => x.OrderItemSelectedChoiceId);
            entity.HasIndex(x => x.OrderItemId);
            entity.Property(x => x.GroupNameSnapshot).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ChoiceNameSnapshot).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ExtraPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.ChannelExtraPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.FinalExtraPriceSnapshot).HasPrecision(18, 2);

            entity.HasOne(x => x.OrderItem)
                .WithMany(x => x.SelectedChoices)
                .HasForeignKey(x => x.OrderItemId);

            entity.HasOne(x => x.ChoiceGroup)
                .WithMany(x => x.OrderItemSelectedChoices)
                .HasForeignKey(x => x.ChoiceGroupId);

            entity.HasOne(x => x.ChoiceItem)
                .WithMany(x => x.OrderItemSelectedChoices)
                .HasForeignKey(x => x.ChoiceItemId);
        });
    }

    private static void ConfigureRequests(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(x => x.RequestId);
            entity.Property(x => x.ClientToken).HasMaxLength(200);
            entity.Property(x => x.RequestType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500);
            entity.Property(x => x.PaymentMethod).HasMaxLength(30);
            entity.Property(x => x.Message).HasMaxLength(500);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.ServiceRequests)
                .HasForeignKey(x => x.TableSessionId);

            entity.HasOne(x => x.SessionCustomer)
                .WithMany(x => x.ServiceRequests)
                .HasForeignKey(x => x.SessionCustomerId);
        });
    }

    private static void ConfigureBills(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.HasKey(x => x.BillId);
            entity.HasIndex(x => x.BillCode).IsUnique();
            entity.HasIndex(x => x.TableSessionId)
                .IsUnique()
                .HasFilter("\"IsDefault\" = TRUE AND \"Status\" = 'Unpaid'");
            entity.Property(x => x.BillCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.SalesChannelCodeSnapshot).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SalesChannelNameSnapshot).HasMaxLength(120).IsRequired();
            entity.Property(x => x.BillName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.SubTotal).HasPrecision(18, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            entity.Property(x => x.FinalAmount).HasPrecision(18, 2);
            entity.Property(x => x.CancelReason).HasMaxLength(500);

            entity.HasOne(x => x.TableSession)
                .WithMany(x => x.Bills)
                .HasForeignKey(x => x.TableSessionId);

            entity.HasOne(x => x.SalesChannel)
                .WithMany()
                .HasForeignKey(x => x.SalesChannelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BillDetail>(entity =>
        {
            entity.HasKey(x => x.BillDetailId);
            entity.HasIndex(x => x.BillId);
            entity.HasIndex(x => x.MenuItemId);
            entity.Property(x => x.ItemName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ChoiceSummary).HasMaxLength(500);
            entity.Property(x => x.Note).HasMaxLength(300);
            entity.Property(x => x.BasePriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.MenuItemChannelExtraPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.ChoiceExtraPriceSnapshot).HasPrecision(18, 2);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(x => x.Bill)
                .WithMany(x => x.BillDetails)
                .HasForeignKey(x => x.BillId);

            entity.HasOne(x => x.MenuItem)
                .WithMany(x => x.BillDetails)
                .HasForeignKey(x => x.MenuItemId);

            entity.HasOne<DineFlow.BusinessObjects.Menu.SalesChannel>()
                .WithMany()
                .HasForeignKey(x => x.SalesChannelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<BillDetailAdjustment>(entity =>
        {
            entity.HasKey(x => x.BillDetailAdjustmentId);
            entity.HasIndex(x => x.BillId);
            entity.HasIndex(x => x.BillDetailId);
            entity.HasIndex(x => x.MenuItemId);
            entity.Property(x => x.ItemName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ChangeType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();

            entity.HasOne(x => x.Bill)
                .WithMany()
                .HasForeignKey(x => x.BillId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.PaymentId);
            entity.HasIndex(x => x.BillId);
            entity.Property(x => x.PaymentMethod).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.ChangeReason).HasMaxLength(500);

            entity.HasOne(x => x.Bill)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.BillId);
        });
    }
}
