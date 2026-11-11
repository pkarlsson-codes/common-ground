import './App.css'
import { useEffect } from 'react'
import { BrowserRouter, Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom'
import Layout from './components/Layout'
import DevelopmentProjectsPage from './pages/DevelopmentProjectsPage'
import DocumentsPage from './pages/documents/DocumentsPage'
import GroundLeasesPage from './pages/GroundLeasesPage'
import GroundLeaseTemplatesPage from './pages/GroundLeaseTemplatesPage'
import HomesPage from './pages/HomesPage'
import HomeSelectionPage from './pages/home-selections/HomeSelectionPage'
import HomeSelectionsPage from './pages/HomeSelectionsPage'
import ApplicationsPage from './pages/ApplicationsPage'
import HouseholdsPage from './pages/HouseholdsPage'
import WaitingListPage from './pages/WaitingListPage'
import LandParcelsPage from './pages/LandParcelsPage'
import InvitePage from './pages/InvitePage'
import OnboardingPage from './pages/OnboardingPage'
import PurchaseWorkflowsPage from './pages/PurchaseWorkflowsPage'
import ResalesPage from './pages/ResalesPage'
import SignupPage from './pages/SignupPage'
import TodayPage from './pages/TodayPage'
import StewardshipPage from './pages/stewardship/StewardshipPage'
import LeaseFeeNoticesPrintPage from './pages/lease-fee-notices/LeaseFeeNoticesPrintPage'
import TrustPolicyPage from './pages/trust-policy/TrustPolicyPage'
import TeamPage from './pages/team/TeamPage'
import { AuthProvider, useAuth } from './contexts/AuthContext'
import { useCurrentTrust } from './hooks/trusts'
import { setTrustTimeZone } from './lib/calendarDate'

function Loading() {
  return (
    <div className="skeleton-rows" role="status" aria-label="Loading">
      <div className="skeleton-row" />
      <div className="skeleton-row" />
      <div className="skeleton-row" />
    </div>
  )
}

function ConnectionError({ error }: { error: string }) {
  return (
    <div className="error-state">
      <div className="error-state__title">Connection error</div>
      <div className="error-state__sub">{error}</div>
    </div>
  )
}

function ProtectedRoute() {
  const { isLoading, isAuthenticated, user, error } = useAuth()
  const location = useLocation()
  const needsLogin = !isLoading && !error && !isAuthenticated
  const returnUrl = `${location.pathname}${location.search}${location.hash}`
  useEffect(() => {
    if (needsLogin) window.location.href = `/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`
  }, [needsLogin, returnUrl])

  if (isLoading) return <Loading />
  if (error) return <ConnectionError error={error} />
  if (!isAuthenticated) return null

  const onOnboarding = location.pathname === '/onboarding' || location.pathname === '/signup'
  // An invite link works with or without a trust: the page itself says when the user already has one.
  const onInvite = location.pathname.startsWith('/invite/')
  if (!user?.hasTrust && !onOnboarding && !onInvite) {
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

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/signup" element={<SignupPage />} />
            <Route path="/onboarding" element={<OnboardingPage />} />
            <Route path="/invite/:token" element={<InvitePage />} />
            <Route element={<TrustRoute />}>
              <Route element={<Layout />}>
                <Route index element={<TodayPage />} />
                <Route path="/land-parcels"        element={<LandParcelsPage />} />
                <Route path="/homes"               element={<HomesPage />} />
                <Route path="/homes/selections/:selectionId" element={<HomeSelectionPage />} />
                <Route path="/home-selections"     element={<HomeSelectionsPage />} />
                <Route path="/development"         element={<DevelopmentProjectsPage />} />
                <Route path="/ground-leases"          element={<GroundLeasesPage />} />
                <Route path="/ground-lease-templates" element={<GroundLeaseTemplatesPage />} />
                <Route path="/applications" element={<ApplicationsPage />} />
                <Route path="/households"   element={<HouseholdsPage />} />
                <Route path="/waiting-list" element={<WaitingListPage />} />
                <Route path="/purchase-workflows"  element={<PurchaseWorkflowsPage />} />
                <Route path="/resales"             element={<ResalesPage />} />
                <Route path="/documents"           element={<DocumentsPage />} />
                <Route path="/stewardship"          element={<StewardshipPage />} />
                <Route path="/trust-policy"         element={<TrustPolicyPage />} />
                <Route path="/team"                 element={<TeamPage />} />
              </Route>
              <Route path="/lease-fee-notices/print" element={<LeaseFeeNoticesPrintPage />} />
            </Route>
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  )
}
