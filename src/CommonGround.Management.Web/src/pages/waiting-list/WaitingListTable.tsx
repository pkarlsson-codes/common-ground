import type { WaitingListEntryResponse } from '../../api'
import Pagination from '../../components/Pagination'
import { localToday } from '../../lib/dates'
import { getResponseStatus } from '../../lib/getErrorMessage'

type Props = {
  entries: WaitingListEntryResponse[]
  isLoading: boolean
  isError: boolean
  error: unknown
  selectedHouseholdId: string | null
  page: number
  totalPages: number
  totalCount: number
  pageSize: number
  hasFilters: boolean
  onRowClick: (householdId: string) => void
  onPage: (page: number) => void
  onRetry: () => void
}

/** Formats a YYYY-MM-DD date without the UTC shift new Date() applies to date-only strings. */
const fmtDate = (value?: string | null) => {
  if (!value) return '—'
  const [year, month, day] = value.split('-').map(Number)
  return new Date(year, month - 1, day).toLocaleDateString()
}

/** Whole days from a YYYY-MM-DD date to the user's local today. */
const daysSince = (value?: string | null) => {
  if (!value) return '—'
  const toUtcDay = (iso: string) => {
    const [year, month, day] = iso.split('-').map(Number)
    return Date.UTC(year, month - 1, day) / 86_400_000
  }
  return toUtcDay(localToday()) - toUtcDay(value)
}

function ReadinessBadge({ entry }: { entry: WaitingListEntryResponse }) {
  if (entry.needsReverification) return <span className="badge badge-stalled">Needs re-verification</span>
  if (entry.isReadyForSelection) return <span className="badge badge-available">Ready</span>
  return <span className="badge badge-default">Not ready</span>
}

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

export default function WaitingListTable({
  entries,
  isLoading,
  isError,
  error,
  selectedHouseholdId,
  page,
  totalPages,
  totalCount,
  pageSize,
  hasFilters,
  onRowClick,
  onPage,
  onRetry,
}: Props) {
  const status = getResponseStatus(error)

  return (
    <div className="registry-table-wrap">
      {isLoading && <SkeletonRows />}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load the waiting list</div>
          <div className="error-state__sub">
            {status === 404 ? 'The waiting list endpoint was not found.' : 'Ensure the API and gateway are running.'}
          </div>
          <div style={{ marginTop: 10 }}>
            <button className="btn btn-ghost btn-sm" type="button" onClick={onRetry}>Retry</button>
          </div>
        </div>
      )}

      {!isLoading && !isError && entries.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__title">
            {hasFilters ? 'No waiting households match your search' : 'No households are waiting'}
          </div>
          <div className="empty-state__sub">
            {hasFilters
              ? 'Search matches any member’s name or email.'
              : 'Households join the waiting list when their application is approved.'}
          </div>
        </div>
      )}

      {!isLoading && !isError && entries.length > 0 && (
        <>
          <table className="registry-table">
            <thead>
              <tr>
                <th>Primary Contact</th>
                <th>Size</th>
                <th>Approved</th>
                <th>Days Waiting</th>
                <th>Prequalification Expires</th>
                <th>Readiness</th>
              </tr>
            </thead>
            <tbody>
              {entries.map(entry => (
                <tr
                  key={entry.applicationId!}
                  onClick={() => onRowClick(entry.householdId!)}
                  className={selectedHouseholdId === entry.householdId ? 'row--selected' : ''}
                >
                  <td>{entry.primaryContact?.name ?? '—'}</td>
                  <td><span className="cell-mono">{entry.householdSize ?? '—'}</span></td>
                  <td><span className="cell-mono">{fmtDate(entry.approvedDate)}</span></td>
                  <td><span className="cell-mono">{daysSince(entry.approvedDate)}</span></td>
                  <td><span className="cell-mono">{fmtDate(entry.mortgagePrequalificationExpiryDate)}</span></td>
                  <td><ReadinessBadge entry={entry} /></td>
                </tr>
              ))}
            </tbody>
          </table>

          {totalPages > 1 && (
            <Pagination
              page={page}
              totalPages={totalPages}
              totalCount={totalCount}
              pageSize={pageSize}
              onPage={onPage}
            />
          )}
        </>
      )}
    </div>
  )
}
