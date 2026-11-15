import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import { IMPROVEMENT_REQUEST_STATUS_LABELS, useImprovementRequests } from '../../hooks/groundLeases'
import { daysBetween, formatCalendarDate, trustToday } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import ImprovementRequestPanel from './ImprovementRequestPanel'
import RecordImprovementRequestPanel from './RecordImprovementRequestPanel'

/**
 * The credit the lease holds for approved improvements (Art 10) and what counts at resale under the lease's cap,
 * with the homeowner's requests to alter or add to the home, newest first.
 */
export default function LeaseImprovementsSection({ lease }: { lease: GroundLeaseResponse }) {
  const [recording, setRecording] = useState(false)
  const [openRequestId, setOpenRequestId] = useState<string | null>(null)
  const { data: requests } = useImprovementRequests(lease.id ?? null)
  const confirmed = lease.approvedImprovements ?? 0
  const cap = lease.improvementsCap ?? null
  const today = trustToday()

  return (
    <div className="detail-section">
      <div className="detail-section__title">Improvements</div>
      <div className="detail-grid" style={{ marginBottom: 12 }}>
        <div className="detail-field">
          <div className="detail-field__label">Confirmed Improvements</div>
          <div className="detail-field__value mono">{formatPrice(confirmed)}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Counted at Resale</div>
          <div className="detail-field__value">
            {cap == null
              ? "No improvements cap on this lease, so credits aren't counted"
              : <span className="cell-mono">{formatPrice(Math.min(confirmed, cap))} of {formatPrice(cap)} cap</span>}
          </div>
        </div>
      </div>

      {lease.status === 'Active' && (
        <div style={{ marginBottom: 12 }}>
          <button type="button" className="btn btn-primary" onClick={() => setRecording(true)}>Record Request</button>
        </div>
      )}

      {requests && requests.length > 0 ? (
        <div className="registry-table-wrap">
          <table className="registry-table" aria-label="Improvement requests">
            <thead>
              <tr>
                <th>Received</th>
                <th>Description</th>
                <th>Estimated Cost</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {[...requests]
                .sort((a, b) => (b.receivedOn ?? '').localeCompare(a.receivedOn ?? ''))
                .map(r => {
                  const waiting = r.status === 'Waiting for decision'
                  const lateDays = waiting && r.replyDueOn ? daysBetween(r.replyDueOn, today) : 0
                  return (
                    <tr
                      key={r.id}
                      tabIndex={0}
                      onClick={() => setOpenRequestId(r.id!)}
                      onKeyDown={e => { if (e.key === 'Enter') setOpenRequestId(r.id!) }}
                    >
                      <td className="cell-mono">{r.receivedOn ? formatCalendarDate(r.receivedOn) : '—'}</td>
                      <td>{r.description}</td>
                      <td className="cell-mono">{formatPrice(r.estimatedCost ?? 0)}</td>
                      <td>
                        {IMPROVEMENT_REQUEST_STATUS_LABELS[r.status ?? ''] ?? r.status}
                        {waiting && r.replyDueOn && (
                          <div style={{ fontSize: 11, color: lateDays > 0 ? 'var(--danger)' : 'var(--text-dim)' }}>
                            Reply due {formatCalendarDate(r.replyDueOn)}
                          </div>
                        )}
                      </td>
                    </tr>
                  )
                })}
            </tbody>
          </table>
        </div>
      ) : (
        <p style={{ fontSize: 13, color: 'var(--text-dim)' }}>No improvement requests recorded.</p>
      )}

      {recording && (
        <RecordImprovementRequestPanel
          lease={lease}
          onClose={() => setRecording(false)}
          onRecorded={id => {
            setRecording(false)
            setOpenRequestId(id)
          }}
        />
      )}
      {openRequestId && <ImprovementRequestPanel improvementRequestId={openRequestId} onClose={() => setOpenRequestId(null)} />}
    </div>
  )
}
