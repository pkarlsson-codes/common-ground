import { useState } from 'react'
import type { GroundLeaseResponse, PermittedMortgageDto } from '../../api'
import {
  MORTGAGE_PURPOSE_LABELS,
  MORTGAGE_REQUEST_STATUS_LABELS,
  MORTGAGE_SOURCE_LABELS,
  useMortgageRequests,
} from '../../hooks/groundLeases'
import { formatCalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import EditPermittedMortgagePanel from './EditPermittedMortgagePanel'
import MarkMortgagePaidOffPanel from './MarkMortgagePaidOffPanel'
import MortgageRequestPanel from './MortgageRequestPanel'
import RecordMortgageRequestPanel from './RecordMortgageRequestPanel'
import RecordPermittedMortgagePanel from './RecordPermittedMortgagePanel'

/**
 * The lease's debt cap (Art 8.3), the mortgages the trust permitted on it (Art 8.1), current ones first, and the
 * homeowner's requests for new loans, newest first.
 */
export default function LeaseMortgagesSection({ lease }: { lease: GroundLeaseResponse }) {
  const [recording, setRecording] = useState(false)
  const [editing, setEditing] = useState<PermittedMortgageDto | null>(null)
  const [payingOff, setPayingOff] = useState<PermittedMortgageDto | null>(null)
  const [requesting, setRequesting] = useState(false)
  const [openRequestId, setOpenRequestId] = useState<string | null>(null)
  const { data: requests } = useMortgageRequests(lease.id ?? null)
  const mortgages = lease.mortgages
  if (!mortgages) return null
  const permitted = mortgages.permitted ?? []

  return (
    <div className="detail-section">
      <div className="detail-section__title">Mortgages</div>
      <div className="detail-grid" style={{ marginBottom: 12 }}>
        <div className="detail-field">
          <div className="detail-field__label">Debt Cap</div>
          <div className="detail-field__value">{mortgages.maxMortgageDebtPercent}% of the purchase option price</div>
        </div>
      </div>

      <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
        {lease.status === 'Active' && (
          <button type="button" className="btn btn-primary" onClick={() => setRequesting(true)}>
            Record Request
          </button>
        )}
        <button type="button" className="btn btn-secondary" onClick={() => setRecording(true)}>
          Record Mortgage
        </button>
      </div>

      {permitted.length === 0 ? (
        <p style={{ fontSize: 13, color: 'var(--text-dim)' }}>No permitted mortgages recorded.</p>
      ) : (
        <div className="registry-table-wrap">
          <table className="registry-table" aria-label="Permitted mortgages">
            <thead>
              <tr>
                <th>Lender</th>
                <th>Original Principal</th>
                <th>Permitted</th>
                <th>Notice Address</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {permitted.map(m => {
                const paidOff = !!m.paidOffOn
                return (
                  <tr key={m.mortgageId} style={paidOff ? { color: 'var(--text-dim)' } : undefined}>
                    <td>{m.lender}</td>
                    <td className="cell-mono">{formatPrice(m.originalPrincipal ?? 0)}</td>
                    <td>
                      <span className="cell-mono">{m.permittedOn ? formatCalendarDate(m.permittedOn) : '—'}</span>
                      <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                        {MORTGAGE_SOURCE_LABELS[m.source ?? ''] ?? m.source}
                      </div>
                    </td>
                    <td style={{ whiteSpace: 'pre-line' }}>
                      {m.noticeAddress
                        ? m.noticeAddress
                        : paidOff
                          ? '—'
                          : <span style={{ color: 'var(--warning)' }}>Notice address not on file</span>}
                    </td>
                    <td style={{ whiteSpace: 'nowrap' }}>
                      {paidOff ? (
                        <span>Paid off {formatCalendarDate(m.paidOffOn!)}</span>
                      ) : (
                        <>
                          <button type="button" className="btn btn-ghost btn-sm" onClick={() => setEditing(m)}>
                            Edit
                          </button>
                          <button type="button" className="btn btn-ghost btn-sm" onClick={() => setPayingOff(m)}>
                            Paid Off
                          </button>
                        </>
                      )}
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      {requests && requests.length > 0 && (
        <div className="registry-table-wrap" style={{ marginTop: 16 }}>
          <table className="registry-table" aria-label="Mortgage requests">
            <thead>
              <tr>
                <th>Received</th>
                <th>Purpose</th>
                <th>Lender</th>
                <th>Principal</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {[...requests]
                .sort((a, b) => (b.receivedOn ?? '').localeCompare(a.receivedOn ?? ''))
                .map(r => (
                  <tr
                    key={r.id}
                    tabIndex={0}
                    onClick={() => setOpenRequestId(r.id!)}
                    onKeyDown={e => { if (e.key === 'Enter') setOpenRequestId(r.id!) }}
                  >
                    <td className="cell-mono">{r.receivedOn ? formatCalendarDate(r.receivedOn) : '—'}</td>
                    <td>{MORTGAGE_PURPOSE_LABELS[r.purpose ?? ''] ?? r.purpose}</td>
                    <td>{r.lender}</td>
                    <td className="cell-mono">{formatPrice(r.principal ?? 0)}</td>
                    <td>{MORTGAGE_REQUEST_STATUS_LABELS[r.status ?? ''] ?? r.status}</td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>
      )}

      {requesting && (
        <RecordMortgageRequestPanel
          lease={lease}
          onClose={() => setRequesting(false)}
          onRecorded={id => {
            setRequesting(false)
            setOpenRequestId(id)
          }}
        />
      )}
      {openRequestId && <MortgageRequestPanel mortgageRequestId={openRequestId} onClose={() => setOpenRequestId(null)} />}
      {recording && <RecordPermittedMortgagePanel lease={lease} onClose={() => setRecording(false)} />}
      {editing && (
        <EditPermittedMortgagePanel groundLeaseId={lease.id!} mortgage={editing} onClose={() => setEditing(null)} />
      )}
      {payingOff && (
        <MarkMortgagePaidOffPanel groundLeaseId={lease.id!} mortgage={payingOff} onClose={() => setPayingOff(null)} />
      )}
    </div>
  )
}
