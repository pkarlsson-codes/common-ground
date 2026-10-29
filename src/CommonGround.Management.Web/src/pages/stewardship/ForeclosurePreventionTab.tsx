import { useMemo, useState } from 'react'
import type { ForeclosurePreventionCaseResponse } from '../../api/models/ForeclosurePreventionCaseResponse'
import { useForeclosurePreventionQueue } from '../../hooks/stewardship'
import { statusLabel } from '../../hooks/types'
import StatusBadge from '../../components/StatusBadge'
import Pagination from '../../components/Pagination'
import StewardshipFpcDetailPanel from '../../components/stewardship/StewardshipFpcDetailPanel'
import FpcOpenCasePanel from '../../components/stewardship/FpcOpenCasePanel'

const PAGE_SIZE = 20

// ── Helpers ──────────────────────────────────────────────────────────────────

/** The most recently recorded servicer delinquency, e.g. "First National · 3 mo". */
function delinquencySummary(fpc: ForeclosurePreventionCaseResponse): string | null {
  const latest = (fpc.servicerDelinquencies ?? []).at(-1)
  return latest ? `${latest.servicerName} · ${latest.delinquentMonths} mo` : null
}

function openInterventionCount(fpc: ForeclosurePreventionCaseResponse): number {
  const openReferrals = (fpc.counselingReferrals ?? []).filter(r => !r.outcome).length
  const openForbearance = (fpc.forbearanceRequests ?? []).filter(r => !r.outcome).length
  return openReferrals + openForbearance
}

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 6 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

interface SelectedCase {
  id: string
  homeownerFullName: string | null
  address: string | null
}

// ── Main component ───────────────────────────────────────────────────────────

export default function ForeclosurePreventionTab() {
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [selected, setSelected] = useState<SelectedCase | null>(null)
  const [openingNewCase, setOpeningNewCase] = useState(false)

  const { data: queue, isLoading, isError } = useForeclosurePreventionQueue()

  const filtered = useMemo(() => {
    const items = queue ?? []
    const term = search.trim().toLowerCase()
    if (!term) return items
    return items.filter(i =>
      (i.homeownerFullName ?? '').toLowerCase().includes(term)
      || (i.address ?? '').toLowerCase().includes(term))
  }, [queue, search])

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
  const pageItems = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)

  if (isLoading) return <SkeletonRows />

  if (isError) {
    return (
      <div className="error-state">
        <div className="error-state__title">Could not load foreclosure prevention cases</div>
        <div className="error-state__sub">Ensure the API and gateway are running.</div>
      </div>
    )
  }

  return (
    <>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          gap: 12,
          marginBottom: 16,
        }}
      >
        <input
          className="form-input"
          type="search"
          placeholder="Search by homeowner or address"
          aria-label="Search foreclosure prevention cases"
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1) }}
          style={{ maxWidth: 320 }}
        />
        <button
          className="btn btn-primary"
          onClick={() => setOpeningNewCase(true)}
        >
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
          </svg>
          Open New Case
        </button>
      </div>

      {filtered.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state__glyph">⊡</div>
          <div className="empty-state__title">
            {search.trim() ? 'No matching foreclosure prevention cases' : 'No foreclosure prevention cases'}
          </div>
          {!search.trim() && <div className="empty-state__sub">No open or active cases.</div>}
        </div>
      ) : (
        <div className="registry-table-wrap">
          <table className="registry-table">
            <thead>
              <tr>
                <th>Homeowner</th>
                <th>Address</th>
                <th>Status</th>
                <th>Trigger</th>
                <th>Delinquency</th>
                <th>Open interventions</th>
                <th>Opened</th>
              </tr>
            </thead>
            <tbody>
              {pageItems.map(({ foreclosurePreventionCase: fpc, homeownerFullName, address }) => {
                const openCount = openInterventionCount(fpc)
                const delinquency = delinquencySummary(fpc)
                return (
                  <tr
                    key={fpc.id}
                    onClick={() => setSelected({
                      id: fpc.id!,
                      homeownerFullName: homeownerFullName ?? null,
                      address: address ?? null,
                    })}
                    style={{ cursor: 'pointer' }}
                    className={selected?.id === fpc.id ? 'row--selected' : ''}
                  >
                    <td>{homeownerFullName ?? '—'}</td>
                    <td>{address ?? '—'}</td>
                    <td><StatusBadge status={fpc.status ?? ''} /></td>
                    <td>{statusLabel(fpc.trigger ?? '')}</td>
                    <td>
                      {delinquency
                        ? <span style={{ fontFamily: 'var(--font-mono)', fontSize: 12 }}>{delinquency}</span>
                        : <span style={{ color: 'var(--text-dim)' }}>—</span>}
                    </td>
                    <td>
                      {openCount > 0
                        ? <span className="badge badge-initiated">{openCount}</span>
                        : <span style={{ color: 'var(--text-dim)' }}>—</span>}
                    </td>
                    <td>
                      <span className="cell-mono">{fpc.openedDate ?? '—'}</span>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
          {totalPages > 1 && (
            <Pagination
              page={page}
              totalPages={totalPages}
              totalCount={filtered.length}
              pageSize={PAGE_SIZE}
              onPage={setPage}
            />
          )}
        </div>
      )}

      {selected && (
        <StewardshipFpcDetailPanel
          caseId={selected.id}
          homeownerName={selected.homeownerFullName ?? 'Foreclosure Prevention Case'}
          address={selected.address ?? ''}
          onClose={() => setSelected(null)}
        />
      )}

      {openingNewCase && (
        <FpcOpenCasePanel
          onClose={() => setOpeningNewCase(false)}
        />
      )}
    </>
  )
}
