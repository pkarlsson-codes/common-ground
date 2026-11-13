import { CalendarCheck, ChevronRight } from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { useToday } from '../hooks/today'
import { useNavPreferences } from '../hooks/useNavPreferences'
import { FOCUSES, NAV_ITEMS, isFocus, splitWork, type Focus, type NavItem } from '../lib/navigation'
import { countForFocus, countsByPage } from '../lib/today'
import DemoBanner from './DemoBanner'

// The count is left out of the link's accessible name, so the link keeps its label's name.
function NavItemLink({ item, count }: { item: Pick<NavItem, 'to' | 'icon' | 'label'>; count?: number }) {
  const Icon = item.icon
  return (
    <NavLink
      to={item.to}
      end={item.to === '/'}
      className={({ isActive }) => `nav-item${isActive ? ' active' : ''}`}
    >
      <Icon size={16} strokeWidth={1.6} />
      {item.label}
      {count ? <span className="nav-item__count" aria-hidden="true">{count}</span> : null}
    </NavLink>
  )
}

/** What the layout shares with its pages: the focus chosen in the sidebar. */
export interface LayoutContext {
  focus: Focus
}

export default function Layout() {
  const { user, logout } = useAuth()
  const { focus, setFocus, otherWorkOpen, setOtherWorkOpen } = useNavPreferences()
  const { mine, other } = splitWork(NAV_ITEMS, focus)
  const records = NAV_ITEMS.filter(i => i.group === 'records')
  const settings = NAV_ITEMS.filter(i => i.group === 'settings')
  const { data: today } = useToday()
  const counts = countsByPage(today?.kindCounts ?? [])
  const todayCount = focus === 'everything' ? today?.totalCount : countForFocus(today?.kindCounts ?? [], focus)
  const otherWorkCount = other.reduce((sum, item) => sum + (counts[item.to] ?? 0), 0)
  const link = (item: NavItem) => <NavItemLink key={item.to} item={item} count={counts[item.to]} />

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <div className="sidebar-brand__kicker">CLT Registry</div>
          <div className="sidebar-brand__name">CommonGround</div>
          <div className="sidebar-brand__sub">Property Register</div>
        </div>
        <nav className="sidebar-nav">
          <NavItemLink item={{ to: '/', icon: CalendarCheck, label: 'Today' }} count={todayCount} />
          {focus === 'everything' ? (
            <>
              <div className="sidebar-nav__section">Work</div>
              {mine.map(link)}
            </>
          ) : (
            <>
              <div className="sidebar-nav__section">My work</div>
              {mine.map(link)}
              <button
                type="button"
                className="sidebar-nav__toggle"
                aria-expanded={otherWorkOpen}
                onClick={() => setOtherWorkOpen(!otherWorkOpen)}
              >
                <ChevronRight size={12} className="sidebar-nav__toggle-chevron" />
                Other work
                {!otherWorkOpen && otherWorkCount > 0 && (
                  <span className="nav-item__count" aria-hidden="true">{otherWorkCount}</span>
                )}
              </button>
              {otherWorkOpen && other.map(link)}
            </>
          )}

          <div className="sidebar-nav__section">Records</div>
          {records.map(link)}

          <div className="sidebar-nav__section">Settings</div>
          {settings.map(link)}
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
        <div className="sidebar__account">
          <div className="sidebar__user">
            {user?.username}
          </div>
          <button className="btn btn-ghost btn-sm sidebar__logout" onClick={logout}>
            Log out
          </button>
        </div>
        <div className="sidebar-footer">Property Registry · v0.1</div>
      </aside>
      <main className="main-content">
        <DemoBanner />
        <Outlet context={{ focus } satisfies LayoutContext} />
      </main>
    </div>
  )
}
