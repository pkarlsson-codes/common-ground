import { useState } from 'react'
import { useResalePriceStatementsDue } from '../../hooks/groundLeases'
import { formatCalendarDate } from '../../lib/calendarDate'
import GroundLeaseDetailPanel from '../ground-leases/GroundLeaseDetailPanel'
import { formulaText } from '../ground-leases/resalePriceStatementLetter'
import PrepareResalePriceStatementsPanel from './PrepareResalePriceStatementsPanel'

type Filter = 'due' | 'all'

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 6 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

/** Leases on the yearly resale price statement cycle (Ch 23, Disclosure): those due, or all of them. */
export default function ResalePricesTab() {
  const [search, setSearch] = useState('')
  const [filter, setFilter] = useState<Filter>('due')
  const [openLeaseId, setOpenLeaseId] = useState<string | null>(null)
  const [preparing, setPreparing] = useState(false)

  const { data, isLoading, isError } = useResalePriceStatementsDue(filter === 'all', search.trim())
  const leases = data?.leases ?? []
  const today = data?.today

  return (
    <>
      <div style={{ display: 'flex', gap: 12, alignItems: 'center', marginBottom: 16, flexWrap: 'wrap' }}>
        <input
          className="form-input"
          type="search"
          placeholder="Search by homeowner or home"
          aria-label="Search resale prices"
          value={search}
          onChange={e => setSearch(e.target.value)}
          style={{ maxWidth: 320 }}
        />
        <select
          className="form-select"
          aria-label="Resale price filter"
          value={filter}
          onChange={e => setFilter(e.target.value as Filter)}
          style={{ maxWidth: 180 }}
        >
          <option value="due">Due</option>
          <option value="all">All leases</option>
        </select>
        {filter === 'due' && leases.length > 0 && (
          <button type="button" className="btn btn-primary" style={{ marginLeft: 'auto' }} onClick={() => setPreparing(true)}>
            Prepare Statements
          </button>
        )}
      </div>

      {isLoading ? <SkeletonRows /> : isError ? (
        <div className="error-state">
          <div className="error-state__title">Could not load resale prices</div>
          <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
        </div>
      ) : leases.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state__glyph">⊡</div>
          <div className="empty-state__title">
            {search.trim() ? 'No matching leases' : filter === 'due' ? 'No statements are due.' : 'No leases'}
          </div>
        </div>
      ) : (
        <div className="registry-table-wrap">
          <table className="registry-table" aria-label="Resale price statements">
            <thead>
              <tr>
                <th>Homeowner</th>
                <th>Home</th>
                <th>Formula</th>
                <th>Last Statement</th>
                <th>Next Due</th>
              </tr>
            </thead>
            <tbody>
              {leases.map(l => (
                <tr
                  key={l.groundLeaseId}
                  onClick={() => setOpenLeaseId(l.groundLeaseId!)}
                  style={{ cursor: 'pointer' }}
                  className={openLeaseId === l.groundLeaseId ? 'row--selected' : ''}
                >
                  <td>{l.homeownerFullName ?? '—'}</td>
                  <td>{l.homeLabel ?? '—'}</td>
                  <td>{formulaText(l)}</td>
                  <td>{l.lastStatementDate ? formatCalendarDate(l.lastStatementDate) : 'None'}</td>
                  <td style={today && l.nextDueOn! <= today ? { color: 'var(--red)' } : undefined}>
                    {formatCalendarDate(l.nextDueOn!)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {preparing && <PrepareResalePriceStatementsPanel onClose={() => setPreparing(false)} />}
      {openLeaseId && <GroundLeaseDetailPanel groundLeaseId={openLeaseId} onClose={() => setOpenLeaseId(null)} />}
    </>
  )
}
