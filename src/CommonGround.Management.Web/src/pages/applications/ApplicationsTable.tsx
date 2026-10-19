import type { useHomeownerApplications } from '../../hooks/applications'
import Pagination from '../../components/Pagination'
import ApplicationPhaseBadge from './ApplicationPhaseBadge'

type Application = NonNullable<NonNullable<ReturnType<typeof useHomeownerApplications>['data']>['items']>[number]

type Props = {
  applications: Application[]
  isLoading: boolean
  isError: boolean
  error: unknown
  selectedId: string | null
  panel: 'detail' | 'submit' | null
  page: number
  totalPages: number
  totalCount: number
  pageSize: number
  hasFilters: boolean
  onRowClick: (id: string) => void
  onPage: (page: number) => void
  onRetry: () => void
}

const fmtDateTime = (value?: string | null) => {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString()
}

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

export default function ApplicationsTable({
  applications,
  isLoading,
  isError,
  error,
  selectedId,
  panel,
  page,
  totalPages,
  totalCount,
  pageSize,
  hasFilters,
  onRowClick,
  onPage,
  onRetry,
}: Props) {
  const status = typeof error === 'object' && error && 'response' in error
    ? Number((error as { response?: { status?: number } }).response?.status)
    : undefined

  const statusText = status === 404
    ? 'The applications endpoint was not found.'
    : status === 409
      ? 'A conflict occurred while loading applications.'
      : status === 422
        ? 'The current filters are invalid. Adjust and try again.'
        : 'Ensure the API and gateway are running.'

  return (
    <div className="registry-table-wrap">
      {isLoading && <SkeletonRows />}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load applications</div>
          <div className="error-state__sub">{statusText}</div>
          <div style={{ marginTop: 10 }}>
            <button className="btn btn-ghost btn-sm" type="button" onClick={onRetry}>Retry</button>
          </div>
        </div>
      )}

      {!isLoading && !isError && applications.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__title">
            {hasFilters ? 'No applications match your filters' : 'No homeowner applications yet'}
          </div>
          <div className="empty-state__sub">
            {hasFilters
              ? 'Try clearing the phase filter and run the query again.'
              : 'Submit an application to start the selection pipeline.'}
          </div>
        </div>
      )}

      {!isLoading && !isError && applications.length > 0 && (
        <>
          <table className="registry-table">
            <thead>
              <tr>
                <th>Applicant</th>
                <th>Email</th>
                <th>Household</th>
                <th>Accessibility</th>
                <th>Phase</th>
                <th>Submitted</th>
              </tr>
            </thead>
            <tbody>
              {applications.map(application => (
                <tr
                  key={application.id!}
                  onClick={() => onRowClick(application.id!)}
                  className={selectedId === application.id && panel === 'detail' ? 'row--selected' : ''}
                >
                  <td>{application.primaryContact?.name ?? '—'}</td>
                  <td>{application.primaryContact?.email ?? '—'}</td>
                  <td><span className="cell-mono">{application.householdSize ?? '—'}</span></td>
                  <td>{application.accessibilityRequired ? 'Yes' : 'No'}</td>
                  <td><ApplicationPhaseBadge phase={application.phase} /></td>
                  <td><span className="cell-mono">{fmtDateTime(application.submittedAt)}</span></td>
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
