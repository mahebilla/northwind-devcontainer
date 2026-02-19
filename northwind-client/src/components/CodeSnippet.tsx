import React from 'react'

interface Props {
  code: string
  title?: string
}

const CodeSnippet: React.FC<Props> = ({ code, title }) => {
  return (
    <div style={{
      marginTop: '10px',
      backgroundColor: '#1e1e1e',
      borderRadius: '6px',
      overflow: 'hidden',
    }}>
      {title && (
        <div style={{
          padding: '6px 14px',
          backgroundColor: '#2d2d2d',
          color: '#aaa',
          fontSize: '11px',
          fontWeight: 600,
        }}>
          {title}
        </div>
      )}
      <pre style={{
        padding: '14px',
        margin: 0,
        color: '#d4d4d4',
        fontSize: '12px',
        lineHeight: '1.5',
        overflowX: 'auto',
        fontFamily: 'Consolas, Monaco, "Courier New", monospace',
      }}>
        <code>{code}</code>
      </pre>
    </div>
  )
}

export default CodeSnippet
