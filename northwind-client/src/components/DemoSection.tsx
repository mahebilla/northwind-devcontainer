import { useState } from 'react'
import axios from 'axios'
import DataTable from './DataTable'
import CodeSnippet from './CodeSnippet'

export interface EndpointDemo {
  name: string
  endpoint: string
  method: 'GET' | 'POST' | 'PUT' | 'DELETE'
  code: string
  description: string
  body?: unknown
}

interface Props {
  title: string
  subtitle: string
  demos: EndpointDemo[]
}

const DemoSection: React.FC<Props> = ({ title, subtitle, demos }) => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const [results, setResults] = useState<Record<string, any>>({})
  const [loading, setLoading] = useState<Record<string, boolean>>({})

  const runDemo = async (demo: EndpointDemo) => {
    setLoading(prev => ({ ...prev, [demo.name]: true }))
    try {
      const response = await axios({
        method: demo.method,
        url: demo.endpoint,
        data: demo.body,
        headers: demo.body ? { 'Content-Type': 'application/json' } : undefined,
      })
      setResults(prev => ({ ...prev, [demo.name]: response.data }))
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Unknown error'
      setResults(prev => ({ ...prev, [demo.name]: { error: msg } }))
    }
    setLoading(prev => ({ ...prev, [demo.name]: false }))
  }

  return (
    <div>
      <h1>{title}</h1>
      <p style={{ color: '#666', marginBottom: '20px' }}>{subtitle}</p>

      {demos.map(demo => (
        <div key={demo.name} style={{
          marginBottom: '20px',
          border: '1px solid #e0e0e0',
          borderRadius: '8px',
          padding: '16px',
          backgroundColor: '#fff',
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
            <div>
              <h3>{demo.name}</h3>
              <p style={{ color: '#666', fontSize: '13px', marginTop: '2px' }}>{demo.description}</p>
            </div>
            <span style={{
              padding: '2px 8px',
              borderRadius: '4px',
              fontSize: '11px',
              fontWeight: 700,
              color: '#fff',
              backgroundColor: demo.method === 'GET' ? '#28a745' : demo.method === 'POST' ? '#007bff' : demo.method === 'PUT' ? '#ffc107' : '#dc3545',
            }}>
              {demo.method}
            </span>
          </div>

          <CodeSnippet code={demo.code} title="C# Backend Code" />

          <button
            onClick={() => runDemo(demo)}
            disabled={loading[demo.name]}
            style={{
              marginTop: '12px',
              padding: '8px 20px',
              backgroundColor: loading[demo.name] ? '#ccc' : '#0066cc',
              color: '#fff',
              border: 'none',
              borderRadius: '4px',
              fontSize: '13px',
              fontWeight: 600,
            }}
          >
            {loading[demo.name] ? 'Loading...' : `Run ${demo.method} Request`}
          </button>

          {results[demo.name] && (
            <div style={{ marginTop: '12px' }}>
              {renderResult(results[demo.name])}
            </div>
          )}
        </div>
      ))}
    </div>
  )
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function renderResult(result: any) {
  if (!result || typeof result !== 'object') return null
  const data = result as Record<string, any>

  if (data.error) {
    return <div style={{ color: '#dc3545', padding: '8px', backgroundColor: '#f8d7da', borderRadius: '4px' }}>
      Error: {String(data.error)}
    </div>
  }

  const method = data.method || data.Method
  const description = data.description || data.Description
  const resultData = data.data || data.Data

  return (
    <div style={{ backgroundColor: '#f8f9fa', padding: '12px', borderRadius: '6px', border: '1px solid #e9ecef' }}>
      {method && <div style={{ fontWeight: 600, color: '#16213e', marginBottom: '4px' }}>Method: {String(method as string)}</div>}
      {description && <div style={{ fontSize: '12px', color: '#666', marginBottom: '8px' }}>{String(description as string)}</div>}

      {Array.isArray(resultData) ? (
        <DataTable data={resultData} />
      ) : (
        <pre style={{
          backgroundColor: '#fff',
          padding: '10px',
          borderRadius: '4px',
          border: '1px solid #e0e0e0',
          fontSize: '12px',
          overflowX: 'auto',
        }}>
          {JSON.stringify(data, null, 2)}
        </pre>
      )}
    </div>
  )
}

export default DemoSection
