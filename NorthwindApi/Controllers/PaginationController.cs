using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: Skip/Take offset pagination, keyset (cursor) pagination,
/// pagination with Include
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PaginationController : ControllerBase
{
    private readonly NorthwindContext _context;

    public PaginationController(NorthwindContext context) => _context = context;

    // GET api/pagination/offset?page=1&pageSize=10
    [HttpGet("offset")]
    public async Task<IActionResult> OffsetPaginationExample([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalCount = await _context.Products.IgnoreQueryFilters().CountAsync();
        var products = await _context.Products
            .IgnoreQueryFilters()
            .OrderBy(p => p.ProductId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice, p.CategoryId })
            .ToListAsync();

        return Ok(new
        {
            Method = "Skip/Take Offset Pagination",
            Description = "Traditional pagination using OFFSET/FETCH — simple but slower for large offsets",
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = products
        });
    }

    // GET api/pagination/keyset?lastId=0&pageSize=10
    [HttpGet("keyset")]
    public async Task<IActionResult> KeysetPaginationExample([FromQuery] int lastId = 0, [FromQuery] int pageSize = 10)
    {
        var products = await _context.Products
            .IgnoreQueryFilters()
            .OrderBy(p => p.ProductId)
            .Where(p => p.ProductId > lastId)
            .Take(pageSize)
            .Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
            .ToListAsync();

        var nextCursor = products.LastOrDefault()?.ProductId ?? lastId;

        return Ok(new
        {
            Method = "Keyset (Cursor-Based) Pagination",
            Description = "Uses WHERE ProductId > @lastId instead of OFFSET — constant performance regardless of page depth",
            LastId = lastId,
            PageSize = pageSize,
            NextCursor = nextCursor,
            HasMore = products.Count == pageSize,
            Data = products
        });
    }

    // GET api/pagination/orders?page=1&pageSize=10
    [HttpGet("orders")]
    public async Task<IActionResult> OrdersPaginationExample([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalCount = await _context.Orders.CountAsync();
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.OrderId,
                o.OrderDate,
                CustomerName = o.Customer!.CompanyName,
                o.Freight,
                o.ShipCountry
            })
            .ToListAsync();

        return Ok(new
        {
            Method = "Pagination with Include",
            Description = "Real-world pagination combining Skip/Take with eager loading via Include",
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Data = orders
        });
    }
}
