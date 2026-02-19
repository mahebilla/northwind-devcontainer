import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'Default Tracking',
    endpoint: '/api/tracking/tracked',
    method: 'GET',
    description: 'EF Core tracks entities by default \u2014 the change tracker monitors property changes for SaveChanges',
    code: `// GET /api/tracking/tracked
var products = await _context.Products
    .Take(10)
    .ToListAsync();

// These entities are tracked by the DbContext.
// Any property changes will be detected and persisted
// when SaveChangesAsync() is called.

var trackedCount = _context.ChangeTracker
    .Entries()
    .Count();

return Ok(new { trackedCount, products });`,
  },
  {
    name: 'AsNoTracking',
    endpoint: '/api/tracking/notracking',
    method: 'GET',
    description: 'Skip change tracking for read-only queries \u2014 faster and uses less memory',
    code: `// GET /api/tracking/notracking
var products = await _context.Products
    .AsNoTracking()
    .Take(10)
    .ToListAsync();

// AsNoTracking() skips the change tracker.
// - Faster query materialization
// - Lower memory usage
// - Cannot call SaveChanges to persist edits

var trackedCount = _context.ChangeTracker
    .Entries()
    .Count(); // 0 — nothing is tracked`,
  },
  {
    name: 'AsNoTrackingWithIdentityResolution',
    endpoint: '/api/tracking/identity-resolution',
    method: 'GET',
    description: 'No tracking but de-duplicates entities that appear multiple times in the result set',
    code: `// GET /api/tracking/identity-resolution
var orders = await _context.Orders
    .Include(o => o.Customer)
    .AsNoTrackingWithIdentityResolution()
    .Take(20)
    .ToListAsync();

// Without identity resolution, the same Customer object
// would be materialized multiple times (once per order).
// With it, EF reuses the same instance — saving memory
// while still skipping the change tracker.`,
  },
  {
    name: 'Performance Comparison',
    endpoint: '/api/tracking/comparison',
    method: 'GET',
    description: 'Compare query execution time between tracked, no-tracking, and identity resolution modes',
    code: `// GET /api/tracking/comparison
var sw = Stopwatch.StartNew();

// 1) Tracked query
var tracked = await _context.Products.ToListAsync();
var trackedMs = sw.ElapsedMilliseconds;

_context.ChangeTracker.Clear();
sw.Restart();

// 2) No-tracking query
var noTracking = await _context.Products
    .AsNoTracking()
    .ToListAsync();
var noTrackingMs = sw.ElapsedMilliseconds;

sw.Restart();

// 3) No-tracking with identity resolution
var identityRes = await _context.Products
    .AsNoTrackingWithIdentityResolution()
    .ToListAsync();
var identityResMs = sw.ElapsedMilliseconds;

return Ok(new { trackedMs, noTrackingMs, identityResMs });`,
  },
]

const Tracking = () => (
  <DemoSection
    title="Query Tracking"
    subtitle="Demonstrates default tracking, AsNoTracking, AsNoTrackingWithIdentityResolution, and performance comparison."
    demos={demos}
  />
)

export default Tracking
