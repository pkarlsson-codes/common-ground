import { useState, type ReactNode } from 'react'
import { useNavigate, useOutletContext } from 'react-router-dom'
import type { WorkItemResponse } from '../api'
import { useToday } from '../hooks/today'
import { useMarkReminderDone } from '../hooks/reminders'
import { useToast } from '../hooks/useToast'
import type { LayoutContext } from '../components/Layout'
import { daysBetween, formatCalendarDate, type CalendarDate } from '../lib/calendarDate'
import { getErrorMessage } from '../lib/getErrorMessage'
import {
  MARK_REMINDER_DONE,
  REMINDER_KIND,
  TODAY_SECTIONS,
  actionLink,
  areasForFocus,
  recordLink,
  rowActions,
  shownItems,
  type TodaySection,
} from '../lib/today'
import ApproveApplicationPanel from './applications/ApproveApplicationPanel'
import DenyApplicationPanel from './applications/DenyApplicationPanel'
import RecordCltOrientationPanel from './households/RecordCltOrientationPanel'
import RecordHousingCounselingPanel from './households/RecordHousingCounselingPanel'
import RecordMortgagePrequalificationPanel from './households/RecordMortgagePrequalificationPanel'
import TodayMortgageRequestPanel from './ground-leases/TodayMortgageRequestPanel'
import TodayImprovementRequestPanel from './ground-leases/TodayImprovementRequestPanel'
import TodayInsuranceCertificatePanel from './ground-leases/TodayInsuranceCertificatePanel'
import TodayResalePriceStatementPanel from './ground-leases/TodayResalePriceStatementPanel'
import TodayResalePanel from './resales/TodayResalePanel'
import NewReminderPanel from './reminders/NewReminderPanel'

const KIND_LABELS: Record<string, string> = {
  'application-awaiting-decision': 'Application to decide',
  'readiness-step-missing': 'Readiness incomplete',
  'prequalification-expiring': 'Prequalification expiring',
  'home-available': 'Home available',
  'purchase-step-waiting': 'Purchase waiting',
  'purchase-stalled': 'Purchase stalled',
  'closing-scheduled': 'Closing coming up',
  'lease-fee-notice-due': 'Lease fee notice due',
  'lease-default-uncured': 'Lease default not cured',
  'mortgage-request-pending': 'Mortgage request to decide',
  'improvement-request-waiting': 'Improvement request to decide',
  'mortgage-loan-closing-unrecorded': 'Mortgage loan closing to confirm',
  'resale-price-statement-due': 'Resale price statement due',
  'resale-decision-due': 'Resale decision due',
  'resale-closing-waiting': 'Resale closing to record',
  'insurance-lapsed': 'Insurance lapsed',
  'insurance-ending-soon': 'Insurance ending soon',
  'insurance-short-of-lease': 'Insurance short of the lease',
  'document-expiring': 'Document expiring',
  'income-limits-missing': 'Income limits missing',
  'income-limits-stale': 'Income limits out of date',
  reminder: 'Reminder',
}

type ActivePanel = { action: string; recordId: string }

/** The panel each action that opens over Today renders, given the item's record. */
const ACTION_PANELS: Record<string, (recordId: string, onClose: () => void) => ReactNode> = {
  'approve-application': (id, onClose) => <ApproveApplicationPanel applicationId={id} onClose={onClose} />,
  'deny-application': (id, onClose) => <DenyApplicationPanel applicationId={id} onClose={onClose} />,
  'record-housing-counseling': (id, onClose) => <RecordHousingCounselingPanel householdId={id} onClose={onClose} />,
  'record-clt-orientation': (id, onClose) => <RecordCltOrientationPanel householdId={id} onClose={onClose} />,
  'record-mortgage-prequalification': (id, onClose) => <RecordMortgagePrequalificationPanel householdId={id} onClose={onClose} />,
  // Mortgage items name the lease; the panel finds the request.
  'record-mortgage-request-appraisal': (id, onClose) => <TodayMortgageRequestPanel groundLeaseId={id} open="pending" onClose={onClose} />,
  'decide-mortgage-request': (id, onClose) => <TodayMortgageRequestPanel groundLeaseId={id} open="pending" onClose={onClose} />,
  'record-mortgage-loan-closed': (id, onClose) => <TodayMortgageRequestPanel groundLeaseId={id} open="closing" onClose={onClose} />,
  'decide-improvement-request': (id, onClose) => <TodayImprovementRequestPanel groundLeaseId={id} onClose={onClose} />,
  'record-resale-price-statement': (id, onClose) => <TodayResalePriceStatementPanel groundLeaseId={id} onClose={onClose} />,
  'record-resale-appraisal': (id, onClose) => <TodayResalePanel resaleId={id} action="appraisal" onClose={onClose} />,
  'exercise-purchase-option': (id, onClose) => <TodayResalePanel resaleId={id} action="exercise" onClose={onClose} />,
  'record-buyback-closing': (id, onClose) => <TodayResalePanel resaleId={id} action="buyback-closing" onClose={onClose} />,
  'record-insurance-certificate': (id, onClose) => <TodayInsuranceCertificatePanel groundLeaseId={id} onClose={onClose} />,
}

const plural = (n: number, word: string) => `${n.toLocaleString()} ${word}${n === 1 ? '' : 's'}`

