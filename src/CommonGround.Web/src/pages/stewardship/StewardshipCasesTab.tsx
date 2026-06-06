import { useMemo, useState } from 'react'
import { useAllStewardshipCases } from '../../hooks/stewardship'
import { useAllActiveGroundLeases } from '../../hooks/groundLeases'
import type { GroundLeaseListItemResponse } from '../../api'
import type { OccupancyVerificationResponse } from '../../api/models/OccupancyVerificationResponse'
import {
  buildStewardshipQueue,
  type StewardshipQueueRow,
  type UrgencyLevel,
} from './stewardshipSortUtils'
import StewardshipCaseDetailPanel from '../../components/stewardship/StewardshipCaseDetailPanel'

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

  const [currentPage, setCurrentPage] = useState(1)

  const { data: casePage, isLoading: casesLoading, isError: casesError } = useAllStewardshipCases(currentPage)
  const { data: leasePage, isLoading: leasesLoading } = useAllActiveGroundLeases()

  const isLoading = casesLoading || leasesLoading
  const isError = casesError

  const [selectedRow, setSelectedRow] = useState<StewardshipQueueRow | null>(null)

  const leaseIndex = useMemo(() => {
    const map = new Map<string, GroundLeaseListItemResponse>()
    for (const l of leasePage?.items ?? []) {
      if (l.id) map.set(l.id, l)
    }
    return map
  }, [leasePage])

  const cases = casePage?.items ?? []
  const pagination = casePage
    ? { pageNumber: casePage.pageNumber, totalPages: casePage.totalPages, totalCount: casePage.totalCount, hasNextPage: casePage.hasNextPage ?? false }
    : null

  const rows = useMemo(
    () => buildStewardshipQueue(cases, leaseIndex as Map<string, { homeownerFullName?: string | null; parcelAddress?: string | null }>),
    [cases, leaseIndex],
  )

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
            {rows.map(row => (
              <tr
                key={row.caseId}
                className={URGENCY_ROW_CLASS[row.urgency]}
                onClick={() => setSelectedRow(row)}
                style={{ cursor: 'pointer' }}
              >
                <td>{row.homeownerName}</td>
                <td>{row.address}</td>
                <td><OccupancyCell verification={row.currentVerification} /></td>
                <td><ViolationCell count={row.activeViolationCount} /></td>
                <td>
                  {row.nextInspectionDate
                    ? <span className="cell-mono">{row.nextInspectionDate.toISOString().slice(0, 10)}</span>
                    : <span style={{ color: 'var(--text-dim)' }}>—</span>}
                </td>
                <td><span className="cell-mono">{row.openedDate.toISOString().slice(0, 10)}</span></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {pagination && pagination.totalPages > 1 && (
        <div className="pagination-controls" style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 12, justifyContent: 'flex-end', fontSize: 13 }}>
          <button
            className="btn btn-secondary"
            disabled={pagination.pageNumber <= 1}
            onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
          >
            Previous
          </button>
          <span style={{ color: 'var(--text-secondary)' }}>
            Page {pagination.pageNumber} of {pagination.totalPages}
          </span>
          <button
            className="btn btn-secondary"
            disabled={!pagination.hasNextPage}
            onClick={() => setCurrentPage(p => p + 1)}
          >
            Next
          </button>
        </div>
      )}

      {selectedRow && (
        <StewardshipCaseDetailPanel
          row={selectedRow}
          lease={leaseIndex.get(selectedRow.groundLeaseId)}
          onClose={() => setSelectedRow(null)}
        />
      )}
    </>
  )
}
