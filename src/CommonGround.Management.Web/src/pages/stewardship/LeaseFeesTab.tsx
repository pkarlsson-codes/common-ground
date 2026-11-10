import { useState } from 'react'
import Pagination from '../../components/Pagination'
import { useLeaseFeeBalances } from '../../hooks/groundLeases'
import { formatPrice } from '../../lib/currency'
import GroundLeaseDetailPanel from '../ground-leases/GroundLeaseDetailPanel'
import { formatBalance, formatDaysOverdue, formatDueMonth } from '../ground-leases/leaseFeeFormat'

const PAGE_SIZE = 20

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 6 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

/** Leases with a balance due, oldest debt first; or every lease. */
export default function LeaseFeesTab() {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [includeAll, setIncludeAll] = useState(false)
  const [openLeaseId, setOpenLeaseId] = useState<string | null>(null)

  const { data, isLoading, isError } = useLeaseFeeBalances(page, includeAll, search.trim())
  const items = data?.items ?? []

  return (
    <>
      <div style={{ display: 'flex', gap: 12, alignItems: 'center', marginBottom: 16 }}>
        <input
          className="form-input"
          type="search"
          placeholder="Search by homeowner or home"
          aria-label="Search lease fees"
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1) }}
          style={{ maxWidth: 320 }}
        />
        <select
          className="form-select"
          aria-label="Lease fee filter"
          value={includeAll ? 'all' : 'due'}
          onChange={e => { setIncludeAll(e.target.value === 'all'); setPage(1) }}
          style={{ maxWidth: 180 }}
        >
          <option value="due">Balance due</option>
          <option value="all">All leases</option>
        </select>
      </div>

      {isLoading ? <SkeletonRows /> : isError ? (
        <div className="error-state">
          <div className="error-state__title">Could not load lease fees</div>
          <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
        </div>
      ) : items.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state__glyph">⊡</div>
          <div className="empty-state__title">
            {search.trim() ? 'No matching leases' : includeAll ? 'No leases' : 'No lease has a balance due.'}
          </div>
        </div>
      ) : (
        <div className="registry-table-wrap">
          <table className="registry-table" aria-label="Lease fee balances">
            <thead>
              <tr>
                <th>Homeowner</th>
                <th>Home</th>
                <th>Monthly Fee</th>
                <th>Balance</th>
                <th>Oldest Unpaid</th>
                <th>Days Overdue</th>
              </tr>
            </thead>
            <tbody>
              {items.map(b => (
                <tr
                  key={b.groundLeaseId}
                  onClick={() => setOpenLeaseId(b.groundLeaseId!)}
                  style={{ cursor: 'pointer' }}
                  className={openLeaseId === b.groundLeaseId ? 'row--selected' : ''}
                >
                  <td>{b.homeownerFullName ?? '—'}</td>
                  <td>{b.homeLabel ?? '—'}</td>
                  <td className="cell-mono">{formatPrice(b.monthlyFee ?? 0)}</td>
                  <td className="cell-mono" style={(b.balance ?? 0) > 0 ? { color: 'var(--red)' } : undefined}>
                    {formatBalance(b.balance ?? 0)}
                  </td>
                  <td>{b.oldestUnpaidDueDate ? formatDueMonth(b.oldestUnpaidDueDate) : '—'}</td>
                  <td className="cell-mono">{b.oldestUnpaidDueDate ? formatDaysOverdue(b.daysOverdue ?? 0) : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
          {(data?.totalPages ?? 1) > 1 && (
            <Pagination
              page={page}
              totalPages={data!.totalPages!}
              totalCount={data!.totalCount!}
              pageSize={PAGE_SIZE}
              onPage={setPage}
            />
          )}
        </div>
      )}

      {openLeaseId && <GroundLeaseDetailPanel groundLeaseId={openLeaseId} onClose={() => setOpenLeaseId(null)} />}
    </>
  )
}
