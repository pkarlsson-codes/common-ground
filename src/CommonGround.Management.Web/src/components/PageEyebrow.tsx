import { useLocation } from 'react-router-dom'
import { navGroupForPath } from '../lib/navigation'

/** The small label above a page title: the name of the sidebar group the page sits in. */
export default function PageEyebrow() {
  const { pathname } = useLocation()
  const label = navGroupForPath(pathname)
  return label ? <div className="page-header__eyebrow">{label}</div> : null
}
