import { useState } from 'react'
import type { GroundLeaseResponse, LeaseFeePaymentDto } from '../../api'
import { NOTICE_DELIVERY_LABELS, NOTICE_LEVEL_LABELS, PAYMENT_METHOD_LABELS } from '../../hooks/groundLeases'
import { formatCalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { formatBalance, formatDaysOverdue, formatDueMonth } from './leaseFeeFormat'
import { lateInterestText } from './lateInterest'
import { openNoticeLetters } from './leaseFeeLetter'
import RecordLeaseFeeNoticePanel from './RecordLeaseFeeNoticePanel'
import RecordLeaseFeePaymentPanel from './RecordLeaseFeePaymentPanel'
import VoidLeaseFeePaymentPanel from './VoidLeaseFeePaymentPanel'
import WaiveLeaseFeeInterestPanel from './WaiveLeaseFeeInterestPanel'

/** The lease's monthly fee, what it owes today with late interest, the payments the trust has recorded, and its late notices. */
export default function LeaseFeesSection({ lease }: { lease: GroundLeaseResponse }) {
  const [recording, setRecording] = useState(false)
  const [recordingNotice, setRecordingNotice] = useState(false)
  const [voiding, setVoiding] = useState<LeaseFeePaymentDto | null>(null)
  const [waiving, setWaiving] = useState(false)
  const fees = lease.leaseFees
  if (!fees) return null

  const statement = fees.statement
  const monthlyFee = fees.monthlyFee ?? 0
  const balance = statement?.balance ?? 0
  const payments = fees.payments ?? []
  const notices = fees.notices ?? []
  const noticeDue = fees.noticeDue
  const chargesInterest = (fees.lateInterestRate ?? 0) > 0
  const interestDue = statement?.interestDue ?? 0
  const interestAccruing = statement?.interestAccruing ?? 0
  const interestWaivers = fees.interestWaivers ?? []

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
              <div className="detail-field__label">Late Interest</div>
              <div className="detail-field__value">{lateInterestText(fees.lateInterestRate, fees.interestWaiverDays)}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Balance</div>
              <div className="detail-field__value mono" style={balance > 0 ? { color: 'var(--red)' } : undefined}>
                {formatBalance(balance)}
                {interestDue > 0 && (
                  <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>includes {formatPrice(interestDue)} interest</div>
                )}
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

          {chargesInterest && interestAccruing > 0 && (
            <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 12 }}>
              Interest accruing: {formatPrice(interestAccruing)}
              {statement?.interestWaivedIfPaidBy && statement.oldestUnpaidDueDate && (
                <>, waived if the {formatDueMonth(statement.oldestUnpaidDueDate)} fee is paid by {formatCalendarDate(statement.interestWaivedIfPaidBy)}</>
              )}
              . Not in the balance yet.
            </p>
          )}

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
            {interestDue > 0 && (
              <button type="button" className="btn btn-ghost" onClick={() => setWaiving(true)}>
                Waive Interest
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

          {interestWaivers.length > 0 && (
            <div className="registry-table-wrap" style={{ marginTop: 12 }}>
              <div className="detail-field__label" style={{ marginBottom: 4 }}>Interest Waived</div>
              <table className="registry-table" aria-label="Interest waivers">
                <thead>
                  <tr>
                    <th>Waived</th>
                    <th>Amount</th>
                    <th>Reason</th>
                  </tr>
                </thead>
                <tbody>
                  {interestWaivers.map(w => (
                    <tr key={w.waiverId}>
                      <td className="cell-mono">{w.waivedOn ? formatCalendarDate(w.waivedOn) : '—'}</td>
                      <td className="cell-mono">{formatPrice(w.amount ?? 0)}</td>
                      <td>{w.reason}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}

      {waiving && (
        <WaiveLeaseFeeInterestPanel lease={lease} onClose={() => setWaiving(false)} />
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
