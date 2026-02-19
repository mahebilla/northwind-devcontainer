using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: Where, Find, FirstOrDefault, SingleOrDefault, Any, Count,
/// Sum/Avg/Min/Max, OrderBy/ThenBy, GroupBy, Select, Distinct, Contains, All,
/// Join, TagWith, LINQ query syntax
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BasicQueriesController : ControllerBase
{
    private readonly NorthwindContext _context;

    public BasicQueriesController(NorthwindContext context) => _context = context;

    // GET api/basicqueries/where
    [HttpGet("where")]
    public async Task<IActionResult> WhereExample()
    {
        var products = await _context.Products
            .IgnoreQueryFilters()
            .Where(p => p.UnitPrice > 20)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .ToListAsync();
        return Ok(new { Method = "Where()", Description = "Filter products with UnitPrice > 20", Data = products });
    }

    // GET api/basicqueries/find/1
    [HttpGet("find/{id}")]
    public async Task<IActionResult> FindExample(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return Ok(new
        {
            Method = "FindAsync()",
            Description = "Look up product by primary key — checks change tracker first, then DB",
            Data = product == null ? null : new { product.ProductId, product.ProductName, product.UnitPrice }
        });
    }

    // GET api/basicqueries/firstordefault
    [HttpGet("firstordefault")]
    public async Task<IActionResult> FirstOrDefaultExample()
    {
        var product = await _context.Products
            .IgnoreQueryFilters()
            .Where(p => p.CategoryId == 1)
            .FirstOrDefaultAsync();
        return Ok(new
        {
            Method = "FirstOrDefaultAsync()",
            Description = "Get the first product in category 1 (Beverages)",
            Data = product == null ? null : new { product.ProductId, product.ProductName, product.UnitPrice, product.CategoryId }
        });
    }

    // GET api/basicqueries/singleordefault/1
    [HttpGet("singleordefault/{id}")]
    public async Task<IActionResult> SingleOrDefaultExample(int id)
    {
        var product = await _context.Products
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(p => p.ProductId == id);
        return Ok(new
        {
            Method = "SingleOrDefaultAsync()",
            Description = "Expects exactly one result — throws if more than one match",
            Data = product == null ? null : new { product.ProductId, product.ProductName, product.UnitPrice }
        });
    }

    // GET api/basicqueries/any
    [HttpGet("any")]
    public async Task<IActionResult> AnyExample()
    {
        var hasExpensive = await _context.Products.IgnoreQueryFilters().AnyAsync(p => p.UnitPrice > 100);
        return Ok(new { Method = "AnyAsync()", Description = "Check if any product costs > $100", Data = hasExpensive });
    }

    // GET api/basicqueries/count
    [HttpGet("count")]
    public async Task<IActionResult> CountExample()
    {
        var total = await _context.Products.IgnoreQueryFilters().CountAsync();
        var discontinued = await _context.Products.IgnoreQueryFilters().CountAsync(p => p.Discontinued);
        return Ok(new { Method = "CountAsync()", Description = "Count all products and discontinued products", TotalProducts = total, DiscontinuedProducts = discontinued });
    }

    // GET api/basicqueries/aggregates
    [HttpGet("aggregates")]
    public async Task<IActionResult> AggregatesExample()
    {
        var avg = await _context.Products.IgnoreQueryFilters().AverageAsync(p => p.UnitPrice);
        var max = await _context.Products.IgnoreQueryFilters().MaxAsync(p => p.UnitPrice);
        var min = await _context.Products.IgnoreQueryFilters().Where(p => p.UnitPrice > 0).MinAsync(p => p.UnitPrice);
        var totalStock = await _context.Products.IgnoreQueryFilters().SumAsync(p => (int)(p.UnitsInStock ?? 0));
        return Ok(new
        {
            Method = "AverageAsync / MaxAsync / MinAsync / SumAsync",
            Description = "Aggregate functions on Products table",
            AveragePrice = avg,
            MaxPrice = max,
            MinPrice = min,
            TotalUnitsInStock = totalStock
        });
    }

    // GET api/basicqueries/orderby
    [HttpGet("orderby")]
    public async Task<IActionResult> OrderByExample()
    {
        var products = await _context.Products
            .IgnoreQueryFilters()
            .OrderByDescending(p => p.UnitPrice)
            .ThenBy(p => p.ProductName)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .Take(10)
            .ToListAsync();
        return Ok(new { Method = "OrderByDescending().ThenBy().Take()", Description = "Top 10 most expensive products", Data = products });
    }

    // GET api/basicqueries/groupby
    [HttpGet("groupby")]
    public async Task<IActionResult> GroupByExample()
    {
        var grouped = await _context.Products
            .IgnoreQueryFilters()
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, ProductCount = g.Count(), AvgPrice = g.Average(p => p.UnitPrice) })
            .ToListAsync();
        return Ok(new { Method = "GroupBy()", Description = "Group products by category with counts and average prices", Data = grouped });
    }

    // GET api/basicqueries/select
    [HttpGet("select")]
    public async Task<IActionResult> SelectExample()
    {
        var projected = await _context.Products
            .IgnoreQueryFilters()
            .Select(p => new { p.ProductName, p.UnitPrice, IsExpensive = p.UnitPrice > 50, StockStatus = p.UnitsInStock > 0 ? "In Stock" : "Out of Stock" })
            .Take(15)
            .ToListAsync();
        return Ok(new { Method = "Select() Projection", Description = "Project products into anonymous types with computed properties", Data = projected });
    }

    // GET api/basicqueries/distinct
    [HttpGet("distinct")]
    public async Task<IActionResult> DistinctExample()
    {
        var cities = await _context.Customers
            .Select(c => c.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        return Ok(new { Method = "Distinct()", Description = "Get distinct customer cities", Count = cities.Count, Data = cities });
    }

    // GET api/basicqueries/contains
    [HttpGet("contains")]
    public async Task<IActionResult> ContainsExample()
    {
        var categoryIds = new List<int> { 1, 2, 3 };
        var products = await _context.Products
            .IgnoreQueryFilters()
            .Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value))
            .Select(p => new { p.ProductId, p.ProductName, p.CategoryId })
            .ToListAsync();
        return Ok(new { Method = "Contains() → SQL IN clause", Description = "Products in categories 1, 2, or 3", Data = products });
    }

    // GET api/basicqueries/all
    [HttpGet("all")]
    public async Task<IActionResult> AllExample()
    {
        var allInStock = await _context.Products.IgnoreQueryFilters().AllAsync(p => p.UnitsInStock > 0);
        return Ok(new { Method = "AllAsync()", Description = "Check if ALL products are in stock", AllProductsInStock = allInStock });
    }

    // GET api/basicqueries/querysyntax
    [HttpGet("querysyntax")]
    public async Task<IActionResult> QuerySyntaxExample()
    {
        var products = await (
            from p in _context.Products.IgnoreQueryFilters()
            where p.UnitPrice > 20
            orderby p.ProductName
            select new { p.ProductId, p.ProductName, p.UnitPrice }
        ).ToListAsync();
        return Ok(new { Method = "LINQ Query Syntax", Description = "Same as method syntax but using from/where/select keywords", Data = products });
    }

    // GET api/basicqueries/join
    [HttpGet("join")]
    public async Task<IActionResult> JoinExample()
    {
        var result = await _context.Products.IgnoreQueryFilters()
            .Join(_context.Categories,
                p => p.CategoryId,
                c => c.CategoryId,
                (p, c) => new { p.ProductName, c.CategoryName, p.UnitPrice })
            .OrderBy(x => x.CategoryName)
            .ThenBy(x => x.ProductName)
            .ToListAsync();
        return Ok(new { Method = "Join()", Description = "Explicit LINQ join between Products and Categories", Data = result });
    }

    // GET api/basicqueries/tagwith
    [HttpGet("tagwith")]
    public async Task<IActionResult> TagWithExample()
    {
        var products = await _context.Products
            .IgnoreQueryFilters()
            .TagWith("BasicQueriesController: List all beverages (CategoryId=1)")
            .Where(p => p.CategoryId == 1)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .ToListAsync();
        return Ok(new { Method = "TagWith()", Description = "Adds a SQL comment for diagnostics — visible in SQL Server Profiler", Data = products });
    }
}
