import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'ChangeTracker.Entries()',
    endpoint: '/api/changetracker/entries',
    method: 'GET',
    description: 'Loads entities, modifies one, adds a new one, then inspects ChangeTracker.Entries() to see each tracked entity and its state.',
    code: `[HttpGet("entries")]
public async Task<IActionResult> GetEntries()
{
    var products = await _context.Products.Take(3).ToListAsync();

    // Modify the first product — state becomes Modified
    products[0].UnitPrice += 1;

    // Add a new product — state becomes Added
    _context.Products.Add(new Product { ProductName = "New Demo Product" });

    // Inspect all tracked entries
    var entries = _context.ChangeTracker.Entries()
        .Select(e => new {
            EntityType = e.Entity.GetType().Name,
            e.State,
            StateName = e.State.ToString()
        })
        .ToList();

    return Ok(new {
        method = "ChangeTracker.Entries()",
        description = "Shows every entity being tracked and its current EntityState",
        data = entries
    });
}`,
  },
  {
    name: 'All EntityState Values',
    endpoint: '/api/changetracker/entitystate',
    method: 'GET',
    description: 'Demonstrates all five EntityState values: Unchanged, Modified, Added, Deleted, and Detached.',
    code: `[HttpGet("entitystate")]
public async Task<IActionResult> GetEntityStates()
{
    // Unchanged: loaded from DB, no modifications
    var unchanged = await _context.Products.FirstAsync();

    // Modified: loaded then changed
    var modified = await _context.Products.Skip(1).FirstAsync();
    modified.UnitPrice += 1;

    // Added: brand new entity not yet in DB
    var added = new Product { ProductName = "Added Product" };
    _context.Products.Add(added);

    // Deleted: loaded then marked for deletion
    var deleted = await _context.Products.Skip(2).FirstAsync();
    _context.Products.Remove(deleted);

    // Detached: exists in memory but not tracked
    var detached = new Product { ProductName = "Detached Product" };

    var states = new[]
    {
        new { Name = unchanged.ProductName, State = _context.Entry(unchanged).State.ToString() },
        new { Name = modified.ProductName,  State = _context.Entry(modified).State.ToString() },
        new { Name = added.ProductName,     State = _context.Entry(added).State.ToString() },
        new { Name = deleted.ProductName,   State = _context.Entry(deleted).State.ToString() },
        new { Name = detached.ProductName,  State = _context.Entry(detached).State.ToString() }
    };

    return Ok(new {
        method = "EntityState (all 5 values)",
        description = "Unchanged, Modified, Added, Deleted, Detached",
        data = states
    });
}`,
  },
  {
    name: 'OriginalValues / CurrentValues',
    endpoint: '/api/changetracker/original-current',
    method: 'GET',
    description: 'Shows how to read OriginalValues vs CurrentValues and check which properties are marked as modified.',
    code: `[HttpGet("original-current")]
public async Task<IActionResult> GetOriginalCurrent()
{
    var product = await _context.Products.FirstAsync();
    var originalPrice = product.UnitPrice;

    // Modify the price
    product.UnitPrice += 10;

    var entry = _context.Entry(product);

    // Compare original vs current for each property
    var propertyStates = entry.Properties.Select(p => new {
        Property = p.Metadata.Name,
        OriginalValue = p.OriginalValue?.ToString(),
        CurrentValue = p.CurrentValue?.ToString(),
        p.IsModified
    }).ToList();

    return Ok(new {
        method = "OriginalValues / CurrentValues",
        description = $"UnitPrice changed from {originalPrice} to {product.UnitPrice}",
        data = propertyStates
    });
}`,
  },
  {
    name: 'DetectChanges / HasChanges',
    endpoint: '/api/changetracker/detect-changes',
    method: 'GET',
    description: 'Calls ChangeTracker.DetectChanges() explicitly and checks HasChanges() before and after modifications.',
    code: `[HttpGet("detect-changes")]
public async Task<IActionResult> GetDetectChanges()
{
    var product = await _context.Products.FirstAsync();
    var hasChangesBefore = _context.ChangeTracker.HasChanges();

    // Modify via the backing field (bypasses property setter notification)
    product.UnitPrice += 1;

    // DetectChanges scans all tracked entities for modifications
    _context.ChangeTracker.DetectChanges();
    var hasChangesAfter = _context.ChangeTracker.HasChanges();

    var trackedEntities = _context.ChangeTracker.Entries()
        .Select(e => new {
            Entity = e.Entity.GetType().Name,
            State = e.State.ToString()
        })
        .ToList();

    return Ok(new {
        method = "DetectChanges() / HasChanges()",
        description = "DetectChanges scans tracked entities; HasChanges reports if any are modified",
        data = new {
            hasChangesBefore,
            hasChangesAfter,
            trackedEntities
        }
    });
}`,
  },
]

const ChangeTracker = () => (
  <DemoSection
    title="Change Tracker"
    subtitle="Demonstrates ChangeTracker.Entries, all five EntityState values, OriginalValues/CurrentValues, and DetectChanges."
    demos={demos}
  />
)

export default ChangeTracker
