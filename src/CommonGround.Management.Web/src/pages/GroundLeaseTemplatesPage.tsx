import { useState } from 'react'
import { useGroundLeaseTemplates } from '../hooks/groundLeaseTemplates'
import { statusLabel } from '../hooks/types'
import GroundLeaseTemplatesTable from './ground-lease-templates/GroundLeaseTemplatesTable'

const TEMPLATE_STATUSES = ['Active', 'Inactive'] as const

export default function GroundLeaseTemplatesPage() {
  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState('')

  const { data, isLoading, isError } = useGroundLeaseTemplates(page, undefined, statusFilter || undefined)

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
      />
    </div>
  )
}
