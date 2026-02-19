namespace NorthwindCqrs.Application.ReadModels;

// ProductName is pre-joined. LineTotal is a computed column in SQL Server.
public class OrderLineReadModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? ProductName { get; set; }
    public short? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? LineTotal { get; set; }  // computed by DB: UnitPrice * Quantity
}
