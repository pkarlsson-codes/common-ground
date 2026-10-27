import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useGroundLeases } from '../hooks/groundLeases'
import {
  GROUND_LEASE_STATUSES,
  statusLabel,
} from '../hooks/types'
import GroundLeasesTable from './ground-leases/GroundLeasesTable'
import GroundLeaseDetailPanel from './ground-leases/GroundLeaseDetailPanel'
import GroundLeaseCreatePanel from './ground-leases/GroundLeaseCreatePanel'

type Panel = 'detail' | 'create' | null

export default function GroundLeasesPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [panel, setPanel] = useState<Panel>(null)

  useEffect(() => {
    const timeout = setTimeout(() => {
      setSearch(searchInput.trim())
      setPage(1)
    }, 300)
    return () => clearTimeout(timeout)
  }, [searchInput])

  const { data, isLoading, isError } = useGroundLeases(page, statusFilter || undefined, undefined, undefined, search || undefined)

  const openDetail = (id: string) => {
    setSelectedId(id)
    setPanel('detail')
  }

  const openCreate = () => {
    setPanel('create')
  }

  useEffect(() => {
    const openId = (location.state as { openId?: string } | null)?.openId
    if (!openId) {
      return
    }

    openDetail(openId)
    navigate(
      { pathname: location.pathname, search: location.search, hash: location.hash },
      { replace: true, state: null },
    )
  }, [location, navigate])

  const closePanel = () => {
    setPanel(null)
    setSelectedId(null)
  }

  const leases = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Ground Leases</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} lease{data.totalCount !== 1 ? 's' : ''} in registry
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={openCreate}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          Create Lease
        </button>
      </div>

      <div className="filter-bar">
        <input
          className="form-input"
          type="search"
          placeholder="Search by homeowner name or email"
          aria-label="Search ground leases"
          value={searchInput}
          onChange={e => setSearchInput(e.target.value)}
          style={{ maxWidth: 320 }}
        />
        <select
          className="select"
          value={statusFilter}
          onChange={e => { setStatusFilter(e.target.value); setPage(1) }}
        >
          <option value="">All Statuses</option>
          {GROUND_LEASE_STATUSES.map(s => (
            <option key={s} value={s}>{statusLabel(s)}</option>
          ))}
        </select>
      </div>

      <GroundLeasesTable
        leases={leases}
        isLoading={isLoading}
        isError={isError}
        selectedId={selectedId}
        panel={panel}
        page={page}
        totalPages={Number(data?.totalPages ?? 0)}
        totalCount={Number(data?.totalCount ?? 0)}
        onRowClick={openDetail}
        onPage={setPage}
      />

      {panel === 'detail' && selectedId && (
        <GroundLeaseDetailPanel groundLeaseId={selectedId} onClose={closePanel} />
      )}

      {panel === 'create' && (
        <GroundLeaseCreatePanel onClose={closePanel} />
      )}

    </div>
  )
}
