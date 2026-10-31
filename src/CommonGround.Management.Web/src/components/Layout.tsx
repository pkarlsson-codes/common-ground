import { ChevronRight } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { useNavPreferences } from '../hooks/useNavPreferences'
import { FOCUSES, NAV_ITEMS, isFocus, splitWork, type NavItem } from '../lib/navigation'

function NavItemLink({ item }: { item: NavItem }) {
  const Icon = item.icon
  return (
    <NavLink
      to={item.to}
      className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
    >
      <Icon size={16} strokeWidth={1.6} />
      {item.label}
    </NavLink>
  )
}

export default function Layout() {
  const { user, logout } = useAuth()
  const { focus, setFocus, otherWorkOpen, setOtherWorkOpen } = useNavPreferences()
  const { mine, other } = splitWork(NAV_ITEMS, focus)
  const records = NAV_ITEMS.filter(i => i.group === 'records')
  const settings = NAV_ITEMS.filter(i => i.group === 'settings')

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <div className="sidebar-brand__kicker">CLT Registry</div>
          <div className="sidebar-brand__name">CommonGround</div>
          <div className="sidebar-brand__sub">Property Register</div>
        </div>
        <nav className="sidebar-nav">
          {focus === 'everything' ? (
            <>
              <div className="sidebar-nav__section">Work</div>
              {mine.map(item => <NavItemLink key={item.to} item={item} />)}
            </>
          ) : (
            <>
              <div className="sidebar-nav__section">My work</div>
              {mine.map(item => <NavItemLink key={item.to} item={item} />)}
              <button
                type="button"
                className="sidebar-nav__toggle"
                aria-expanded={otherWorkOpen}
                onClick={() => setOtherWorkOpen(!otherWorkOpen)}
              >
                <ChevronRight size={12} className="sidebar-nav__toggle-chevron" />
                Other work
              </button>
              {otherWorkOpen && other.map(item => <NavItemLink key={item.to} item={item} />)}
            </>
          )}

          <div className="sidebar-nav__section">Records</div>
          {records.map(item => <NavItemLink key={item.to} item={item} />)}

          <div className="sidebar-nav__section">Settings</div>
          {settings.map(item => <NavItemLink key={item.to} item={item} />)}
        </nav>
        <div className="sidebar-focus">
          <label className="form-label" htmlFor="sidebar-focus">My focus</label>
          <select
            id="sidebar-focus"
            className="form-select sidebar-focus__select"
            value={focus}
            onChange={e => { if (isFocus(e.target.value)) setFocus(e.target.value) }}
          >
            {FOCUSES.map(f => <option key={f.value} value={f.value}>{f.label}</option>)}
          </select>
        </div>
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
