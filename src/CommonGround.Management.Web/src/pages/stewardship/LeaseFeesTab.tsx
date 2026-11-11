import { useState } from 'react'
import Pagination from '../../components/Pagination'
import type { LeaseFeeBalanceResponse } from '../../api'
import { useLeaseFeeBalances, type LeaseFeeFilter } from '../../hooks/groundLeases'
import { formatCalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import GroundLeaseDetailPanel from '../ground-leases/GroundLeaseDetailPanel'
import { openNoticeLetters } from '../ground-leases/leaseFeeLetter'
import { formatBalance, formatDaysOverdue, formatDueMonth } from '../ground-leases/leaseFeeFormat'
import RecordLeaseFeeNoticesPanel from './RecordLeaseFeeNoticesPanel'

const PAGE_SIZE = 20

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 6 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

/** "First due", "Second due", the last notice sent in these arrears, or nothing. */
function noticeCell(b: LeaseFeeBalanceResponse) {
  if (b.noticeDue) return <span style={{ color: 'var(--warning)' }}>{b.noticeDue} due</span>
  if (b.lastNoticeLevel && b.lastNoticeSentOn) return `${b.lastNoticeLevel} sent ${formatCalendarDate(b.lastNoticeSentOn)}`
  return '—'
}

const EMPTY: Record<LeaseFeeFilter, string> = {
  due: 'No lease has a balance due.',
  notice: 'No notices are due.',
  all: 'No leases',
}

/** Leases with a balance due, oldest debt first; those with a late notice due; or every lease. */
export default function LeaseFeesTab() {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [filter, setFilter] = useState<LeaseFeeFilter>('due')
  const [openLeaseId, setOpenLeaseId] = useState<string | null>(null)
  const [recordingNotices, setRecordingNotices] = useState(false)

  const { data, isLoading, isError } = useLeaseFeeBalances(page, filter, search.trim())
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
          value={filter}
          onChange={e => { setFilter(e.target.value as LeaseFeeFilter); setPage(1) }}
          style={{ maxWidth: 180 }}
        >
          <option value="due">Balance due</option>
          <option value="notice">Notice due</option>
          <option value="all">All leases</option>
        </select>
        {filter === 'notice' && items.length > 0 && (
          <div style={{ display: 'flex', gap: 8, marginLeft: 'auto' }}>
            <button type="button" className="btn btn-secondary" onClick={() => openNoticeLetters()}>
              Print Notices
            </button>
            <button type="button" className="btn btn-primary" onClick={() => setRecordingNotices(true)}>
              Record Notices Sent
            </button>
          </div>
        )}
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
            {search.trim() ? 'No matching leases' : EMPTY[filter]}
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
                <th>Notice</th>
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
                  <td>{noticeCell(b)}</td>
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

      {recordingNotices && <RecordLeaseFeeNoticesPanel onClose={() => setRecordingNotices(false)} />}
      {openLeaseId && <GroundLeaseDetailPanel groundLeaseId={openLeaseId} onClose={() => setOpenLeaseId(null)} />}
    </>
  )
}
