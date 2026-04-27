import './App.css'
import { BrowserRouter, Navigate, Outlet, Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import BuildingsPage from './pages/BuildingsPage'
import DocumentsPage from './pages/documents/DocumentsPage'
import GroundLeasesPage from './pages/GroundLeasesPage'
import LandParcelsPage from './pages/LandParcelsPage'
import LoginPage from './pages/LoginPage'
import PurchaseWorkflowsPage from './pages/PurchaseWorkflowsPage'
import { AuthProvider, useAuth } from './contexts/AuthContext'

function ProtectedRoute() {
  const { isLoading, isAuthenticated } = useAuth()
  if (isLoading) return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--text-dim)', fontSize: '12px', letterSpacing: '0.1em' }}>
        Loading…
      </span>
    </div>
  )
  if (!isAuthenticated) return <Navigate to="/login" replace />
  return <Outlet />
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<ProtectedRoute />}>
            <Route element={<Layout />}>
              <Route index element={<Navigate to="/land-parcels" replace />} />
              <Route path="/land-parcels"        element={<LandParcelsPage />} />
              <Route path="/buildings"           element={<BuildingsPage />} />
              <Route path="/ground-leases"       element={<GroundLeasesPage />} />
              <Route path="/purchase-workflows"  element={<PurchaseWorkflowsPage />} />
              <Route path="/documents"           element={<DocumentsPage />} />
            </Route>
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
