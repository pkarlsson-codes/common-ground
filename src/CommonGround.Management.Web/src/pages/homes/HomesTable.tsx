import type { useHomes } from '../../hooks/homes'
import Pagination from '../../components/Pagination'
import StatusBadge from '../../components/StatusBadge'
import { homeLabel } from '../../lib/lifecycle'
import { HOME_TYPE_LABELS } from '../../hooks/types'

type Home = NonNullable<NonNullable<ReturnType<typeof useHomes>['data']>['items']>[number]

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
  homes: Home[]
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

export default function HomesTable({
  homes,
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
          <div className="error-state__title">Could not load homes</div>
          <div className="error-state__sub">Ensure the API and gateway are running.</div>
        </div>
      )}

      {!isLoading && !isError && homes.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__glyph">⌂</div>
          <div className="empty-state__title">No homes registered</div>
          <div className="empty-state__sub">Register a home on an existing land parcel.</div>
        </div>
      )}

      {!isLoading && !isError && homes.length > 0 && (
        <>
          <table className="registry-table">
            <thead>
              <tr>
                <th>Home</th>
                <th>Type</th>
                <th>Beds / Baths</th>
                <th>Sq. Ft.</th>
                <th>Assessed Value</th>
                <th>Status</th>
                <th>Parcel</th>
              </tr>
            </thead>
            <tbody>
              {homes.map(h => (
                <tr
                  key={h.id!}
                  onClick={() => onRowClick(h.id!)}
                  className={selectedId === h.id && panel === 'detail' ? 'row--selected' : ''}
                >
                  <td>{h.label || '—'}</td>
                  <td>{HOME_TYPE_LABELS[h.homeType ?? ''] ?? h.homeType}</td>
                  <td>
                    <span className="cell-mono">
                      {h.bedrooms} bd / {Number(h.bathrooms).toFixed(1)} ba
                    </span>
                  </td>
                  <td><span className="cell-mono">{fmtNumber(Number(h.squareFootage))}</span></td>
                  <td><span className="cell-mono">{fmtCurrency(Number(h.assessedValue))}</span></td>
                  <td><StatusBadge status={homeLabel(h.occupancy, h.saleStage)} /></td>
                  <td>
                    <span className="cell-id" title={h.landParcelId ?? ''}>
                      {(h.landParcelId ?? '').slice(0, 8)}…
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
