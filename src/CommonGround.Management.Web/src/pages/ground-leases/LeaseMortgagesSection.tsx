import { useState } from 'react'
import type { GroundLeaseResponse, PermittedMortgageDto } from '../../api'
import { MORTGAGE_SOURCE_LABELS } from '../../hooks/groundLeases'
import { formatCalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import EditPermittedMortgagePanel from './EditPermittedMortgagePanel'
import MarkMortgagePaidOffPanel from './MarkMortgagePaidOffPanel'
import RecordPermittedMortgagePanel from './RecordPermittedMortgagePanel'

/** The lease's debt cap (Art 8.3) and the mortgages the trust permitted on it (Art 8.1), current ones first. */
export default function LeaseMortgagesSection({ lease }: { lease: GroundLeaseResponse }) {
  const [recording, setRecording] = useState(false)
  const [editing, setEditing] = useState<PermittedMortgageDto | null>(null)
  const [payingOff, setPayingOff] = useState<PermittedMortgageDto | null>(null)
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
