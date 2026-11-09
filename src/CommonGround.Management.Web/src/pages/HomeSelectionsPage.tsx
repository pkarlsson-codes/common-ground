import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import Pagination from '../components/Pagination'
import StatusBadge from '../components/StatusBadge'
import { formatStepDate } from '../lib/lifecycle'
import { homeSelectionPath, useHomeSelections } from '../hooks/homeSelections'

const PAGE_SIZE = 20

const STATUSES = ['Open', 'Completed', 'FellThrough', 'Cancelled'] as const
const STATUS_LABELS: Record<string, string> = { FellThrough: 'Fell Through' }

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

/** Home selections across every home, newest first; open ones by default. */
export default function HomeSelectionsPage() {
  const navigate = useNavigate()
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState<string>('Open')
  const { data, isLoading, isError, refetch } = useHomeSelections(page, status || undefined)
  const selections = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Home Selections</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} selection{data.totalCount !== 1 ? 's' : ''} · newest first
            </div>
          )}
        </div>
      </div>

      <div className="filter-bar">
        <select
          className="form-select"
          aria-label="Filter by status"
          value={status}
          onChange={e => { setStatus(e.target.value); setPage(1) }}
          style={{ maxWidth: 220 }}
        >
          <option value="">All Statuses</option>
          {STATUSES.map(s => <option key={s} value={s}>{STATUS_LABELS[s] ?? s}</option>)}
        </select>
      </div>

      <div className="registry-table-wrap">
        {isLoading && <SkeletonRows />}

        {isError && (
          <div className="error-state">
            <div className="error-state__title">Could not load home selections</div>
            <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
            <div style={{ marginTop: 10 }}>
              <button className="btn btn-ghost btn-sm" type="button" onClick={() => { void refetch() }}>Retry</button>
            </div>
          </div>
        )}

        {!isLoading && !isError && selections.length === 0 && (
          <div className="empty-state">
            <div className="empty-state__title">No home selections{status ? ` are ${(STATUS_LABELS[status] ?? status).toLowerCase()}` : ''}</div>
            <div className="empty-state__sub">Start a selection from an available home's detail panel.</div>
          </div>
        )}

        {!isLoading && !isError && selections.length > 0 && (
          <>
            <table className="registry-table">
              <thead>
                <tr>
                  <th>Home</th>
                  <th>Status</th>
                  <th>Candidates</th>
                  <th>Started</th>
                  <th>Chosen Household</th>
                </tr>
              </thead>
              <tbody>
                {selections.map(s => (
                  <tr key={s.id} onClick={() => navigate(homeSelectionPath(s.id!))}>
                    <td>{s.homeLabel ?? '—'}</td>
                    <td><StatusBadge status={s.status ?? ''} /></td>
                    <td><span className="cell-mono">{s.candidateCount}</span></td>
                    <td><span className="cell-mono">{formatStepDate(s.startedAt) ?? '—'}</span></td>
                    <td>{s.chosenContact?.name ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>

            {Number(data?.totalPages ?? 0) > 1 && (
              <Pagination
                page={page}
                totalPages={Number(data?.totalPages ?? 0)}
                totalCount={Number(data?.totalCount ?? 0)}
                pageSize={PAGE_SIZE}
                onPage={setPage}
              />
            )}
          </>
        )}
      </div>
    </div>
  )
}
