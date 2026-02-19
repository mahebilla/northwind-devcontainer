using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorthwindApi.Data;
using NorthwindApi.DTOs;

namespace NorthwindApi.Controllers;

/// <summary>
/// Demonstrates: Calling stored procedures via FromSqlRaw / SqlQueryRaw
/// Northwind SPs: CustOrderHist, [Ten Most Expensive Products]
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class StoredProceduresController : ControllerBase
{
    private readonly NorthwindContext _context;

    public StoredProceduresController(NorthwindContext context) => _context = context;

    // GET api/storedprocedures/custorderhistory?customerId=ALFKI
    [HttpGet("custorderhistory")]
    public async Task<IActionResult> CustOrderHistExample([FromQuery] string customerId = "ALFKI")
    {
        var param = new Microsoft.Data.SqlClient.SqlParameter("@CustomerID", customerId);
        var results = await _context.Database
            .SqlQueryRaw<CustOrderHistResult>("EXEC CustOrderHist @CustomerID", param)
            .ToListAsync();
        return Ok(new
        {
            Method = "Database.SqlQueryRaw<T>() — Stored Procedure",
            Description = "Call CustOrderHist SP which returns product order totals for a customer",
            CustomerId = customerId,
            Data = results
        });
    }

    // GET api/storedprocedures/tenmostexpensive
    [HttpGet("tenmostexpensive")]
    public async Task<IActionResult> TenMostExpensiveExample()
    {
        var results = await _context.Database
            .SqlQueryRaw<TenMostExpensiveResult>("EXEC [Ten Most Expensive Products]")
            .ToListAsync();
        return Ok(new
        {
            Method = "Database.SqlQueryRaw<T>() — SP with no parameters",
            Description = "Call [Ten Most Expensive Products] stored procedure",
            Data = results
        });
    }
}
