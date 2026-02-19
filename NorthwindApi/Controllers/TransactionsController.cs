using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;
using NorthwindApi.Models;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: BeginTransaction, Commit, Rollback, CreateSavepoint,
/// RollbackToSavepoint, multiple SaveChanges in one transaction
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TransactionsController : ControllerBase
{
    private readonly NorthwindContext _context;

    public TransactionsController(NorthwindContext context) => _context = context;

    // POST api/transactions/basic
    [HttpPost("basic")]
    public async Task<IActionResult> BasicTransactionExample()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var product = await _context.Products.IgnoreQueryFilters().FirstAsync();
            var originalPrice = product.UnitPrice;
            product.UnitPrice = (originalPrice ?? 0) + 1;
            await _context.SaveChangesAsync();

            // Commit the transaction
            await transaction.CommitAsync();

            return Ok(new
            {
                Method = "BeginTransaction + CommitAsync",
                Description = "Explicit transaction — all changes within are atomic",
                Data = new { product.ProductId, product.ProductName, OriginalPrice = originalPrice, NewPrice = product.UnitPrice }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { Error = ex.Message, Note = "Transaction rolled back" });
        }
    }

    // POST api/transactions/rollback
    [HttpPost("rollback")]
    public async Task<IActionResult> RollbackExample()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        var product = await _context.Products.IgnoreQueryFilters().FirstAsync();
        var originalPrice = product.UnitPrice;
        product.UnitPrice = 9999.99m;
        await _context.SaveChangesAsync();

        // Intentionally rollback
        await transaction.RollbackAsync();

        // Verify rollback: re-read from DB
        _context.ChangeTracker.Clear();
        var reloaded = await _context.Products.IgnoreQueryFilters()
            .FirstAsync(p => p.ProductId == product.ProductId);

        return Ok(new
        {
            Method = "BeginTransaction + RollbackAsync",
            Description = "Changes are saved to DB within transaction, then rolled back — DB returns to original state",
            AttemptedPrice = 9999.99m,
            ActualPriceAfterRollback = reloaded.UnitPrice,
            Note = "Price unchanged because transaction was rolled back"
        });
    }

    // POST api/transactions/savepoint
    [HttpPost("savepoint")]
    public async Task<IActionResult> SavepointExample()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        // Step 1: Add product
        var product1 = new Product { ProductName = "Savepoint Demo 1", UnitPrice = 10m, Discontinued = false };
        _context.Products.Add(product1);
        await _context.SaveChangesAsync();

        await transaction.CreateSavepointAsync("AfterFirstInsert");

        // Step 2: Add another product
        var product2 = new Product { ProductName = "Savepoint Demo 2", UnitPrice = 20m, Discontinued = false };
        _context.Products.Add(product2);
        await _context.SaveChangesAsync();

        // Rollback only the second insert
        await transaction.RollbackToSavepointAsync("AfterFirstInsert");

        // Commit — only the first product is persisted
        await transaction.CommitAsync();

        return Ok(new
        {
            Method = "CreateSavepointAsync / RollbackToSavepointAsync",
            Description = "Savepoints allow partial rollback within a transaction",
            Product1Id = product1.ProductId,
            Product1Persisted = true,
            Product2Persisted = false,
            Note = "Product 2 was rolled back to savepoint, only Product 1 was committed"
        });
    }

    // POST api/transactions/multiple-savechanges
    [HttpPost("multiple-savechanges")]
    public async Task<IActionResult> MultipleSaveChangesExample()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        // First operation
        var p = await _context.Products.IgnoreQueryFilters().FirstAsync();
        p.UnitPrice = (p.UnitPrice ?? 0) + 0.01m;
        var save1 = await _context.SaveChangesAsync();

        // Second operation in same transaction
        var c = await _context.Categories.FirstAsync();
        var origDesc = c.Description;
        c.Description = (origDesc ?? "") + " [updated]";
        var save2 = await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(new
        {
            Method = "Multiple SaveChangesAsync() in one Transaction",
            Description = "Multiple save operations grouped into one atomic unit — either all commit or all rollback",
            FirstSave = save1,
            SecondSave = save2,
            Note = "Both operations committed atomically"
        });
    }
}
