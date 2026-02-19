import { NavLink } from 'react-router-dom'

const navItems = [
  { path: '/basic-queries', label: 'Basic Queries', desc: 'Where, Find, First, Count, Aggregates, GroupBy, Join' },
  { path: '/raw-sql', label: 'Raw SQL', desc: 'FromSql, FromSqlRaw, SqlQuery, ExecuteSql' },
  { path: '/related-data', label: 'Related Data', desc: 'Include, ThenInclude, Explicit, Lazy Loading' },
  { path: '/tracking', label: 'Tracking', desc: 'AsNoTracking, IdentityResolution, Comparison' },
  { path: '/crud-operations', label: 'CRUD Operations', desc: 'Add, Update, Remove, Attach, SaveChanges' },
  { path: '/bulk-operations', label: 'Bulk Operations', desc: 'ExecuteUpdate, ExecuteDelete' },
  { path: '/transactions', label: 'Transactions', desc: 'BeginTransaction, Savepoints, Rollback' },
  { path: '/compiled-queries', label: 'Compiled Queries', desc: 'CompileQuery, CompileAsyncQuery' },
  { path: '/global-filters', label: 'Global Filters', desc: 'HasQueryFilter, IgnoreQueryFilters' },
  { path: '/change-tracker', label: 'Change Tracker', desc: 'Entries, EntityState, OriginalValues' },
  { path: '/stored-procedures', label: 'Stored Procedures', desc: 'FromSqlRaw with SPs' },
  { path: '/pagination', label: 'Pagination', desc: 'Skip/Take, Keyset pagination' },
]

const Sidebar = () => {
  return (
    <nav style={{
      width: '280px',
      minWidth: '280px',
      backgroundColor: '#1a1a2e',
      color: '#eee',
      padding: '16px',
      overflowY: 'auto',
      display: 'flex',
      flexDirection: 'column',
    }}>
      <div style={{ marginBottom: '24px' }}>
        <h2 style={{ color: '#fff', fontSize: '18px', margin: 0 }}>EF Core Methods</h2>
        <p style={{ color: '#888', fontSize: '11px', marginTop: '4px' }}>Northwind Database Demo</p>
      </div>
      {navItems.map(item => (
        <NavLink
          key={item.path}
          to={item.path}
          style={({ isActive }) => ({
            display: 'block',
            padding: '10px 12px',
            marginBottom: '2px',
            color: isActive ? '#fff' : '#aaa',
            backgroundColor: isActive ? '#16213e' : 'transparent',
            borderRadius: '6px',
            textDecoration: 'none',
            borderLeft: isActive ? '3px solid #0066cc' : '3px solid transparent',
            transition: 'all 0.15s ease',
          })}
        >
          <div style={{ fontWeight: 600, fontSize: '13px' }}>{item.label}</div>
          <div style={{ fontSize: '10px', color: '#777', marginTop: '2px' }}>{item.desc}</div>
        </NavLink>
      ))}
    </nav>
  )
}

export default Sidebar
