import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

function IconMap() {
  return (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
      <path d="M10 2L6 4L2 2v10l4 2 4-2 4 2V4l-4-2z" stroke="currentColor" strokeWidth="1.2" strokeLinejoin="round"/>
      <path d="M6 4v10M10 2v10" stroke="currentColor" strokeWidth="1.2"/>
    </svg>
  )
}

function IconBuilding() {
  return (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
      <rect x="2" y="7" width="12" height="8" rx="1" stroke="currentColor" strokeWidth="1.2"/>
      <path d="M5 15v-5h6v5" stroke="currentColor" strokeWidth="1.2"/>
      <path d="M1 7l7-5 7 5" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" strokeLinejoin="round"/>
    </svg>
  )
}

function IconLease() {
  return (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
      <rect x="3" y="1" width="10" height="14" rx="1" stroke="currentColor" strokeWidth="1.2"/>
      <path d="M6 5h4M6 8h4M6 11h2" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round"/>
    </svg>
  )
}

function IconWorkflow() {
  return (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
      <circle cx="3" cy="8" r="2" stroke="currentColor" strokeWidth="1.2"/>
      <circle cx="13" cy="8" r="2" stroke="currentColor" strokeWidth="1.2"/>
      <path d="M5 8h4" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round"/>
      <circle cx="8" cy="3" r="1.5" stroke="currentColor" strokeWidth="1.2"/>
      <path d="M8 4.5v2" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round"/>
    </svg>
  )
}

function IconDocuments() {
  return (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
      <rect x="3" y="1" width="10" height="14" rx="1" stroke="currentColor" strokeWidth="1.2"/>
      <path d="M6 5h4M6 8h4M6 11h2" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round"/>
    </svg>
  )
}

function IconStewardship() {
  return (
    <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
      <circle cx="8" cy="5" r="3" stroke="currentColor" strokeWidth="1.2" />
      <path d="M2 14c0-3.314 2.686-5 6-5s6 1.686 6 5" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" />
      <path d="M5 11l2 2 4-4" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  )
}

export default function Layout() {
  const { user, logout } = useAuth()

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <div className="sidebar-brand__kicker">CLT Registry</div>
          <div className="sidebar-brand__name">CommonGround</div>
          <div className="sidebar-brand__sub">Property Register</div>
        </div>
        <nav className="sidebar-nav">
          <div className="sidebar-nav__section">Registry</div>
          <NavLink
            to="/land-parcels"
            className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
          >
            <IconMap />
            Land Parcels
          </NavLink>
          <NavLink
            to="/homes"
            className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
          >
            <IconBuilding />
            Homes
          </NavLink>
          <NavLink
            to="/development"
            className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
          >
            <IconBuilding />
            Development
          </NavLink>

          <div className="sidebar-nav__section">Homeownership</div>
          <NavLink
            to="/ground-leases"
            className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
          >
            <IconLease />
            Ground Leases
          </NavLink>
          <NavLink
            to="/resale-workflows"
            className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
          >
            <IconWorkflow />
            Resale Workflows
          </NavLink>
          <NavLink
            to="/purchase-workflows"
            className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
          >
            <IconWorkflow />
            Purchase Workflows
          </NavLink>

          <div className="sidebar-nav__section">Documents</div>
          <NavLink
            to="/documents"
            className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
          >
            <IconDocuments />
            Document Library
          </NavLink>

          <div className="sidebar-nav__section">Stewardship</div>
          <NavLink
            to="/stewardship"
            className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
          >
            <IconStewardship />
            Stewardship Queue
          </NavLink>
        </nav>
        <div style={{ borderTop: '1px solid var(--border)', padding: '12px 20px' }}>
          <div style={{ fontSize: '12px', color: 'var(--text-secondary)', marginBottom: '8px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
            {user?.username}
          </div>
          <button
            onClick={logout}
            style={{
              width: '100%',
              padding: '6px 10px',
              background: 'transparent',
              color: 'var(--text-dim)',
              border: '1px solid var(--border)',
              borderRadius: '6px',
              fontFamily: 'var(--font-body)',
              fontSize: '12px',
              cursor: 'pointer',
              textAlign: 'left',
              transition: 'color var(--transition), border-color var(--transition)',
            }}
            onMouseEnter={e => { (e.target as HTMLButtonElement).style.color = 'var(--text-primary)'; (e.target as HTMLButtonElement).style.borderColor = 'var(--border-bright)' }}
            onMouseLeave={e => { (e.target as HTMLButtonElement).style.color = 'var(--text-dim)'; (e.target as HTMLButtonElement).style.borderColor = 'var(--border)' }}
          >
            Log out
          </button>
        </div>
        <div className="sidebar-footer">Property Registry · v0.1</div>
      </aside>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  )
}
