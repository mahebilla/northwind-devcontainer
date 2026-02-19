namespace NorthwindCqrs.Application.ReadModels;

// Denormalized read model stored in NorthwindRead database.
// CategoryName is pre-joined at write time so read queries need zero joins.
// CategoryId is kept for GroupBy and Contains query demos.
public class ProductReadModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public bool Discontinued { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? SupplierId { get; set; }
    public short? UnitsInStock { get; set; }
}
