using Microsoft.EntityFrameworkCore;
using NorthwindCqrs.Application.Interfaces;
using NorthwindCqrs.Application.ReadModels;

namespace NorthwindCqrs.Infrastructure.Persistence.Read;

// Connects to the NorthwindRead (denormalized projections) Read database.
// QueryTrackingBehavior.NoTracking is set globally — all read queries skip the
// identity cache, reducing memory overhead and improving performance.
// Command handlers can still Add/Update/Remove through this context to sync
// read models after writes — NoTracking only affects query behavior, not DML.
public class ReadDbContext : DbContext, IReadDbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options)
    {
        // Global NoTracking — read queries never enter the change tracker
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<ProductReadModel>   Products   => Set<ProductReadModel>();
    public DbSet<OrderReadModel>     Orders     => Set<OrderReadModel>();
    public DbSet<OrderLineReadModel> OrderLines => Set<OrderLineReadModel>();
    public DbSet<CustomerReadModel>  Customers  => Set<CustomerReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── ProductReadModel ──────────────────────────────────────────────
        modelBuilder.Entity<ProductReadModel>(entity =>
        {
            entity.HasKey(e => e.ProductId);
            entity.ToTable("ProductReadModel");
            entity.Property(e => e.ProductName).HasMaxLength(40);
            entity.Property(e => e.UnitPrice).HasColumnType("money");
            entity.Property(e => e.CategoryName).HasMaxLength(15);
        });

        // ── OrderReadModel ────────────────────────────────────────────────
        modelBuilder.Entity<OrderReadModel>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.ToTable("OrderReadModel");
            entity.Property(e => e.CustomerId).HasMaxLength(5).IsFixedLength();
            entity.Property(e => e.CustomerName).HasMaxLength(40);
            entity.Property(e => e.Freight).HasColumnType("money");
            entity.Property(e => e.ShipCountry).HasMaxLength(15);
            entity.Property(e => e.OrderDate).HasColumnType("datetime");
        });

        // ── OrderLineReadModel ────────────────────────────────────────────
        modelBuilder.Entity<OrderLineReadModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("OrderLineReadModel");
            entity.Property(e => e.ProductName).HasMaxLength(40);
            entity.Property(e => e.UnitPrice).HasColumnType("money");
            // LineTotal is a SQL Server computed column — EF must not try to INSERT/UPDATE it
            entity.Property(e => e.LineTotal)
                .HasColumnType("money")
                .ValueGeneratedOnAddOrUpdate();
        });

        // ── CustomerReadModel ─────────────────────────────────────────────
        modelBuilder.Entity<CustomerReadModel>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.ToTable("CustomerReadModel");
            entity.Property(e => e.CustomerId).HasMaxLength(5).IsFixedLength();
            entity.Property(e => e.CompanyName).HasMaxLength(40);
            entity.Property(e => e.City).HasMaxLength(15);
            entity.Property(e => e.Country).HasMaxLength(15);
        });
    }
}
