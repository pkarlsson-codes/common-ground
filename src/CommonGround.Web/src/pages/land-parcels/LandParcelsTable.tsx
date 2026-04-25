import type { useLandParcels } from '../../hooks/landParcels'
import Pagination from '../../components/Pagination'
import StatusBadge from '../../components/StatusBadge'

type LandParcel = NonNullable<NonNullable<ReturnType<typeof useLandParcels>['data']>['items']>[number]

const PAGE_SIZE = 20

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

interface Props {
  parcels: LandParcel[]
  isLoading: boolean
  isError: boolean
  selectedId: string | null
  panel: 'detail' | 'register' | null
  page: number
  totalPages: number
  totalCount: number
  onRowClick: (id: string) => void
  onPage: (page: number) => void
}

export default function LandParcelsTable({
  parcels,
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
          <div className="error-state__title">Could not load parcels</div>
          <div className="error-state__sub">Ensure the API and gateway are running.</div>
        </div>
      )}

      {!isLoading && !isError && parcels.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__glyph">⬡</div>
          <div className="empty-state__title">No parcels registered</div>
          <div className="empty-state__sub">Register your first land parcel to get started.</div>
        </div>
      )}

      {!isLoading && !isError && parcels.length > 0 && (
        <>
          <table className="registry-table">
            <thead>
              <tr>
                <th>Parcel No.</th>
                <th>Address</th>
                <th>Area (ac)</th>
                <th>Status</th>
                <th>Registered</th>
              </tr>
            </thead>
            <tbody>
              {parcels.map(p => (
                <tr
                  key={p.id}
                  onClick={() => onRowClick(p.id!)}
                  className={selectedId === p.id && panel === 'detail' ? 'row--selected' : ''}
                >
                  <td><span className="cell-mono">{p.legalParcelNumber}</span></td>
                  <td>{p.address}</td>
                  <td><span className="cell-mono">{Number(p.areaAcres).toFixed(2)}</span></td>
                  <td><StatusBadge status={p.status ?? ''} /></td>
                  <td><span className="cell-mono">{new Date(p.registeredAt!).toLocaleDateString()}</span></td>
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
