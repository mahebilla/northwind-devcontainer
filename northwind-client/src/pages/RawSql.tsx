import DemoSection, { type EndpointDemo } from '../components/DemoSection'

const demos: EndpointDemo[] = [
  {
    name: 'FromSql (Auto-Parameterized)',
    endpoint: '/api/rawsql/fromsql',
    method: 'GET',
    description: 'Uses FromSql with string interpolation \u2014 EF auto-parameterizes to prevent SQL injection',
    code: `// GET /api/rawsql/fromsql
decimal minPrice = 20m;

var products = await _context.Products
    .FromSql($"SELECT * FROM Products WHERE UnitPrice > {minPrice}")
    .ToListAsync();

// EF Core translates the interpolated value into a
// parameterized query: @p0 = 20`,
  },
  {
    name: 'FromSqlRaw (Manual Params)',
    endpoint: '/api/rawsql/fromsqlraw',
    method: 'GET',
    description: 'Uses FromSqlRaw with explicit SqlParameter for full control',
    code: `// GET /api/rawsql/fromsqlraw
var priceParam = new SqlParameter("@price", 30m);

var products = await _context.Products
    .FromSqlRaw("SELECT * FROM Products WHERE UnitPrice > @price", priceParam)
    .ToListAsync();

// Always use parameters — never concatenate user input!`,
  },
  {
    name: 'FromSqlInterpolated',
    endpoint: '/api/rawsql/fromsqlinterpolated',
    method: 'GET',
    description: 'Explicitly parameterized interpolated SQL query',
    code: `// GET /api/rawsql/fromsqlinterpolated
decimal minPrice = 25m;

var products = await _context.Products
    .FromSqlInterpolated(
        $"SELECT * FROM Products WHERE UnitPrice > {minPrice}"
    )
    .OrderBy(p => p.ProductName)
    .ToListAsync();

// Same result as FromSql — both auto-parameterize.
// FromSqlInterpolated is the explicit version.`,
  },
  {
    name: 'SqlQuery<string> (EF Core 8)',
    endpoint: '/api/rawsql/sqlquery',
    method: 'GET',
    description: 'Query scalar values that do not map to an entity using Database.SqlQuery<T>()',
    code: `// GET /api/rawsql/sqlquery
var productNames = await _context.Database
    .SqlQuery<string>(
        $"SELECT ProductName AS Value FROM Products ORDER BY ProductName"
    )
    .ToListAsync();

// SqlQuery<T> returns unmapped scalar types.
// The column must be aliased as "Value".`,
  },
  {
    name: 'ExecuteSqlRawAsync',
    endpoint: '/api/rawsql/executesqlraw',
    method: 'POST',
    description: 'Execute a non-query SQL command (UPDATE, INSERT, DELETE) with raw SQL',
    code: `// POST /api/rawsql/executesqlraw
var rowsAffected = await _context.Database
    .ExecuteSqlRawAsync(
        "UPDATE Products SET Discontinued = @disc WHERE CategoryID = @catId",
        new SqlParameter("@disc", true),
        new SqlParameter("@catId", 8)
    );

return Ok(new { rowsAffected });`,
  },
  {
    name: 'ExecuteSqlAsync',
    endpoint: '/api/rawsql/executesql',
    method: 'POST',
    description: 'Execute a non-query SQL command with auto-parameterized interpolation',
    code: `// POST /api/rawsql/executesql
bool discontinued = true;
int categoryId = 8;

var rowsAffected = await _context.Database
    .ExecuteSqlAsync(
        $"UPDATE Products SET Discontinued = {discontinued} WHERE CategoryID = {categoryId}"
    );

return Ok(new { rowsAffected });`,
  },
  {
    name: 'FromSql + LINQ Composition',
    endpoint: '/api/rawsql/fromsql-with-linq',
    method: 'GET',
    description: 'Start with raw SQL and then compose additional LINQ operators on top',
    code: `// GET /api/rawsql/fromsql-with-linq
var products = await _context.Products
    .FromSql($"SELECT * FROM Products")
    .Where(p => p.UnitPrice > 15)
    .OrderByDescending(p => p.UnitPrice)
    .Take(5)
    .Select(p => new {
        p.ProductName,
        p.UnitPrice,
        p.CategoryId
    })
    .ToListAsync();

// EF Core merges the raw SQL with the LINQ as a subquery.`,
  },
]

const RawSql = () => (
  <DemoSection
    title="Raw SQL Queries"
    subtitle="Demonstrates FromSql, FromSqlRaw, FromSqlInterpolated, SqlQuery<T>, ExecuteSql, and SQL+LINQ composition."
    demos={demos}
  />
)

export default RawSql
