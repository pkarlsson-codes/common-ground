import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import PageEyebrow from '../../components/PageEyebrow'
import { describeLevels, useIncomeLimitSchedules } from '../../hooks/incomeLimits'
import { formatCalendarDate } from '../../lib/calendarDate'
import IncomeLimitScheduleDetailPanel from './IncomeLimitScheduleDetailPanel'
import IncomeLimitSchedulePanel from './IncomeLimitSchedulePanel'
import { useUserName } from '../../hooks/useUserName'

type Panel = { kind: 'enter' } | { kind: 'view'; id: string } | { kind: 'correct'; id: string } | null

export default function IncomeLimitsPage() {
  const { data, isLoading, isError, refetch } = useIncomeLimitSchedules()
  const location = useLocation()
  const navigate = useNavigate()
  // A link from Today opens the Enter panel, or a schedule's detail, on arrival.
  const [panel, setPanel] = useState<Panel>(() => {
    const state = location.state as { openId?: string; enter?: boolean } | null
    if (state?.enter) return { kind: 'enter' }
    return state?.openId ? { kind: 'view', id: state.openId } : null
  })

  useEffect(() => {
    if (!location.state) return
    navigate({ pathname: location.pathname, search: location.search, hash: location.hash }, { replace: true, state: null })
  }, [location, navigate])

  const schedules = data ?? []
  const newest = schedules[0] ?? null
  const panelSchedule = panel && panel.kind !== 'enter' ? schedules.find(s => s.id === panel.id) ?? null : null

  const nameOf = useUserName()

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <PageEyebrow />
          <h1 className="page-header__title">Income Limits</h1>
          {data && (
            <div className="page-header__count">
              {schedules.length} schedule{schedules.length !== 1 ? 's' : ''}
            </div>
          )}
        </div>
        <button className="btn btn-primary" type="button" onClick={() => setPanel({ kind: 'enter' })}>
          Enter Income Limits
        </button>
      </div>

      {isLoading && (
        <div className="skeleton-rows">
          {Array.from({ length: 3 }).map((_, i) => <div key={i} className="skeleton-row" />)}
        </div>
      )}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load income limits</div>
          <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
          <div style={{ marginTop: 10 }}>
            <button className="btn btn-ghost btn-sm" type="button" onClick={() => { void refetch() }}>Retry</button>
          </div>
        </div>
      )}

      {data && schedules.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__glyph">⊟</div>
          <div className="empty-state__title">No income limits entered</div>
          <div className="empty-state__sub">
            Selections for homes with an income limit can’t start until you enter them.
          </div>
        </div>
      )}

      {schedules.length > 0 && (
        <div className="registry-table-wrap">
          <table className="registry-table">
            <thead>
              <tr>
                <th>Effective From</th>
                <th>Levels</th>
                <th>Entered By</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {schedules.map(s => (
                <tr key={s.id} onClick={() => setPanel({ kind: 'view', id: s.id! })} style={{ cursor: 'pointer' }}>
                  <td className="cell-mono">{formatCalendarDate(s.effectiveFrom!)}</td>
                  <td>{describeLevels(s)}</td>
                  <td>{nameOf(s.enteredBy)}</td>
                  <td style={{ textAlign: 'right' }}>
                    {s.isInEffect && <span className="badge badge-active">In effect</span>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {panel?.kind === 'enter' && (
        <IncomeLimitSchedulePanel schedule={null} newest={newest} onClose={() => setPanel(null)} />
      )}

      {panel?.kind === 'view' && panelSchedule && (
        <IncomeLimitScheduleDetailPanel
          schedule={panelSchedule}
          nameOf={nameOf}
          onCorrect={() => setPanel({ kind: 'correct', id: panel.id })}
          onClose={() => setPanel(null)}
        />
      )}

      {panel?.kind === 'correct' && panelSchedule && (
        <IncomeLimitSchedulePanel schedule={panelSchedule} newest={newest} onClose={() => setPanel(null)} />
      )}
    </div>
  )
}
