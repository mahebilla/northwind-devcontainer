using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: Include, ThenInclude, Explicit loading, Lazy loading,
/// AsSplitQuery, Multiple Includes, Filtered Include
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class RelatedDataController : ControllerBase
{
    private readonly NorthwindContext _context;
    private readonly NorthwindLazyContext _lazyContext;

    public RelatedDataController(NorthwindContext context, NorthwindLazyContext lazyContext)
    {
        _context = context;
        _lazyContext = lazyContext;
    }

    // GET api/relateddata/eager-include
    [HttpGet("eager-include")]
    public async Task<IActionResult> EagerIncludeExample()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Take(10)
            .Select(o => new { o.OrderId, o.OrderDate, CustomerName = o.Customer!.CompanyName })
            .ToListAsync();
        return Ok(new { Method = "Include()", Description = "Eager load Customer for each Order in a single SQL query", Data = orders });
    }

    // GET api/relateddata/eager-theninclude
    [HttpGet("eager-theninclude")]
    public async Task<IActionResult> EagerThenIncludeExample()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .Take(5)
            .Select(o => new
            {
                o.OrderId,
                o.OrderDate,
                Items = o.OrderDetails.Select(od => new
                {
                    od.Product.ProductName,
                    od.Quantity,
                    od.UnitPrice,
                    LineTotal = od.UnitPrice * od.Quantity
                })
            })
            .ToListAsync();
        return Ok(new { Method = "Include().ThenInclude()", Description = "Multi-level eager loading: Order → OrderDetails → Product", Data = orders });
    }

    // GET api/relateddata/explicit-loading
    [HttpGet("explicit-loading")]
    public async Task<IActionResult> ExplicitLoadingExample()
    {
        var order = await _context.Orders.FirstAsync();

        // Explicitly load the collection navigation
        await _context.Entry(order).Collection(o => o.OrderDetails).LoadAsync();
        // Explicitly load the reference navigation
        await _context.Entry(order).Reference(o => o.Customer).LoadAsync();

        return Ok(new
        {
            Method = "Entry().Collection().LoadAsync() / Entry().Reference().LoadAsync()",
            Description = "Explicitly load related data after the main entity is already loaded",
            Data = new
            {
                order.OrderId,
                order.OrderDate,
                CustomerName = order.Customer?.CompanyName,
                ItemCount = order.OrderDetails.Count
            }
        });
    }

    // GET api/relateddata/lazy-loading
    [HttpGet("lazy-loading")]
    public async Task<IActionResult> LazyLoadingExample()
    {
        var product = await _lazyContext.Products.IgnoreQueryFilters().FirstAsync();
        // Accessing Category triggers lazy loading (separate SQL query fired automatically)
        var categoryName = product.Category?.CategoryName;

        return Ok(new
        {
            Method = "Lazy Loading (UseLazyLoadingProxies)",
            Description = "Accessing navigation property auto-triggers a DB query. Requires virtual nav props + proxy config.",
            Data = new { product.ProductId, product.ProductName, CategoryName = categoryName }
        });
    }

    // GET api/relateddata/split-query
    [HttpGet("split-query")]
    public async Task<IActionResult> SplitQueryExample()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderDetails)
            .Include(o => o.Customer)
            .AsSplitQuery()
            .Take(5)
            .Select(o => new
            {
                o.OrderId,
                CustomerName = o.Customer!.CompanyName,
                DetailCount = o.OrderDetails.Count
            })
            .ToListAsync();
        return Ok(new
        {
            Method = "AsSplitQuery()",
            Description = "Splits multi-Include query into separate SQL queries to avoid cartesian explosion",
            Data = orders
        });
    }

    // GET api/relateddata/multiple-includes
    [HttpGet("multiple-includes")]
    public async Task<IActionResult> MultipleIncludesExample()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .Include(o => o.ShipViaNavigation)
            .Take(5)
            .Select(o => new
            {
                o.OrderId,
                CustomerName = o.Customer!.CompanyName,
                EmployeeName = o.Employee!.FirstName + " " + o.Employee.LastName,
                ShipperName = o.ShipViaNavigation!.CompanyName
            })
            .ToListAsync();
        return Ok(new { Method = "Multiple Include() calls", Description = "Load Customer, Employee, and Shipper for each Order", Data = orders });
    }

    // GET api/relateddata/filtered-include
    [HttpGet("filtered-include")]
    public async Task<IActionResult> FilteredIncludeExample()
    {
        var categories = await _context.Categories
            .Include(c => c.Products.Where(p => !p.Discontinued))
            .Select(c => new
            {
                c.CategoryName,
                ActiveProductCount = c.Products.Count,
                ActiveProducts = c.Products.Select(p => new { p.ProductName, p.UnitPrice })
            })
            .ToListAsync();
        return Ok(new { Method = "Filtered Include()", Description = "EF Core 5+ — Include only non-discontinued products per category", Data = categories });
    }
}
