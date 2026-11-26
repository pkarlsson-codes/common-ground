import PageEyebrow from '../components/PageEyebrow'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import Pagination from '../components/Pagination'
import { formatStepDate } from '../lib/lifecycle'
import { homeSelectionPath, useHomeSelections } from '../hooks/homeSelections'

const PAGE_SIZE = 20

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

/** The households chosen for homes across every home, newest first. */
export default function HomeSelectionsPage() {
  const navigate = useNavigate()
  const [page, setPage] = useState(1)
  const { data, isLoading, isError, refetch } = useHomeSelections(page)
  const selections = data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <PageEyebrow />
          <h1 className="page-header__title">Home Selections</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} household{data.totalCount !== 1 ? 's' : ''} chosen · newest first
            </div>
          )}
        </div>
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
            <div className="empty-state__title">No households have been chosen yet</div>
            <div className="empty-state__sub">Choose a household from an available home's selection page.</div>
          </div>
        )}

        {!isLoading && !isError && selections.length > 0 && (
          <>
            <table className="registry-table">
              <thead>
                <tr>
                  <th>Home</th>
                  <th>Chosen Household</th>
                  <th>Method</th>
                  <th>On the Waiting List</th>
                  <th>Chosen On</th>
                </tr>
              </thead>
              <tbody>
                {selections.map(s => (
                  <tr key={s.id} onClick={() => navigate(homeSelectionPath(s.homeId!))}>
                    <td>{s.homeLabel ?? '—'}</td>
                    <td>{s.chosenContact?.name ?? '—'}</td>
                    <td>{s.selectionMethod === 'Lottery' ? 'Lottery' : 'Priority'}</td>
                    <td><span className="cell-mono">{s.applicationCount}</span></td>
                    <td><span className="cell-mono">{formatStepDate(s.selectedDate) ?? '—'}</span></td>
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
