using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: FromSql, FromSqlRaw, FromSqlInterpolated, SqlQuery&lt;T&gt;,
/// ExecuteSqlRaw, ExecuteSqlAsync, FromSql + LINQ composition
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class RawSqlController : ControllerBase
{
    private readonly NorthwindContext _context;

    public RawSqlController(NorthwindContext context) => _context = context;

    // GET api/rawsql/fromsql
    [HttpGet("fromsql")]
    public async Task<IActionResult> FromSqlExample()
    {
        var minPrice = 20m;
        var products = await _context.Products
            .FromSql($"SELECT * FROM Products WHERE UnitPrice > {minPrice}")
            .IgnoreQueryFilters()
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .ToListAsync();
        return Ok(new { Method = "FromSql()", Description = "Interpolated SQL with auto-parameterization (safe from SQL injection)", Data = products });
    }

    // GET api/rawsql/fromsqlraw
    [HttpGet("fromsqlraw")]
    public async Task<IActionResult> FromSqlRawExample()
    {
        var param = new Microsoft.Data.SqlClient.SqlParameter("@price", 30m);
        var products = await _context.Products
            .FromSqlRaw("SELECT * FROM Products WHERE UnitPrice > @price", param)
            .IgnoreQueryFilters()
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .ToListAsync();
        return Ok(new { Method = "FromSqlRaw()", Description = "Raw SQL with explicit SqlParameter — manual parameterization", Data = products });
    }

    // GET api/rawsql/fromsqlinterpolated
    [HttpGet("fromsqlinterpolated")]
    public async Task<IActionResult> FromSqlInterpolatedExample()
    {
        var categoryId = 1;
        var products = await _context.Products
            .FromSqlInterpolated($"SELECT * FROM Products WHERE CategoryID = {categoryId}")
            .IgnoreQueryFilters()
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice, p.CategoryId })
            .ToListAsync();
        return Ok(new { Method = "FromSqlInterpolated()", Description = "Interpolated SQL (predecessor to FromSql in EF Core 3-6)", Data = products });
    }

    // GET api/rawsql/sqlquery
    [HttpGet("sqlquery")]
    public async Task<IActionResult> SqlQueryExample()
    {
        var productNames = await _context.Database
            .SqlQuery<string>($"SELECT ProductName AS [Value] FROM Products WHERE CategoryID = 1")
            .ToListAsync();
        return Ok(new { Method = "Database.SqlQuery<string>()", Description = "EF Core 8 — query scalar/non-entity types directly", Data = productNames });
    }

    // POST api/rawsql/executesqlraw
    [HttpPost("executesqlraw")]
    public async Task<IActionResult> ExecuteSqlRawExample()
    {
        // Safe demo: update a column to its existing value (no-op)
        var rowsAffected = await _context.Database
            .ExecuteSqlRawAsync("UPDATE Products SET Discontinued = Discontinued WHERE ProductID = @p0", 1);
        return Ok(new { Method = "Database.ExecuteSqlRawAsync()", Description = "Execute non-query SQL (UPDATE/DELETE/INSERT)", RowsAffected = rowsAffected });
    }

    // POST api/rawsql/executesql
    [HttpPost("executesql")]
    public async Task<IActionResult> ExecuteSqlExample()
    {
        var productId = 1;
        var rowsAffected = await _context.Database
            .ExecuteSqlAsync($"UPDATE Products SET Discontinued = Discontinued WHERE ProductID = {productId}");
        return Ok(new { Method = "Database.ExecuteSqlAsync()", Description = "Interpolated non-query SQL with auto-parameterization", RowsAffected = rowsAffected });
    }

    // GET api/rawsql/fromsql-with-linq
    [HttpGet("fromsql-with-linq")]
    public async Task<IActionResult> FromSqlWithLinqExample()
    {
        var products = await _context.Products
            .FromSql($"SELECT * FROM Products")
            .IgnoreQueryFilters()
            .Where(p => p.UnitPrice > 20)
            .OrderBy(p => p.ProductName)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .ToListAsync();
        return Ok(new { Method = "FromSql() + LINQ composition", Description = "Start with raw SQL, then compose additional LINQ operators on top", Data = products });
    }
}
