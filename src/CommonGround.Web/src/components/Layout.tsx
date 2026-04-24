import { NavLink, Outlet } from 'react-router-dom'

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

export default function Layout() {
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
            to="/buildings"
            className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
          >
            <IconBuilding />
            Buildings
          </NavLink>
        </nav>
        <div className="sidebar-footer">Property Registry · v0.1</div>
      </aside>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  )
}
