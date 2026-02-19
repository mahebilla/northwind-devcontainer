using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;
using NorthwindCqrs.Domain.Entities;

namespace NorthwindCqrs.Infrastructure.Persistence.Write;

// Connects to the Northwind (normalized) Write database.
// Implements IWriteDbContext — command handlers depend on the interface, not this class.
// Only maps the 7 entities needed for Phase 1 controllers.
// No navigation properties, no global query filters, no views — minimal and focused.
public class WriteDbContext : DbContext, IWriteDbContext
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }

    public DbSet<Product>     Products     => Set<Product>();
    public DbSet<Category>    Categories   => Set<Category>();
    public DbSet<Customer>    Customers    => Set<Customer>();
    public DbSet<Order>       Orders       => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Employee>    Employees    => Set<Employee>();
    public DbSet<Shipper>     Shippers     => Set<Shipper>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Product ───────────────────────────────────────────────────────
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.ProductName).HasMaxLength(40);
            entity.Property(e => e.QuantityPerUnit).HasMaxLength(20);
            entity.Property(e => e.UnitPrice).HasColumnType("money").HasDefaultValue(0m);
            entity.Property(e => e.UnitsInStock).HasDefaultValue((short)0);
            entity.Property(e => e.UnitsOnOrder).HasDefaultValue((short)0);
            entity.Property(e => e.ReorderLevel).HasDefaultValue((short)0);
        });

        // ── Category ──────────────────────────────────────────────────────
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(15);
        });

        // ── Customer ──────────────────────────────────────────────────────
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CustomerId)
                .HasMaxLength(5).IsFixedLength().HasColumnName("CustomerID");
            entity.Property(e => e.CompanyName).HasMaxLength(40);
            entity.Property(e => e.ContactName).HasMaxLength(30);
            entity.Property(e => e.City).HasMaxLength(15);
            entity.Property(e => e.Country).HasMaxLength(15);
        });

        // ── Order ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(5).IsFixedLength().HasColumnName("CustomerID");
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.Freight).HasColumnType("money").HasDefaultValue(0m);
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
            entity.Property(e => e.ShippedDate).HasColumnType("datetime");
            entity.Property(e => e.RequiredDate).HasColumnType("datetime");
            entity.Property(e => e.ShipCountry).HasMaxLength(15);
            entity.Property(e => e.ShipName).HasMaxLength(40);
            entity.Property(e => e.ShipAddress).HasMaxLength(60);
            entity.Property(e => e.ShipCity).HasMaxLength(15);
            entity.Property(e => e.ShipRegion).HasMaxLength(15);
            entity.Property(e => e.ShipPostalCode).HasMaxLength(10);
        });

        // ── OrderDetail ───────────────────────────────────────────────────
        // IMPORTANT: The actual SQL Server table is named "Order Details" with a space.
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductId }).HasName("PK_Order_Details");
            entity.ToTable("Order Details");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.Quantity).HasDefaultValue((short)1);
            entity.Property(e => e.UnitPrice).HasColumnType("money");
        });

        // ── Employee ──────────────────────────────────────────────────────
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId);
            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.FirstName).HasMaxLength(10);
            entity.Property(e => e.LastName).HasMaxLength(20);
        });

        // ── Shipper ───────────────────────────────────────────────────────
        modelBuilder.Entity<Shipper>(entity =>
        {
            entity.HasKey(e => e.ShipperId);
            entity.Property(e => e.ShipperId).HasColumnName("ShipperID");
            entity.Property(e => e.CompanyName).HasMaxLength(40);
        });
    }
}
