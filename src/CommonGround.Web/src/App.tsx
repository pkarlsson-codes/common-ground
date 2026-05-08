import './App.css'
import { BrowserRouter, Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom'
import Layout from './components/Layout'
import DevelopmentProjectsPage from './pages/DevelopmentProjectsPage'
import DocumentsPage from './pages/documents/DocumentsPage'
import GroundLeasesPage from './pages/GroundLeasesPage'
import HomesPage from './pages/HomesPage'
import LandParcelsPage from './pages/LandParcelsPage'
import OnboardingPage from './pages/OnboardingPage'
import PurchaseWorkflowsPage from './pages/PurchaseWorkflowsPage'
import ResaleWorkflowsPage from './pages/ResaleWorkflowsPage'
import SignupPage from './pages/SignupPage'
import StewardshipPage from './pages/stewardship/StewardshipPage'
import { AuthProvider, useAuth } from './contexts/AuthContext'

function ProtectedRoute() {
  const { isLoading, isAuthenticated, user, error } = useAuth()
  const location = useLocation()
  if (isLoading) return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--text-dim)', fontSize: '12px', letterSpacing: '0.1em' }}>
        Loading…
      </span>
    </div>
  )
  if (error) return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', flexDirection: 'column', gap: 12 }}>
      <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--error-bright)', fontSize: '12px', letterSpacing: '0.1em' }}>
        Connection error
      </span>
      <span style={{ color: 'var(--text-secondary)', fontSize: '13px' }}>{error}</span>
    </div>
  )
  if (!isAuthenticated) {
    const returnUrl = `${location.pathname}${location.search}${location.hash}`
    window.location.href = `/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`
    return null
  }

  if (!user?.hasTrust && location.pathname !== '/onboarding') {
    return <Navigate to="/onboarding" replace />
  }

  return <Outlet />
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/signup" element={<SignupPage />} />
          <Route element={<ProtectedRoute />}>
            <Route path="/onboarding" element={<OnboardingPage />} />
            <Route element={<Layout />}>
              <Route index element={<Navigate to="/land-parcels" replace />} />
              <Route path="/land-parcels"        element={<LandParcelsPage />} />
              <Route path="/homes"               element={<HomesPage />} />
              <Route path="/development"         element={<DevelopmentProjectsPage />} />
              <Route path="/ground-leases"       element={<GroundLeasesPage />} />
              <Route path="/resale-workflows"    element={<ResaleWorkflowsPage />} />
              <Route path="/purchase-workflows"  element={<PurchaseWorkflowsPage />} />
              <Route path="/documents"           element={<DocumentsPage />} />
              <Route path="/stewardship"          element={<StewardshipPage />} />
            </Route>
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
