import type { useBuildings } from '../../hooks/buildings'
import Pagination from '../../components/Pagination'
import StatusBadge from '../../components/StatusBadge'
import { BUILDING_TYPE_LABELS } from '../../hooks/types'

type Building = NonNullable<NonNullable<ReturnType<typeof useBuildings>['data']>['items']>[number]

const PAGE_SIZE = 20

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtNumber = (n: number) => new Intl.NumberFormat('en-US').format(n)

interface Props {
  buildings: Building[]
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

export default function BuildingsTable({
  buildings,
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
          <div className="error-state__title">Could not load buildings</div>
          <div className="error-state__sub">Ensure the API and gateway are running.</div>
        </div>
      )}

      {!isLoading && !isError && buildings.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__glyph">⌂</div>
          <div className="empty-state__title">No buildings registered</div>
          <div className="empty-state__sub">Register a building on an existing land parcel.</div>
        </div>
      )}

      {!isLoading && !isError && buildings.length > 0 && (
        <>
          <table className="registry-table">
            <thead>
              <tr>
                <th>Type</th>
                <th>Beds / Baths</th>
                <th>Sq. Ft.</th>
                <th>Assessed Value</th>
                <th>Status</th>
                <th>Parcel</th>
              </tr>
            </thead>
            <tbody>
              {buildings.map(b => (
                <tr
                  key={b.id!}
                  onClick={() => onRowClick(b.id!)}
                  className={selectedId === b.id && panel === 'detail' ? 'row--selected' : ''}
                >
                  <td>{BUILDING_TYPE_LABELS[b.buildingType ?? ''] ?? b.buildingType}</td>
                  <td>
                    <span className="cell-mono">
                      {b.bedrooms} bd / {Number(b.bathrooms).toFixed(1)} ba
                    </span>
                  </td>
                  <td><span className="cell-mono">{fmtNumber(Number(b.squareFootage))}</span></td>
                  <td><span className="cell-mono">{fmtCurrency(Number(b.assessedValue))}</span></td>
                  <td><StatusBadge status={b.status ?? ''} /></td>
                  <td>
                    <span className="cell-id" title={b.landParcelId ?? ''}>
                      {(b.landParcelId ?? '').slice(0, 8)}…
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
