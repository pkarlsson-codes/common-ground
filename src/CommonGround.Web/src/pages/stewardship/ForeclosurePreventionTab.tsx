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
  const [includeResolved, setIncludeResolved] = useState(false)
  const [selectedCase, setSelectedCase] = useState<ForeclosurePreventionCaseResponse | null>(null)
  const [openingNewCase, setOpeningNewCase] = useState(false)

  const { data: cases, isLoading: casesLoading, isError } = useAllForeclosurePreventionCases(includeResolved)
  const { data: leasePage, isLoading: leasesLoading } = useAllActiveGroundLeases()

  const isLoading = casesLoading || leasesLoading

  const leaseIndex = useMemo(() => {
    const map = new Map<string, GroundLeaseListItemResponse>()
    for (const l of leasePage?.items ?? []) {
      if (l.id) map.set(l.id, l)
    }
    return map
  }, [leasePage])

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

  const fpcCases = (cases ?? []).filter(fpc => {
    if (includeResolved) return true
    return fpc.status !== 'Resolved'
  })

  return (
    <>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 16,
          gap: 12,
          flexWrap: 'wrap',
        }}
      >
        <label
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 8,
            fontSize: 13,
            color: 'var(--text-secondary)',
            cursor: 'pointer',
          }}
        >
          <input
            type="checkbox"
            checked={includeResolved}
            onChange={e => setIncludeResolved(e.target.checked)}
          />
          Include resolved
        </label>

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
            {includeResolved
              ? 'No cases exist at all for this portfolio.'
              : 'No open or active cases. Toggle "Include resolved" to see history.'}
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
