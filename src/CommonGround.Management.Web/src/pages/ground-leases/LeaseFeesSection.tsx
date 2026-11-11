import { useState } from 'react'
import type { GroundLeaseResponse, LeaseFeePaymentDto } from '../../api'
import { NOTICE_DELIVERY_LABELS, NOTICE_LEVEL_LABELS, PAYMENT_METHOD_LABELS } from '../../hooks/groundLeases'
import { formatCalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { formatBalance, formatDaysOverdue, formatDueMonth } from './leaseFeeFormat'
import { openNoticeLetters } from './leaseFeeLetter'
import RecordLeaseFeeNoticePanel from './RecordLeaseFeeNoticePanel'
import RecordLeaseFeePaymentPanel from './RecordLeaseFeePaymentPanel'
import VoidLeaseFeePaymentPanel from './VoidLeaseFeePaymentPanel'

/** The lease's monthly fee, what it owes today, the payments the trust has recorded, and its late notices. */
export default function LeaseFeesSection({ lease }: { lease: GroundLeaseResponse }) {
  const [recording, setRecording] = useState(false)
  const [recordingNotice, setRecordingNotice] = useState(false)
  const [voiding, setVoiding] = useState<LeaseFeePaymentDto | null>(null)
  const fees = lease.leaseFees
  if (!fees) return null

  const statement = fees.statement
  const monthlyFee = fees.monthlyFee ?? 0
  const balance = statement?.balance ?? 0
  const payments = fees.payments ?? []
  const notices = fees.notices ?? []
  const noticeDue = fees.noticeDue

  return (
    <div className="detail-section">
      <div className="detail-section__title">Lease Fees</div>
      {monthlyFee === 0 && payments.length === 0 ? (
        <p style={{ fontSize: 13, color: 'var(--text-dim)' }}>No lease fee.</p>
      ) : (
        <>
          <div className="detail-grid" style={{ marginBottom: 12 }}>
            <div className="detail-field">
              <div className="detail-field__label">Monthly Fee</div>
              <div className="detail-field__value mono">
                {formatPrice(monthlyFee)}
                <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                  {formatPrice(fees.landUseFee ?? 0)} land use + {formatPrice(fees.repairReserveFee ?? 0)} repair reserve
                </div>
              </div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Balance</div>
              <div className="detail-field__value mono" style={balance > 0 ? { color: 'var(--red)' } : undefined}>
                {formatBalance(balance)}
              </div>
            </div>
            {statement?.oldestUnpaidDueDate && (
              <>
                <div className="detail-field">
                  <div className="detail-field__label">Oldest Unpaid</div>
                  <div className="detail-field__value">{formatDueMonth(statement.oldestUnpaidDueDate)}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Days Overdue</div>
                  <div className="detail-field__value mono">{formatDaysOverdue(statement.daysOverdue ?? 0)}</div>
                </div>
              </>
            )}
          </div>

          {noticeDue && (
            <div
              role="status"
              style={{
                display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap', marginBottom: 12,
                padding: '8px 12px', borderRadius: 6, background: 'var(--warning-surface)',
              }}
            >
              <strong style={{ color: 'var(--warning)' }}>{noticeDue === 'Second' ? 'Second notice due' : 'First notice due'}</strong>
              <button type="button" className="btn btn-secondary btn-sm" onClick={() => openNoticeLetters(lease.id!)}>
                Print Letter
              </button>
              <button type="button" className="btn btn-secondary btn-sm" onClick={() => setRecordingNotice(true)}>
                Record Notice
              </button>
            </div>
          )}

          <div style={{ display: 'flex', gap: 8, marginBottom: 12 }}>
            <button type="button" className="btn btn-secondary" onClick={() => setRecording(true)}>
              Record Payment
            </button>
            {!noticeDue && (statement?.balance ?? 0) > 0 && (
              <button type="button" className="btn btn-ghost" onClick={() => setRecordingNotice(true)}>
                Record Notice
              </button>
            )}
          </div>

          {payments.length === 0 ? (
            <p style={{ fontSize: 13, color: 'var(--text-dim)' }}>No payments recorded.</p>
          ) : (
            <div className="registry-table-wrap">
              <table className="registry-table" aria-label="Lease fee payments">
                <thead>
                  <tr>
                    <th>Received</th>
                    <th>Amount</th>
                    <th>Method</th>
                    <th>Reference</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  {payments.map(p => (
                    <tr key={p.paymentId} style={p.voidReason ? { color: 'var(--text-dim)' } : undefined}>
                      <td className="cell-mono">{p.receivedDate ? formatCalendarDate(p.receivedDate) : '—'}</td>
                      <td className="cell-mono" style={p.voidReason ? { textDecoration: 'line-through' } : undefined}>
                        {formatPrice(p.amount ?? 0)}
                      </td>
                      <td>{PAYMENT_METHOD_LABELS[p.method ?? ''] ?? p.method}</td>
                      <td>{p.reference ?? '—'}</td>
                      <td>
                        {p.voidReason ? (
                          <span title={p.voidReason}>Voided: {p.voidReason}</span>
                        ) : (
                          <button
                            type="button"
                            className="btn btn-ghost btn-sm"
                            style={{ color: 'var(--red)' }}
                            onClick={() => setVoiding(p)}
                          >
                            Void
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {notices.length > 0 && (
            <div className="registry-table-wrap" style={{ marginTop: 12 }}>
              <div className="detail-field__label" style={{ marginBottom: 4 }}>Notices Sent</div>
              <table className="registry-table" aria-label="Lease fee notices">
                <thead>
                  <tr>
                    <th>Sent</th>
                    <th>Notice</th>
                    <th>Delivery</th>
                    <th>Balance Then</th>
                  </tr>
                </thead>
                <tbody>
                  {notices.map(n => (
                    <tr key={n.noticeId}>
                      <td className="cell-mono">{n.sentOn ? formatCalendarDate(n.sentOn) : '—'}</td>
                      <td>{NOTICE_LEVEL_LABELS[n.level ?? ''] ?? n.level}</td>
                      <td>{NOTICE_DELIVERY_LABELS[n.delivery ?? ''] ?? n.delivery}</td>
                      <td className="cell-mono">
                        {formatPrice(n.balanceDue ?? 0)}
                        {n.oldestUnpaidDueDate && (
                          <span style={{ color: 'var(--text-dim)' }}> since {formatDueMonth(n.oldestUnpaidDueDate)}</span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      {recordingNotice && (
        <RecordLeaseFeeNoticePanel lease={lease} onClose={() => setRecordingNotice(false)} />
      )}
      {recording && (
        <RecordLeaseFeePaymentPanel lease={lease} onClose={() => setRecording(false)} />
      )}
      {voiding && (
        <VoidLeaseFeePaymentPanel
          groundLeaseId={lease.id!}
          payment={voiding}
          onClose={() => setVoiding(null)}
        />
      )}
    </div>
  )
}
