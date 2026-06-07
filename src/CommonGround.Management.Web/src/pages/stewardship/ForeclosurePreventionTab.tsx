import { useState, useMemo } from 'react'
import type { ForeclosurePreventionCaseResponse } from '../../api/models/ForeclosurePreventionCaseResponse'
import type { GroundLeaseListItemResponse } from '../../api'
import { useAllForeclosurePreventionCases } from '../../hooks/stewardship'
import { useAllActiveGroundLeases } from '../../hooks/groundLeases'
import { statusLabel } from '../../hooks/types'
import StatusBadge from '../../components/StatusBadge'
import StewardshipFpcDetailPanel from '../../components/stewardship/StewardshipFpcDetailPanel'
import FpcOpenCasePanel from '../../components/stewardship/FpcOpenCasePanel'

// ── Helpers ──────────────────────────────────────────────────────────────────

function delinquencySummary(fpc: ForeclosurePreventionCaseResponse): string {
  return statusLabel(fpc.trigger ?? '')
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

// ── Main component ───────────────────────────────────────────────────────────

export default function ForeclosurePreventionTab() {
  const [currentPage, setCurrentPage] = useState(1)
  const [selectedCase, setSelectedCase] = useState<ForeclosurePreventionCaseResponse | null>(null)
  const [openingNewCase, setOpeningNewCase] = useState(false)

  const { data: casePage, isLoading: casesLoading, isError } = useAllForeclosurePreventionCases(currentPage)
  const { data: leasePage, isLoading: leasesLoading } = useAllActiveGroundLeases()

  const isLoading = casesLoading || leasesLoading

  const leaseIndex = useMemo(() => {
    const map = new Map<string, GroundLeaseListItemResponse>()
    for (const l of leasePage?.items ?? []) {
      if (l.id) map.set(l.id, l)
    }
    return map
  }, [leasePage])

  const fpcCases = casePage?.items ?? []
  const pagination = casePage
    ? { pageNumber: casePage.pageNumber, totalPages: casePage.totalPages, totalCount: casePage.totalCount, hasNextPage: casePage.hasNextPage ?? false }
    : null

  const selectedLease = selectedCase?.groundLeaseId
    ? leaseIndex.get(selectedCase.groundLeaseId)
    : null

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
          justifyContent: 'flex-end',
          alignItems: 'center',
          marginBottom: 16,
        }}
      >
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

      {fpcCases.length === 0 ? (
        <div className="empty-state">
          <div className="empty-state__glyph">⊡</div>
          <div className="empty-state__title">No foreclosure prevention cases</div>
          <div className="empty-state__sub">
            No open or active cases.
          </div>
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
              {fpcCases.map(fpc => {
                const lease = fpc.groundLeaseId ? leaseIndex.get(fpc.groundLeaseId) : undefined
                const openCount = openInterventionCount(fpc)
                return (
                  <tr
                    key={fpc.id}
                    onClick={() => setSelectedCase(fpc)}
                    style={{ cursor: 'pointer' }}
                    className={selectedCase?.id === fpc.id ? 'row--selected' : ''}
                  >
                    <td>{lease?.homeownerFullName ?? '—'}</td>
                    <td>
                      {(lease as { parcelAddress?: string } | undefined)?.parcelAddress ?? '—'}
                    </td>
                    <td><StatusBadge status={fpc.status ?? ''} /></td>
                    <td>{statusLabel(fpc.trigger ?? '')}</td>
                    <td>
                      <span style={{ fontFamily: 'var(--font-mono)', fontSize: 12 }}>
                        {delinquencySummary(fpc)}
                      </span>
                    </td>
                    <td>
                      {openCount > 0
                        ? <span className="badge badge-initiated">{openCount}</span>
                        : <span style={{ color: 'var(--text-dim)' }}>—</span>}
                    </td>
                    <td>
                      <span className="cell-mono">
                        {fpc.openedDate ? fpc.openedDate.toISOString().slice(0, 10) : '—'}
                      </span>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      {pagination && pagination.totalPages > 1 && (
        <div className="pagination-controls" style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 12, justifyContent: 'flex-end', fontSize: 13 }}>
          <button
            className="btn btn-secondary"
            disabled={pagination.pageNumber <= 1}
            onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
          >
            Previous
          </button>
          <span style={{ color: 'var(--text-secondary)' }}>
            Page {pagination.pageNumber} of {pagination.totalPages}
          </span>
          <button
            className="btn btn-secondary"
            disabled={!pagination.hasNextPage}
            onClick={() => setCurrentPage(p => p + 1)}
          >
            Next
          </button>
        </div>
      )}

      {selectedCase && (
        <StewardshipFpcDetailPanel
          fpcCase={selectedCase}
          homeownerName={selectedLease?.homeownerFullName ?? 'Foreclosure Prevention Case'}
          address={(selectedLease as { parcelAddress?: string } | undefined)?.parcelAddress ?? ''}
          onClose={() => setSelectedCase(null)}
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
