using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: EF.CompileQuery, EF.CompileAsyncQuery
/// Compiled queries are pre-compiled LINQ → SQL translations for better performance.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CompiledQueriesController : ControllerBase
{
    private readonly NorthwindContext _context;

    // Compiled queries — defined as static fields (thread-safe, reusable across requests)
    private static readonly Func<NorthwindContext, decimal, IEnumerable<Product>> _getProductsByPrice =
        EF.CompileQuery((NorthwindContext ctx, decimal minPrice) =>
            ctx.Products.IgnoreQueryFilters().Where(p => p.UnitPrice > minPrice));

    private static readonly Func<NorthwindContext, int, Task<Product?>> _getProductByIdAsync =
        EF.CompileAsyncQuery((NorthwindContext ctx, int id) =>
            ctx.Products.IgnoreQueryFilters().FirstOrDefault(p => p.ProductId == id));

    private static readonly Func<NorthwindContext, IEnumerable<Product>> _getAllProducts =
        EF.CompileQuery((NorthwindContext ctx) =>
            ctx.Products.IgnoreQueryFilters());

    private static readonly Func<NorthwindContext, string, IAsyncEnumerable<Product>> _searchProductsAsync =
        EF.CompileAsyncQuery((NorthwindContext ctx, string search) =>
            ctx.Products.IgnoreQueryFilters().Where(p => p.ProductName.Contains(search)));

    public CompiledQueriesController(NorthwindContext context) => _context = context;

    // GET api/compiledqueries/byprice?minPrice=20
    [HttpGet("byprice")]
    public IActionResult GetByPrice([FromQuery] decimal minPrice = 20)
    {
        var products = _getProductsByPrice(_context, minPrice).ToList();
        return Ok(new
        {
            Method = "EF.CompileQuery — GetProductsByPrice",
            Description = "Pre-compiled LINQ query with parameter — skips query translation on each call (~15% faster)",
            Data = products.Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
        });
    }

    // GET api/compiledqueries/byid/1
    [HttpGet("byid/{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _getProductByIdAsync(_context, id);
        return Ok(new
        {
            Method = "EF.CompileAsyncQuery — GetProductById",
            Description = "Async compiled query returning a single entity",
            Data = product == null ? null : new { product.ProductId, product.ProductName, product.UnitPrice }
        });
    }

    // GET api/compiledqueries/all
    [HttpGet("all")]
    public IActionResult GetAll()
    {
        var products = _getAllProducts(_context).ToList();
        return Ok(new
        {
            Method = "EF.CompileQuery — GetAllProducts",
            Description = "Compiled query with no parameters",
            Count = products.Count,
            Data = products.Select(p => new { p.ProductId, p.ProductName, p.UnitPrice })
        });
    }

    // GET api/compiledqueries/search?q=Ch
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q = "Ch")
    {
        var results = new List<object>();
        await foreach (var product in _searchProductsAsync(_context, q))
        {
            results.Add(new { product.ProductId, product.ProductName, product.UnitPrice });
        }
        return Ok(new
        {
            Method = "EF.CompileAsyncQuery — IAsyncEnumerable",
            Description = "Compiled async query returning IAsyncEnumerable for streaming results",
            SearchTerm = q,
            Data = results
        });
    }
}
