namespace NorthwindCqrs.Application.ReadModels;

// CustomerName is pre-joined at write time — no join needed at query time.
public class OrderReadModel
{
    public int OrderId { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerId { get; set; }
    public decimal? Freight { get; set; }
    public string? ShipCountry { get; set; }
    public int? EmployeeId { get; set; }
}
