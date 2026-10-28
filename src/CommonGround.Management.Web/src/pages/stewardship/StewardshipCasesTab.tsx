import { useMemo, useState } from 'react'
import { useStewardshipQueue } from '../../hooks/stewardship'
import type { OccupancyVerificationResponse } from '../../api/models/OccupancyVerificationResponse'
import {
  buildStewardshipQueue,
  type UrgencyLevel,
} from './stewardshipSortUtils'
import StewardshipCaseDetailPanel from '../../components/stewardship/StewardshipCaseDetailPanel'
import Pagination from '../../components/Pagination'
import { fmtD } from '../../components/stewardship/format'

const PAGE_SIZE = 20

// ── Urgency styling ──────────────────────────────────────────────────────────

const URGENCY_ROW_CLASS: Record<UrgencyLevel, string> = {
  red: 'queue-row--red',
  orange: 'queue-row--orange',
  yellow: 'queue-row--yellow',
  neutral: '',
}

const OCCUPANCY_BADGE_CLASS: Record<string, string> = {
  Pending:   'badge badge-initiated',
  Submitted: 'badge badge-inprogress',
  Verified:  'badge badge-active',
  Failed:    'badge badge-failed',
  Overdue:   'badge badge-failed',
}

// ── Sub-components ───────────────────────────────────────────────────────────

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 8 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

function OccupancyCell({
  verification,
}: {
  verification: OccupancyVerificationResponse | null
}) {
  if (!verification) {
    return <span style={{ color: 'var(--text-dim)' }}>—</span>
  }
  const cls = OCCUPANCY_BADGE_CLASS[verification.status ?? ''] ?? 'badge badge-default'
  return <span className={cls}>{verification.status}</span>
}

function ViolationCell({ count }: { count: number }) {
  if (count === 0) {
    return <span style={{ color: 'var(--text-dim)' }}>—</span>
  }
  return (
    <span className="badge badge-failed" style={{ fontVariantNumeric: 'tabular-nums' }}>
      {count}
    </span>
  )
}

// ── Main component ───────────────────────────────────────────────────────────

export default function StewardshipCasesTab() {
  const currentYear = new Date().getFullYear()

  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [selectedCaseId, setSelectedCaseId] = useState<string | null>(null)

  const { data: queue, isLoading, isError } = useStewardshipQueue()

  const rows = useMemo(() => buildStewardshipQueue(queue ?? []), [queue])

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase()
    if (!term) return rows
    return rows.filter(r =>
      r.homeownerName.toLowerCase().includes(term) || r.address.toLowerCase().includes(term))
  }, [rows, search])

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
  const pageRows = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)
  const selectedRow = rows.find(r => r.caseId === selectedCaseId) ?? null

  if (isLoading) return <SkeletonRows />

  if (isError) {
    return (
      <div className="error-state">
        <div className="error-state__title">Could not load stewardship cases</div>
        <div className="error-state__sub">Ensure the API and gateway are running.</div>
      </div>
    )
  }

  if (rows.length === 0) {
    return (
      <div className="empty-state">
        <div className="empty-state__glyph">⊟</div>
        <div className="empty-state__title">No active stewardship cases</div>
        <div className="empty-state__sub">
          Cases are opened automatically when a purchase closes.
        </div>
      </div>
    )
  }

  return (
    <>
      <div style={{ marginBottom: 16 }}>
        <input
          className="form-input"
          type="search"
          placeholder="Search by homeowner or address"
          aria-label="Search stewardship cases"
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1) }}
          style={{ maxWidth: 320 }}
        />
      </div>

      {filtered.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state__title">No matching stewardship cases</div>
        </div>
      ) : (
        <div className="registry-table-wrap">
          <table className="registry-table">
            <thead>
              <tr>
                <th>Homeowner</th>
                <th>Address</th>
                <th>Occupancy {currentYear}</th>
                <th>Active violations</th>
                <th>Next inspection</th>
                <th>Opened</th>
              </tr>
            </thead>
            <tbody>
              {pageRows.map(row => (
                <tr
                  key={row.caseId}
                  className={[URGENCY_ROW_CLASS[row.urgency], selectedCaseId === row.caseId ? 'row--selected' : '']
                    .filter(Boolean).join(' ')}
                  onClick={() => setSelectedCaseId(row.caseId)}
                  style={{ cursor: 'pointer' }}
                >
                  <td>{row.homeownerName}</td>
                  <td>{row.address}</td>
                  <td><OccupancyCell verification={row.currentVerification} /></td>
                  <td><ViolationCell count={row.activeViolationCount} /></td>
                  <td>
                    {row.nextInspectionDate
                      ? <span className="cell-mono">{fmtD(row.nextInspectionDate)}</span>
                      : <span style={{ color: 'var(--text-dim)' }}>—</span>}
                  </td>
                  <td><span className="cell-mono">{fmtD(row.openedDate)}</span></td>
                </tr>
              ))}
            </tbody>
          </table>
          {totalPages > 1 && (
            <Pagination
              page={page}
              totalPages={totalPages}
              totalCount={filtered.length}
              pageSize={PAGE_SIZE}
              onPage={setPage}
            />
          )}
        </div>
      )}

      {selectedRow && (
        <StewardshipCaseDetailPanel
          row={selectedRow}
          onClose={() => setSelectedCaseId(null)}
        />
      )}
    </>
  )
}
