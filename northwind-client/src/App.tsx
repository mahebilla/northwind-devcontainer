import { Routes, Route, Navigate } from 'react-router-dom'
import MainLayout from './layout/MainLayout'
import BasicQueries from './pages/BasicQueries'
import RawSql from './pages/RawSql'
import RelatedData from './pages/RelatedData'
import Tracking from './pages/Tracking'
import CrudOperations from './pages/CrudOperations'
import BulkOperations from './pages/BulkOperations'
import Transactions from './pages/Transactions'
import CompiledQueries from './pages/CompiledQueries'
import GlobalFilters from './pages/GlobalFilters'
import ChangeTracker from './pages/ChangeTracker'
import StoredProcedures from './pages/StoredProcedures'
import Pagination from './pages/Pagination'

function App() {
  return (
    <MainLayout>
      <Routes>
        <Route path="/" element={<Navigate to="/basic-queries" replace />} />
        <Route path="/basic-queries" element={<BasicQueries />} />
        <Route path="/raw-sql" element={<RawSql />} />
        <Route path="/related-data" element={<RelatedData />} />
        <Route path="/tracking" element={<Tracking />} />
        <Route path="/crud-operations" element={<CrudOperations />} />
        <Route path="/bulk-operations" element={<BulkOperations />} />
        <Route path="/transactions" element={<Transactions />} />
        <Route path="/compiled-queries" element={<CompiledQueries />} />
        <Route path="/global-filters" element={<GlobalFilters />} />
        <Route path="/change-tracker" element={<ChangeTracker />} />
        <Route path="/stored-procedures" element={<StoredProcedures />} />
        <Route path="/pagination" element={<Pagination />} />
      </Routes>
    </MainLayout>
  )
}

export default App
