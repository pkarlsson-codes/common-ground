import Pagination from '../../components/Pagination'
import StatusBadge from '../../components/StatusBadge'
import { PROJECT_TYPE_LABELS } from '../../hooks/types'

interface DevelopmentProjectListItem {
  id: string
  landParcelId: string
  name: string
  status: string
  projectType: string
}

const PAGE_SIZE = 20

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

interface Props {
  projects: DevelopmentProjectListItem[]
  isLoading: boolean
  isError: boolean
  selectedId: string | null
  panel: 'detail' | 'initiate' | null
  page: number
  totalPages: number
  totalCount: number
  onRowClick: (id: string) => void
  onPage: (page: number) => void
}

export default function DevelopmentProjectsTable({
  projects,
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
          <div className="error-state__title">Could not load development projects</div>
          <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
        </div>
      )}

      {!isLoading && !isError && projects.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__glyph">◈</div>
          <div className="empty-state__title">No development projects</div>
          <div className="empty-state__sub">Initiate a project on an existing land parcel.</div>
        </div>
      )}

      {!isLoading && !isError && projects.length > 0 && (
        <>
          <table className="registry-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Parcel</th>
                <th>Type</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {projects.map(p => (
                <tr
                  key={p.id}
                  onClick={() => onRowClick(p.id)}
                  className={selectedId === p.id && panel === 'detail' ? 'row--selected' : ''}
                >
                  <td>{p.name}</td>
                  <td>
                    <span className="cell-id" title={p.landParcelId}>
                      {p.landParcelId.slice(0, 8)}…
                    </span>
                  </td>
                  <td>{PROJECT_TYPE_LABELS[p.projectType] ?? p.projectType}</td>
                  <td><StatusBadge status={p.status} /></td>
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
