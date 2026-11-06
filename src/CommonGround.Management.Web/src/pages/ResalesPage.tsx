import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { optionWindowLeft, useOpenResales } from '../hooks/resales'
import Pagination from '../components/Pagination'

const PAGE_SIZE = 20

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

/** Resales in progress, each opened by a homeowner's notice of intent to sell on their lease. */
export default function ResalesPage() {
  const navigate = useNavigate()
  const [page, setPage] = useState(1)
  const { data, isLoading, isError } = useOpenResales(page)
  const resales = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Resales</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} resale{data.totalCount !== 1 ? 's' : ''} in progress
            </div>
          )}
        </div>
      </div>

      <div className="registry-table-wrap">
        {isLoading && <SkeletonRows />}

        {isError && (
          <div className="error-state">
            <div className="error-state__title">Could not load resales</div>
            <div className="error-state__sub">Ensure the API and gateway are running.</div>
          </div>
        )}

        {!isLoading && !isError && resales.length === 0 && (
          <div className="empty-state">
            <div className="empty-state__glyph">⊟</div>
            <div className="empty-state__title">No resales in progress</div>
            <div className="empty-state__sub">Resales start from recording a homeowner's intent to sell on their ground lease.</div>
          </div>
        )}

        {!isLoading && !isError && resales.length > 0 && (
          <>
            <table className="registry-table">
              <thead>
                <tr>
                  <th>Home</th>
                  <th>Homeowner</th>
                  <th>Notice Date</th>
                  <th>Option Window End</th>
                  <th>Window</th>
                </tr>
              </thead>
              <tbody>
                {resales.map(r => (
                  <tr key={r.id} onClick={() => navigate('/ground-leases', { state: { openId: r.groundLeaseId } })}>
                    <td>{r.homeLabel ?? <span style={{ color: 'var(--text-dim)' }}>—</span>}</td>
                    <td>{r.homeownerFullName ?? <span style={{ color: 'var(--text-dim)' }}>—</span>}</td>
                    <td><span className="cell-mono">{r.noticeDate}</span></td>
                    <td><span className="cell-mono">{r.optionWindowEndsOn}</span></td>
                    <td>{optionWindowLeft(r.optionWindowEndsOn!)}</td>
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
