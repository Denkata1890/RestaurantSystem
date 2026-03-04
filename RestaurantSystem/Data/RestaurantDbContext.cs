namespace RestaurantSystem.Data;
using Microsoft.EntityFrameworkCore;
using RestaurantSystem.Data;
using RestaurantSystem.Models;

public class RestaurantDbContext:DbContext
{
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<RestaurantTable> Tables { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MenuItem>()
            .Property(m => m.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderDetail>()
            .Property(od => od.SubTotal)
            .HasPrecision(18, 2);

        
        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .HasDatabaseName("IX_Category_Name");

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Category_IsActive");

       
        modelBuilder.Entity<MenuItem>()
            .HasIndex(m => m.Title)
            .HasDatabaseName("IX_MenuItem_Title");

        modelBuilder.Entity<MenuItem>()
            .HasIndex(m => m.CategoryId)
            .HasDatabaseName("IX_MenuItem_CategoryId");

        modelBuilder.Entity<MenuItem>()
            .HasIndex(m => m.Price)
            .HasDatabaseName("IX_MenuItem_Price");

        modelBuilder.Entity<MenuItem>()
            .HasIndex(m => m.InternalBarcode)
            .IsUnique()
            .HasDatabaseName("IX_MenuItem_Barcode");


        modelBuilder.Entity<RestaurantTable>()
            .HasIndex(t => t.Number)
            .IsUnique()
            .HasDatabaseName("IX_RestaurantTable_Number");

        modelBuilder.Entity<RestaurantTable>()
            .HasIndex(t => t.Zone)
            .HasDatabaseName("IX_RestaurantTable_Zone");

        modelBuilder.Entity<RestaurantTable>()
            .HasIndex(t => t.IsAvailable)
            .HasDatabaseName("IX_RestaurantTable_IsAvailable");

       
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.FullName)
            .HasDatabaseName("IX_Employee_FullName");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Role)
            .HasDatabaseName("IX_Employee_Role");

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Phone)
            .IsUnique()
            .HasDatabaseName("IX_Employee_Phone");

        
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderTime)
            .HasDatabaseName("IX_Order_OrderTime");

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.TableId)
            .HasDatabaseName("IX_Order_TableId");

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.EmployeeId)
            .HasDatabaseName("IX_Order_EmployeeId");

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Status)
            .HasDatabaseName("IX_Order_Status");

        modelBuilder.Entity<OrderDetail>()
            .HasIndex(od => od.OrderId)
            .HasDatabaseName("IX_OrderDetail_OrderId");

        modelBuilder.Entity<OrderDetail>()
            .HasIndex(od => od.MenuItemId)
            .HasDatabaseName("IX_OrderDetail_MenuItemId");

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("UX_Category_Name");

        modelBuilder.Entity<MenuItem>()
            .HasOne(m => m.Category)
            .WithMany(c => c.MenuItems)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);  

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.MenuItem)
            .WithMany(m => m.OrderDetails)
            .HasForeignKey(od => od.MenuItemId)
            .OnDelete(DeleteBehavior.Restrict);  


        modelBuilder.Entity<Order>()
            .HasOne(o => o.Table)
            .WithMany(t => t.Orders)
            .HasForeignKey(o => o.TableId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Waiter)
            .WithMany(e => e.Orders)
            .HasForeignKey(o => o.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Category>()
            .Property(c => c.CreatedDate)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Category>()
            .Property(c => c.IsActive)
            .HasDefaultValue(true);

        modelBuilder.Entity<MenuItem>()
            .Property(m => m.IsAvailable)
            .HasDefaultValue(true);

        modelBuilder.Entity<RestaurantTable>()
            .Property(t => t.IsAvailable)
            .HasDefaultValue(true);

        modelBuilder.Entity<RestaurantTable>()
            .Property(t => t.LastCleaned)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Order>()
            .Property(o => o.OrderTime)
            .HasDefaultValueSql("GETDATE()");

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasDefaultValue("Нова");

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasDefaultValue(0);

        modelBuilder.Entity<OrderDetail>()
            .Property(od => od.SubTotal)
            .IsRequired();

        modelBuilder.Entity<MenuItem>()
            .Property(m => m.Title)
            .HasMaxLength(100);

        modelBuilder.Entity<Category>()
            .Property(c => c.Name)
            .IsRequired();

        modelBuilder.Entity<MenuItem>()
            .Property(m => m.Title)
            .IsRequired();

        modelBuilder.Entity<RestaurantTable>()
            .Property(t => t.Number)
            .IsRequired();

        modelBuilder.Entity<Employee>()
            .Property(e => e.FullName)
            .IsRequired();

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .IsRequired();


        //user
        modelBuilder.Entity<User>()
    .HasIndex(u => u.Username)
    .IsUnique()
    .HasDatabaseName("IX_User_Username");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_User_Email");
    }
}
