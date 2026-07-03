import { useEffect, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useGroundLeaseTemplates } from '../hooks/groundLeaseTemplates'
import { statusLabel } from '../hooks/types'
import GroundLeaseTemplatesTable from './ground-lease-templates/GroundLeaseTemplatesTable'
import CreateGroundLeaseTemplatePanel from './ground-lease-templates/CreateGroundLeaseTemplatePanel'
import GroundLeaseTemplateDetailPanel from './ground-lease-templates/GroundLeaseTemplateDetailPanel'

const TEMPLATE_STATUSES = ['Active', 'Inactive'] as const

export default function GroundLeaseTemplatesPage() {
  const location = useLocation()
  const navigate = useNavigate()

  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState('')
  const [panel, setPanel] = useState<'create' | null>(null)
  const [selectedId, setSelectedId] = useState<string | null>(null)

  const { data, isLoading, isError } = useGroundLeaseTemplates(page, undefined, statusFilter || undefined)

  useEffect(() => {
    const openId = (location.state as { openId?: string } | null)?.openId
    if (!openId) {
      return
    }

    setSelectedId(openId)
    navigate(
      { pathname: location.pathname, search: location.search, hash: location.hash },
      { replace: true, state: null },
    )
  }, [location, navigate])

  const templates = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Ground Lease Templates</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} template{data.totalCount !== 1 ? 's' : ''}
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={() => setPanel('create')}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          New Template
        </button>
      </div>

      <div className="filter-bar">
        <select
          className="select"
          value={statusFilter}
          onChange={e => { setStatusFilter(e.target.value); setPage(1) }}
        >
          <option value="">All Statuses</option>
          {TEMPLATE_STATUSES.map(s => (
            <option key={s} value={s}>{statusLabel(s)}</option>
          ))}
        </select>
      </div>

      <GroundLeaseTemplatesTable
        templates={templates}
        isLoading={isLoading}
        isError={isError}
        page={page}
        totalPages={Number(data?.totalPages ?? 0)}
        totalCount={Number(data?.totalCount ?? 0)}
        onPage={setPage}
        onSelect={id => setSelectedId(id)}
      />

      <CreateGroundLeaseTemplatePanel
        open={panel === 'create'}
        onClose={() => setPanel(null)}
      />

      {selectedId !== null && (
        <GroundLeaseTemplateDetailPanel
          templateId={selectedId}
          onClose={() => setSelectedId(null)}
        />
      )}
    </div>
  )
}
