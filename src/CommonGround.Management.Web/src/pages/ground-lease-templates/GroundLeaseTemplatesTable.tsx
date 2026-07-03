import type { useGroundLeaseTemplates } from '../../hooks/groundLeaseTemplates'
import Pagination from '../../components/Pagination'
import StatusBadge from '../../components/StatusBadge'

type GroundLeaseTemplate = NonNullable<NonNullable<ReturnType<typeof useGroundLeaseTemplates>['data']>['items']>[number]

const PAGE_SIZE = 20

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

interface Props {
  templates: GroundLeaseTemplate[]
  isLoading: boolean
  isError: boolean
  page: number
  totalPages: number
  totalCount: number
  onPage: (page: number) => void
  onSelect: (id: string) => void
}

export default function GroundLeaseTemplatesTable({
  templates,
  isLoading,
  isError,
  page,
  totalPages,
  totalCount,
  onPage,
  onSelect,
}: Props) {
  return (
    <div className="registry-table-wrap">
      {isLoading && <SkeletonRows />}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load ground lease templates</div>
          <div className="error-state__sub">Ensure the API and gateway are running.</div>
        </div>
      )}

      {!isLoading && !isError && templates.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__glyph">⊟</div>
          <div className="empty-state__title">No ground lease templates found</div>
          <div className="empty-state__sub">Create a ground lease template to use when issuing leases.</div>
        </div>
      )}

      {!isLoading && !isError && templates.length > 0 && (
        <>
          <table className="registry-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {templates.map(t => (
                <tr key={t.id} onClick={() => onSelect(t.id!)} style={{ cursor: 'pointer' }}>
                  <td>{t.templateName ?? <span style={{ color: 'var(--text-dim)' }}>—</span>}</td>
                  <td><StatusBadge status={t.status ?? ''} /></td>
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
