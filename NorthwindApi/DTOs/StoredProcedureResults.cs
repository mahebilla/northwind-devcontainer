namespace NorthwindApi.DTOs;

/// <summary>Result type for CustOrderHist stored procedure</summary>
public class CustOrderHistResult
{
    public string ProductName { get; set; } = null!;
    public int Total { get; set; }
}

/// <summary>Result type for [Ten Most Expensive Products] stored procedure</summary>
public class TenMostExpensiveResult
{
    public string TenMostExpensiveProducts { get; set; } = null!;
    public decimal UnitPrice { get; set; }
}
