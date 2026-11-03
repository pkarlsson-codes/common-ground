import { useState } from 'react'
import { Link, useNavigate, useOutletContext } from 'react-router-dom'
import type { WorkItemResponse } from '../api'
import { useToday } from '../hooks/today'
import type { LayoutContext } from '../components/Layout'
import { daysBetween, formatCalendarDate, type CalendarDate } from '../lib/calendarDate'
import { TODAY_SECTIONS, areasForFocus, recordLink, type TodaySection } from '../lib/today'

const KIND_LABELS: Record<string, string> = {
  'application-awaiting-decision': 'Application to decide',
  'readiness-step-missing': 'Readiness incomplete',
  'prequalification-expiring': 'Prequalification expiring',
  'selection-open': 'Selection open',
  'home-available': 'Home available',
  'purchase-step-waiting': 'Purchase waiting',
  'purchase-stalled': 'Purchase stalled',
  'closing-scheduled': 'Closing coming up',
  'document-expiring': 'Document expiring',
}

const plural = (n: number, word: string) => `${n.toLocaleString()} ${word}${n === 1 ? '' : 's'}`

function DueChip({ item, today }: { item: WorkItemResponse; today: CalendarDate }) {
  if (item.dueDate) {
    const due = formatCalendarDate(item.dueDate)
    return (
      <span className={`due-chip due-chip--${item.urgency ?? 'plain'}`}>
        {item.urgency === 'overdue' ? `Overdue · ${due}` : `Due ${due}`}
      </span>
    )
  }

  const days = item.waitingSince ? daysBetween(item.waitingSince, today) : 0
  return <span className="due-chip due-chip--waiting">{days > 0 ? `Waiting ${plural(days, 'day')}` : 'Since today'}</span>
}

function SectionTable({ section, items, today }: { section: TodaySection; items: WorkItemResponse[]; today: CalendarDate }) {
  const navigate = useNavigate()

  return (
    <section className="today-section" aria-labelledby={`today-${section.area}`}>
      <div className="today-section__header">
        <h2 id={`today-${section.area}`} className="today-section__title">
          {section.label} <span className="today-section__count">{items.length}</span>
        </h2>
        <Link className="today-section__link" to={section.to}>{section.linkLabel} →</Link>
      </div>
      <div className="registry-table-wrap">
        <table className="registry-table">
          <thead>
            <tr>
              <th>What</th>
              <th>Record</th>
              <th>Detail</th>
              <th>Due</th>
            </tr>
          </thead>
          <tbody>
            {items.map(item => {
              const link = recordLink(item)
              return (
                <tr
                  key={`${item.kind}-${item.recordId}`}
                  onClick={link ? () => navigate(link.to, { state: link.state }) : undefined}
                >
                  <td>{KIND_LABELS[item.kind ?? ''] ?? item.kind}</td>
                  <td>{item.label}</td>
                  <td style={{ color: 'var(--text-secondary)' }}>{item.detail}</td>
                  <td><DueChip item={item} today={today} /></td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
    </section>
  )
}

/** The landing page: everything waiting on staff, grouped by area, most urgent first. */
export default function TodayPage() {
  const { data, isLoading, isError, refetch } = useToday()
  const { focus } = useOutletContext<LayoutContext>()
  const [showAll, setShowAll] = useState(false)

  const focusAreas = areasForFocus(focus)
  const shownAreas = showAll ? areasForFocus('everything') : focusAreas
  const items = data?.items ?? []
  const shown = items.filter(i => (shownAreas as readonly string[]).includes(i.area ?? ''))
  const hiddenCount = items.length - shown.length
  const sections = TODAY_SECTIONS
    .filter(s => shownAreas.includes(s.area))
    .map(s => ({ section: s, items: shown.filter(i => i.area === s.area) }))
    .filter(s => s.items.length > 0)

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">{data?.today ? formatCalendarDate(data.today, { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' }) : 'Today'}</div>
          <h1 className="page-header__title">Today</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount ? `${plural(data.totalCount, 'thing')} need${data.totalCount === 1 ? 's' : ''} attention` : 'Nothing needs attention'}
            </div>
          )}
        </div>
        {focus !== 'everything' && (
          <div className="today-toggle" role="group" aria-label="Areas shown">
            <button type="button" className="btn btn-secondary btn-sm" aria-pressed={!showAll} onClick={() => setShowAll(false)}>My areas</button>
            <button type="button" className="btn btn-secondary btn-sm" aria-pressed={showAll} onClick={() => setShowAll(true)}>All areas</button>
          </div>
        )}
      </div>

      {!showAll && hiddenCount > 0 && (
        <p className="today-hidden-note">
          {plural(hiddenCount, 'item')} in other areas {hiddenCount === 1 ? 'is' : 'are'} hidden.
        </p>
      )}

      {isLoading && (
        <div className="skeleton-rows">
          {Array.from({ length: 6 }).map((_, i) => <div key={i} className="skeleton-row" />)}
        </div>
      )}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load Today</div>
          <div className="error-state__sub">
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => { void refetch() }}>Try again</button>
          </div>
        </div>
      )}

      {data && sections.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__title">All clear</div>
          <div className="empty-state__sub">Nothing in {showAll ? 'any area' : 'your areas'} needs attention.</div>
        </div>
      )}

      {data?.today && sections.map(({ section, items: sectionItems }) => (
        <SectionTable key={section.area} section={section} items={sectionItems} today={data.today!} />
      ))}
    </div>
  )
}
