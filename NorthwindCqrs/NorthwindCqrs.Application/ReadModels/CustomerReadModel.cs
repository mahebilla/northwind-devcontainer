namespace NorthwindCqrs.Application.ReadModels;

public class CustomerReadModel
{
    public string CustomerId { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}
