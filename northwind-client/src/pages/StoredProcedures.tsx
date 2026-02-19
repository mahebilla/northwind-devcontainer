import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'CustOrderHist (Customer Order History)',
    endpoint: '/api/storedprocedures/custorderhistory?customerId=ALFKI',
    method: 'GET',
    description: 'Calls the Northwind CustOrderHist stored procedure via SqlQueryRaw<T> with a parameterized @CustomerID.',
    code: `// DTO to map the stored procedure result set
public class CustOrderHistResult
{
    public string ProductName { get; set; } = "";
    public int Total { get; set; }
}

[HttpGet("custorderhistory")]
public async Task<IActionResult> GetCustOrderHistory(string customerId)
{
    var param = new SqlParameter("@CustomerID", customerId);

    var results = await _context.Database
        .SqlQueryRaw<CustOrderHistResult>("EXEC CustOrderHist @CustomerID", param)
        .ToListAsync();

    return Ok(new {
        method = "SqlQueryRaw<T> — EXEC CustOrderHist",
        description = $"Order history for customer '{customerId}' — {results.Count} products ordered",
        data = results
    });
}`,
  },
  {
    name: 'Ten Most Expensive Products',
    endpoint: '/api/storedprocedures/tenmostexpensive',
    method: 'GET',
    description: 'Calls the Northwind "Ten Most Expensive Products" stored procedure — no parameters needed.',
    code: `// DTO for the result set
public class TenMostExpensiveResult
{
    public string TenMostExpensiveProducts { get; set; } = "";
    public decimal UnitPrice { get; set; }
}

[HttpGet("tenmostexpensive")]
public async Task<IActionResult> GetTenMostExpensive()
{
    var results = await _context.Database
        .SqlQueryRaw<TenMostExpensiveResult>("EXEC [Ten Most Expensive Products]")
        .ToListAsync();

    return Ok(new {
        method = "SqlQueryRaw<T> — EXEC [Ten Most Expensive Products]",
        description = "Top 10 products by unit price from built-in Northwind stored procedure",
        data = results
    });
}`,
  },
]

const StoredProcedures = () => (
  <DemoSection
    title="Stored Procedures"
    subtitle="Demonstrates calling Northwind stored procedures via SqlQueryRaw<T>."
    demos={demos}
  />
)

export default StoredProcedures
