namespace NorthwindCqrs.Domain.Entities;

// Pure domain entity — no EF Core attributes, no navigation properties.
// Mapping to the Northwind DB schema is done entirely in WriteDbContext.OnModelCreating.
public class Product
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public int? CategoryId { get; set; }
    public string? QuantityPerUnit { get; set; }
    public decimal? UnitPrice { get; set; }
    public short? UnitsInStock { get; set; }
    public short? UnitsOnOrder { get; set; }
    public short? ReorderLevel { get; set; }
    public bool Discontinued { get; set; }
}