function DueChip({ item, today }: { item: WorkItemResponse; today: CalendarDate }) {
  // A reminder's date is when it starts showing, not a deadline, so it never reads as overdue.
  if (item.kind === REMINDER_KIND && item.waitingSince) {
    return <span className="due-chip due-chip--waiting">Set for {formatCalendarDate(item.waitingSince)}</span>
  }

  if (item.dueDate) {
    const due = formatCalendarDate(item.dueDate)
    return (
      <span className={`due-chip due-chip--${item.urgency ?? 'plain'}`}>
        {item.urgency === 'overdue' ? `Overdue · ${due}` : `Due ${due}`}
      </span>
    )
  }

  if (!item.waitingSince) return null

  const days = daysBetween(item.waitingSince, today)
  return <span className="due-chip due-chip--waiting">{days > 0 ? `Waiting ${plural(days, 'day')}` : 'Since today'}</span>
}

function SectionTable({ section, items, today, onAction, onNewReminder }: {
  section: TodaySection
  items: WorkItemResponse[]
  today: CalendarDate
  onAction: (panel: ActivePanel) => void
  onNewReminder: (section: string | null) => void
}) {
  const navigate = useNavigate()
  const markDone = useMarkReminderDone()
  const { success, error } = useToast()

  const markReminderDone = async (item: WorkItemResponse) => {
    try {
      await markDone.mutateAsync(item.recordId!)
      success('Reminder marked done')
    } catch (err) {
      error(await getErrorMessage(err, 'Could not mark the reminder done.'))
    }
  }

  return (
    <section className="today-section" aria-labelledby={`today-${section.area}`}>
      <div className="today-section__header">
        <h2 id={`today-${section.area}`} className="today-section__title">
          {section.label} <span className="today-section__count">{items.length}</span>
        </h2>
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          aria-label={`New reminder in ${section.label}`}
          onClick={() => onNewReminder(section.area === 'reminders' ? null : section.area)}
        >
          New reminder
        </button>
      </div>
      <div className="registry-table-wrap">
        <table className="registry-table">
          <thead>
            <tr>
              <th>What</th>
              <th>Record</th>
              <th>Detail</th>
              <th>Due</th>
              <th><span className="sr-only">Actions</span></th>
            </tr>
          </thead>
          <tbody>
            {items.map(item => {
              const link = recordLink(item)
              const open = link ? () => navigate(link.to, { state: link.state }) : undefined
              return (
                <tr key={`${item.kind}-${item.recordId}`} onClick={open}>
                  <td>{KIND_LABELS[item.kind ?? ''] ?? item.kind}</td>
                  {item.kind === REMINDER_KIND ? (
                    <td colSpan={2}>{item.label}</td>
                  ) : (
                    <>
                      <td>{item.label}</td>
                      <td style={{ color: 'var(--text-secondary)' }}>{item.detail}</td>
                    </>
                  )}
                  <td><DueChip item={item} today={today} /></td>
                  <td>
                    <div className="today-actions">
                      {rowActions(item).map(action => (
                        <button
                          key={action.key}
                          type="button"
                          className="btn btn-secondary btn-sm"
                          aria-label={`${action.label}: ${item.label}`}
                          disabled={action.key === MARK_REMINDER_DONE && markDone.isPending}
                          onClick={e => {
                            e.stopPropagation()
                            if (action.key === MARK_REMINDER_DONE) void markReminderDone(item)
                            else if (action.opensPanel) onAction({ action: action.key, recordId: item.recordId! })
                            else {
                              const target = actionLink(item, action.key)
                              if (target) navigate(target.to, { state: target.state })
                            }
                          }}
                        >
                          {action.label}
                        </button>
                      ))}
                    </div>
                  </td>
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
  const [panel, setPanel] = useState<ActivePanel | null>(null)
  // undefined: no panel; null: a reminder in no section; otherwise the section to preselect.
  const [newReminder, setNewReminder] = useState<string | null | undefined>(undefined)

  const focusAreas = areasForFocus(focus)
  const shownAreas = showAll ? areasForFocus('everything') : focusAreas
  const items = data?.items ?? []
  // Reminders show whatever the focus, so a reminder can bring a section outside it onto the page.
  const shown = shownItems(items, shownAreas)
  const hiddenCount = items.length - shown.length
  const sections = TODAY_SECTIONS
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
        <div className="today-header-actions">
          {focus !== 'everything' && (
            <div className="today-toggle" role="group" aria-label="Areas shown">
              <button type="button" className="btn btn-secondary btn-sm" aria-pressed={!showAll} onClick={() => setShowAll(false)}>My areas</button>
              <button type="button" className="btn btn-secondary btn-sm" aria-pressed={showAll} onClick={() => setShowAll(true)}>All areas</button>
            </div>
          )}
          <button type="button" className="btn btn-primary btn-sm" onClick={() => setNewReminder(null)}>New reminder</button>
        </div>
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
        <SectionTable
          key={section.area}
          section={section}
          items={sectionItems}
          today={data.today!}
          onAction={setPanel}
          onNewReminder={setNewReminder}
        />
      ))}

      {panel && ACTION_PANELS[panel.action]?.(panel.recordId, () => setPanel(null))}
      {newReminder !== undefined && <NewReminderPanel section={newReminder} onClose={() => setNewReminder(undefined)} />}
    </div>
  )
}
