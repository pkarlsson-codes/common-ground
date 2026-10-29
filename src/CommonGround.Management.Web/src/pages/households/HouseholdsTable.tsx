import type { HouseholdListItemResponse } from '../../api'
import Pagination from '../../components/Pagination'
import { getResponseStatus } from '../../lib/getErrorMessage'
import { trustToday } from '../../lib/calendarDate'

type Props = {
  households: HouseholdListItemResponse[]
  isLoading: boolean
  isError: boolean
  error: unknown
  selectedId: string | null
  page: number
  totalPages: number
  totalCount: number
  pageSize: number
  hasFilters: boolean
  onRowClick: (id: string) => void
  onPage: (page: number) => void
  onRetry: () => void
}

/** Mirrors the selection readiness rule: counseling, orientation and a prequalification valid today. */
const isReady = (household: HouseholdListItemResponse) =>
  Boolean(household.isHousingCounselingCompleted) &&
  Boolean(household.isCltOrientationCompleted) &&
  Boolean(household.mortgagePrequalificationExpiryDate) &&
  household.mortgagePrequalificationExpiryDate! >= trustToday()

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

export default function HouseholdsTable({
  households,
  isLoading,
  isError,
  error,
  selectedId,
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
          <div className="error-state__title">Could not load households</div>
          <div className="error-state__sub">
            {status === 404 ? 'The households endpoint was not found.' : 'Ensure the API and gateway are running.'}
          </div>
          <div style={{ marginTop: 10 }}>
            <button className="btn btn-ghost btn-sm" type="button" onClick={onRetry}>Retry</button>
          </div>
        </div>
      )}

      {!isLoading && !isError && households.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__title">
            {hasFilters ? 'No households match your search' : 'No households yet'}
          </div>
          <div className="empty-state__sub">
            {hasFilters
              ? 'Search matches any member’s name or email.'
              : 'Households are registered when a new application is submitted.'}
          </div>
        </div>
      )}

      {!isLoading && !isError && households.length > 0 && (
        <>
          <table className="registry-table">
            <thead>
              <tr>
                <th>Primary Contact</th>
                <th>Email</th>
                <th>Size</th>
                <th>Readiness</th>
              </tr>
            </thead>
            <tbody>
              {households.map(household => (
                <tr
                  key={household.id!}
                  onClick={() => onRowClick(household.id!)}
                  className={selectedId === household.id ? 'row--selected' : ''}
                >
                  <td>{household.primaryContact?.name ?? '—'}</td>
                  <td>{household.primaryContact?.email ?? '—'}</td>
                  <td><span className="cell-mono">{household.householdSize ?? '—'}</span></td>
                  <td>
                    {isReady(household)
                      ? <span className="badge badge-available">Ready</span>
                      : <span className="badge badge-default">Not ready</span>}
                  </td>
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
