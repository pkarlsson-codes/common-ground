import { useState } from 'react'
import type { ReminderResponse } from '../../api'
import UserName from '../../components/UserName'
import { useMarkReminderDone, useReminders } from '../../hooks/reminders'
import { useToast } from '../../hooks/useToast'
import { formatCalendarDate, formatInstant } from '../../lib/calendarDate'
import { getErrorMessage } from '../../lib/getErrorMessage'
import { reminderSectionLabel } from '../../lib/today'
import NewReminderPanel from './NewReminderPanel'

type Group = 'showing' | 'upcoming' | 'done'

const GROUPS: readonly { key: Group; title: string; empty: string }[] = [
  { key: 'showing', title: 'On Today', empty: 'No reminders are showing on Today.' },
  { key: 'upcoming', title: 'Upcoming', empty: 'No reminders are set for a later day.' },
  { key: 'done', title: 'Done in the last 30 days', empty: 'No reminders were marked done in the last 30 days.' },
]

function ReminderTable({ group, reminders }: { group: (typeof GROUPS)[number]; reminders: ReminderResponse[] }) {
  const markDone = useMarkReminderDone()
  const { success, error } = useToast()
  const done = group.key === 'done'

  const markReminderDone = async (reminder: ReminderResponse) => {
    try {
      await markDone.mutateAsync(reminder.id!)
      success('Reminder marked done')
    } catch (err) {
      error(await getErrorMessage(err, 'Could not mark the reminder done.'))
    }
  }

  return (
    <section className="today-section" aria-labelledby={`reminders-${group.key}`}>
      <div className="today-section__header">
        <h2 id={`reminders-${group.key}`} className="today-section__title">
          {group.title} <span className="today-section__count">{reminders.length}</span>
        </h2>
      </div>
      {reminders.length === 0 ? (
        <p className="today-hidden-note">{group.empty}</p>
      ) : (
        <div className="registry-table-wrap">
          <table className="registry-table">
            <thead>
              <tr>
                <th>Date</th>
                <th>Note</th>
                <th>Section</th>
                <th>Set by</th>
                {done ? <th>Done by</th> : <th><span className="sr-only">Actions</span></th>}
              </tr>
            </thead>
            <tbody>
              {reminders.map(r => (
                <tr key={r.id}>
                  <td>{r.remindOn ? formatCalendarDate(r.remindOn) : null}</td>
                  <td>{r.note}</td>
                  <td>{reminderSectionLabel(r.section)}</td>
                  <td><UserName id={r.setBy} /></td>
                  {done ? (
                    <td>
                      <UserName id={r.doneBy} />
                      {r.doneAt && <span style={{ color: 'var(--text-secondary)' }}> · {formatInstant(r.doneAt)}</span>}
                    </td>
                  ) : (
                    <td>
                      {group.key === 'showing' && (
                        <div className="today-actions">
                          <button
                            type="button"
                            className="btn btn-secondary btn-sm"
                            aria-label={`Mark done: ${r.note}`}
                            disabled={markDone.isPending}
                            onClick={() => { void markReminderDone(r) }}
                          >
                            Mark done
                          </button>
                        </div>
                      )}
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}

/** Every reminder the trust has set: showing on Today, upcoming, and done in the last 30 days. */
export default function RemindersPage() {
  const { data, isLoading, isError, refetch } = useReminders()
  const [creating, setCreating] = useState(false)

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <h1 className="page-header__title">Reminders</h1>
          <div className="page-header__count">Everyone in the trust sees every reminder.</div>
        </div>
        <button type="button" className="btn btn-primary btn-sm" onClick={() => setCreating(true)}>New reminder</button>
      </div>

      {isLoading && (
        <div className="skeleton-rows">
          {Array.from({ length: 4 }).map((_, i) => <div key={i} className="skeleton-row" />)}
        </div>
      )}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load reminders</div>
          <div className="error-state__sub">
            <button type="button" className="btn btn-ghost btn-sm" onClick={() => { void refetch() }}>Try again</button>
          </div>
        </div>
      )}

      {data && GROUPS.map(g => <ReminderTable key={g.key} group={g} reminders={data[g.key] ?? []} />)}

      {creating && <NewReminderPanel onClose={() => setCreating(false)} />}
    </div>
  )
}
