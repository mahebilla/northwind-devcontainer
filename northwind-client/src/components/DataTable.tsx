import React from 'react'

interface Props {
  data: Record<string, unknown>[]
  title?: string
}

const DataTable: React.FC<Props> = ({ data, title }) => {
  if (!data || data.length === 0) return <p style={{ color: '#888', fontStyle: 'italic' }}>No data returned.</p>

  const columns = Object.keys(data[0])

  return (
    <div style={{ marginTop: '12px', overflowX: 'auto' }}>
      {title && <h4 style={{ marginBottom: '8px' }}>{title}</h4>}
      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '13px' }}>
        <thead>
          <tr>
            {columns.map(col => (
              <th key={col} style={{
                borderBottom: '2px solid #ddd',
                padding: '8px 10px',
                textAlign: 'left',
                backgroundColor: '#f0f0f0',
                fontWeight: 600,
                fontSize: '12px',
                color: '#555',
                textTransform: 'uppercase',
                letterSpacing: '0.5px',
              }}>
                {col}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((row, i) => (
            <tr key={i} style={{ backgroundColor: i % 2 === 0 ? '#fff' : '#fafafa' }}>
              {columns.map(col => (
                <td key={col} style={{ borderBottom: '1px solid #eee', padding: '7px 10px', fontSize: '13px' }}>
                  {formatValue(row[col])}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
      <div style={{ fontSize: '11px', color: '#999', marginTop: '4px' }}>
        {data.length} row{data.length !== 1 ? 's' : ''}
      </div>
    </div>
  )
}

function formatValue(val: unknown): string {
  if (val === null || val === undefined) return ''
  if (typeof val === 'boolean') return val ? 'true' : 'false'
  if (typeof val === 'object') return JSON.stringify(val)
  return String(val)
}

export default DataTable
