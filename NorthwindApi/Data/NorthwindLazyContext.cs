using Microsoft.EntityFrameworkCore;

namespace NorthwindApi.Data;

/// <summary>
/// DbContext subclass configured with lazy loading proxies.
/// Used by RelatedDataController to demonstrate lazy loading.
/// </summary>
public class NorthwindLazyContext : NorthwindContext
{
    public NorthwindLazyContext(DbContextOptions<NorthwindLazyContext> options)
        : base(options)
    {
    }
}
