import './App.css'
import { BrowserRouter, Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom'
import Layout from './components/Layout'
import DevelopmentProjectsPage from './pages/DevelopmentProjectsPage'
import DocumentsPage from './pages/documents/DocumentsPage'
import GroundLeasesPage from './pages/GroundLeasesPage'
import GroundLeaseTemplatesPage from './pages/GroundLeaseTemplatesPage'
import HomesPage from './pages/HomesPage'
import HomeSelectionPage from './pages/home-selections/HomeSelectionPage'
import ApplicationsPage from './pages/ApplicationsPage'
import HouseholdsPage from './pages/HouseholdsPage'
import WaitingListPage from './pages/WaitingListPage'
import LandParcelsPage from './pages/LandParcelsPage'
import OnboardingPage from './pages/OnboardingPage'
import PurchaseWorkflowsPage from './pages/PurchaseWorkflowsPage'
import ResaleWorkflowsPage from './pages/ResaleWorkflowsPage'
import SignupPage from './pages/SignupPage'
import StewardshipPage from './pages/stewardship/StewardshipPage'
import { AuthProvider, useAuth } from './contexts/AuthContext'
import { useCurrentTrust } from './hooks/trusts'
import { setTrustTimeZone } from './lib/calendarDate'
import { landingPath } from './lib/navigation'
import { readFocus } from './hooks/useNavPreferences'

function Loading() {
  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--text-dim)', fontSize: '12px', letterSpacing: '0.1em' }}>
        Loading…
      </span>
    </div>
  )
}

function ConnectionError({ error }: { error: string }) {
  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', flexDirection: 'column', gap: 12 }}>
      <span style={{ fontFamily: 'var(--font-mono)', color: 'var(--error-bright)', fontSize: '12px', letterSpacing: '0.1em' }}>
        Connection error
      </span>
      <span style={{ color: 'var(--text-secondary)', fontSize: '13px' }}>{error}</span>
    </div>
  )
}

function ProtectedRoute() {
  const { isLoading, isAuthenticated, user, error } = useAuth()
  const location = useLocation()
  if (isLoading) return <Loading />
  if (error) return <ConnectionError error={error} />
  if (!isAuthenticated) {
    const returnUrl = `${location.pathname}${location.search}${location.hash}`
    window.location.href = `/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`
    return null
  }

  const onOnboarding = location.pathname === '/onboarding' || location.pathname === '/signup'
  if (!user?.hasTrust && !onOnboarding) {
    return <Navigate to="/onboarding" replace />
  }
  if (user?.hasTrust && onOnboarding) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}

/** Holds the app until the trust's time zone is known, so every "today" follows it. */
function TrustRoute() {
  const { data: trust, isLoading, isError } = useCurrentTrust()
  if (isLoading) return <Loading />
  if (isError || !trust) return <ConnectionError error='Could not load your trust. Reload to try again.' />

  setTrustTimeZone(trust.timeZone)
  return <Outlet />
}

/** The root lands on the first page under the staff member's My work. */
function LandingRedirect() {
  return <Navigate to={landingPath(readFocus())} replace />
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/signup" element={<SignupPage />} />
            <Route path="/onboarding" element={<OnboardingPage />} />
            <Route element={<TrustRoute />}>
              <Route element={<Layout />}>
                <Route index element={<LandingRedirect />} />
                <Route path="/land-parcels"        element={<LandParcelsPage />} />
                <Route path="/homes"               element={<HomesPage />} />
                <Route path="/homes/selections/:selectionId" element={<HomeSelectionPage />} />
                <Route path="/development"         element={<DevelopmentProjectsPage />} />
                <Route path="/ground-leases"          element={<GroundLeasesPage />} />
                <Route path="/ground-lease-templates" element={<GroundLeaseTemplatesPage />} />
                <Route path="/applications" element={<ApplicationsPage />} />
                <Route path="/households"   element={<HouseholdsPage />} />
                <Route path="/waiting-list" element={<WaitingListPage />} />
                <Route path="/resale-workflows"    element={<ResaleWorkflowsPage />} />
                <Route path="/purchase-workflows"  element={<PurchaseWorkflowsPage />} />
                <Route path="/documents"           element={<DocumentsPage />} />
                <Route path="/stewardship"          element={<StewardshipPage />} />
              </Route>
            </Route>
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
