import type { useGroundLeases } from '../../hooks/groundLeases'
import Pagination from '../../components/Pagination'
import StatusBadge from '../../components/StatusBadge'

type GroundLease = NonNullable<NonNullable<ReturnType<typeof useGroundLeases>['data']>['items']>[number]

const PAGE_SIZE = 20

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

interface Props {
  leases: GroundLease[]
  isLoading: boolean
  isError: boolean
  selectedId: string | null
  panel: 'detail' | 'create' | null
  page: number
  totalPages: number
  totalCount: number
  onRowClick: (id: string) => void
  onPage: (page: number) => void
}

export default function GroundLeasesTable({
  leases,
  isLoading,
  isError,
  selectedId,
  panel,
  page,
  totalPages,
  totalCount,
  onRowClick,
  onPage,
}: Props) {
  return (
    <div className="registry-table-wrap">
      {isLoading && <SkeletonRows />}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load ground leases</div>
          <div className="error-state__sub">Ensure the API and gateway are running.</div>
        </div>
      )}

      {!isLoading && !isError && leases.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__glyph">⊟</div>
          <div className="empty-state__title">No ground leases found</div>
          <div className="empty-state__sub">Create a ground lease on an existing land parcel and building.</div>
        </div>
      )}

      {!isLoading && !isError && leases.length > 0 && (
        <>
          <table className="registry-table">
            <thead>
              <tr>
                <th>Homeowner</th>
                <th>Start Date</th>
                <th>Term</th>
                <th>Status</th>
                <th>Home</th>
                <th>Parcel</th>
              </tr>
            </thead>
            <tbody>
              {leases.map(l => (
                <tr
                  key={l.id}
                  onClick={() => onRowClick(l.id!)}
                  className={selectedId === l.id && panel === 'detail' ? 'row--selected' : ''}
                >
                  <td>{l.homeownerFullName ?? <span style={{ color: 'var(--text-dim)' }}>—</span>}</td>
                  <td><span className="cell-mono">{l.leaseStartDate}</span></td>
                  <td><span className="cell-mono">{l.termYears} yr</span></td>
                  <td><StatusBadge status={l.status ?? ''} /></td>
                  <td>
                    <span className="cell-id" title={l.homeId}>
                      {l.homeId?.slice(0, 8)}…
                    </span>
                  </td>
                  <td>
                    <span className="cell-id" title={l.landParcelId}>
                      {l.landParcelId?.slice(0, 8)}…
                    </span>
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
              pageSize={PAGE_SIZE}
              onPage={onPage}
            />
          )}
        </>
      )}
    </div>
  )
}