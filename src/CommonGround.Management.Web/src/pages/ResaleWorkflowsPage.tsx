import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useGroundLeases } from '../hooks/groundLeases'
import { FORMULA_KIND_LABELS } from '../hooks/types'
import StatusBadge from '../components/StatusBadge'
import Pagination from '../components/Pagination'
import ResaleSlideOver from './resale-workflows/ResaleSlideOver'
import { useToast } from '../hooks/useToast'

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

export default function ResaleWorkflowsPage() {
  const location = useLocation()
  const navigate = useNavigate()
  const { success } = useToast()

  const [page, setPage] = useState(1)
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')

  useEffect(() => {
    const timeout = setTimeout(() => {
      setSearch(searchInput.trim())
      setPage(1)
    }, 300)
    return () => clearTimeout(timeout)
  }, [searchInput])

  const { data, isLoading, isError } = useGroundLeases(page, 'Active', undefined, undefined, search || undefined)

  useEffect(() => {
    const openLeaseId = (location.state as { openLeaseId?: string } | null)?.openLeaseId
    if (!openLeaseId) return

    setSelectedId(openLeaseId)
    navigate(
      { pathname: location.pathname, search: location.search, hash: location.hash },
      { replace: true, state: null },
    )
  }, [location, navigate])

  const leases = data?.items ?? []

  const handleSuccess = (workflowId?: string) => {
    setSelectedId(null)
    success('Purchase workflow initiated')
    if (workflowId) {
      navigate('/purchase-workflows', { state: { openId: workflowId } })
    }
  }

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Resale Workflows</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} active lease{data.totalCount !== 1 ? 's' : ''}
            </div>
          )}
        </div>
      </div>

      <div className="filter-bar">
        <input
          className="form-input"
          type="search"
          placeholder="Search by homeowner name or email"
          aria-label="Search active ground leases"
          value={searchInput}
          onChange={e => setSearchInput(e.target.value)}
          style={{ maxWidth: 320 }}
        />
      </div>

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
            <div className="empty-state__title">No active ground leases</div>
            <div className="empty-state__sub">Resale workflows require an active ground lease.</div>
          </div>
        )}

        {!isLoading && !isError && leases.length > 0 && (
          <>
            <table className="registry-table">
              <thead>
                <tr>
                  <th>Homeowner</th>
                  <th>Start Date</th>
                  <th>Formula</th>
                  <th>Status</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {leases.map(l => (
                  <tr key={l.id} className={selectedId === l.id ? 'row--selected' : ''}>
                    <td>{l.homeownerFullName ?? <span style={{ color: 'var(--text-dim)' }}>—</span>}</td>
                    <td><span className="cell-mono">{l.leaseStartDate}</span></td>
                    <td>{FORMULA_KIND_LABELS[l.formulaKind ?? ''] ?? l.formulaKind}</td>
                    <td><StatusBadge status={l.status ?? ''} /></td>
                    <td>
                      <button
                        className="btn btn-secondary"
                        style={{ fontSize: 12, padding: '4px 10px' }}
                        onClick={() => setSelectedId(l.id!)}
                      >
                        Start Resale
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {data && (data.totalPages ?? 1) > 1 && (
              <Pagination
                page={page}
                totalPages={Number(data.totalPages)}
                totalCount={Number(data.totalCount)}
                pageSize={20}
                onPage={setPage}
              />
            )}
          </>
        )}
      </div>

      {selectedId && (
        <ResaleSlideOver
          groundLeaseId={selectedId}
          onClose={() => setSelectedId(null)}
          onSuccess={handleSuccess}
        />
      )}
    </div>
  )
}
