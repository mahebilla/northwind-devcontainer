import React from 'react'
import Sidebar from './Sidebar'

interface Props {
  children: React.ReactNode
}

const MainLayout: React.FC<Props> = ({ children }) => {
  return (
    <div style={{ display: 'flex', minHeight: '100vh' }}>
      <Sidebar />
      <main style={{ flex: 1, padding: '24px 32px', overflowY: 'auto', backgroundColor: '#f5f5f5' }}>
        {children}
      </main>
    </div>
  )
}

export default MainLayout
