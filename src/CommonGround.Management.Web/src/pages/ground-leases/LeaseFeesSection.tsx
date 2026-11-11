import { useState } from 'react'
import type { DefaultCureDto, GroundLeaseResponse, LeaseFeePaymentDto } from '../../api'
import { NOTICE_DELIVERY_LABELS, NOTICE_LEVEL_LABELS, PAYMENT_METHOD_LABELS } from '../../hooks/groundLeases'
import { formatCalendarDate, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { formatBalance, formatDaysOverdue, formatDueMonth } from './leaseFeeFormat'
import { lateInterestText } from './lateInterest'
import { openNoticeLetters } from './leaseFeeLetter'
import RecordLeaseFeeNoticePanel from './RecordLeaseFeeNoticePanel'
import RecordLeaseFeePaymentPanel from './RecordLeaseFeePaymentPanel'
import VoidLeaseFeePaymentPanel from './VoidLeaseFeePaymentPanel'
import WaiveLeaseFeeInterestPanel from './WaiveLeaseFeeInterestPanel'

const longDate = (d?: CalendarDate | null) => (d ? formatCalendarDate(d, { year: 'numeric', month: 'long', day: 'numeric' }) : '—')

/** Where the lease's last notice of default stands (Art 12.1): the cure deadline, what is left to pay, or how it ended. */
function DefaultCureStatus({ cure }: { cure: DefaultCureDto }) {
  const sent = longDate(cure.sentOn)
  const owed = formatPrice(cure.amountOwed ?? 0)
  const left = formatPrice(cure.stillToPay ?? 0)
  const extended = cure.extended
    ? ` Extended from ${longDate(cure.originalDeadline)}: two-thirds was paid in time.`
    : ''
  const uncured = cure.status === 'Uncured'
  const text = {
    Curing: `Notice of default sent ${sent} for ${owed}. Cure by ${longDate(cure.cureBy)}: ${left} still to pay.${extended}`,
    Cured: `Notice of default of ${sent} cured: ${owed} paid by ${longDate(cure.paidInFullOn)}.`,
    PaidAfterDeadline: `Notice of default of ${sent}: ${owed} paid on ${longDate(cure.paidInFullOn)}, after the cure deadline of ${longDate(cure.cureBy)}.`,
    Uncured: `Notice of default of ${sent} not cured by ${longDate(cure.cureBy)}: ${left} of ${owed} still to pay. The trust may end the lease or agree a plan with the homeowner.${extended}`,
  }[cure.status ?? ''] ?? ''
  const active = cure.status === 'Curing' || uncured

  return (
    <div
      role="status"
      aria-label="Notice of default"
      style={{
        marginBottom: 12, padding: '8px 12px', borderRadius: 6, fontSize: 13,
        background: active ? (uncured ? 'var(--danger-surface)' : 'var(--warning-surface)') : 'var(--surface-2)',
        color: uncured ? 'var(--red)' : undefined,
      }}
    >
      {text}
    </div>
  )
}

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
  const openingBalance = fees.openingBalance ?? 0
  const trackedLater = !!fees.feesTrackedFrom && !!lease.leaseStartDate && fees.feesTrackedFrom > lease.leaseStartDate

  return (
    <div className="detail-section">
      <div className="detail-section__title">Lease Fees</div>
      {monthlyFee === 0 && openingBalance === 0 && payments.length === 0 ? (
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
            {trackedLater && (
              <div className="detail-field">
                <div className="detail-field__label">Fees Tracked From</div>
                <div className="detail-field__value">
                  {longDate(fees.feesTrackedFrom)}
                  {openingBalance > 0 && <span className="mono">, {formatPrice(openingBalance)} owed then</span>}
                </div>
              </div>
            )}
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

          {fees.defaultCure && <DefaultCureStatus cure={fees.defaultCure} />}

          {noticeDue && (
            <div
              role="status"
              style={{
                display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap', marginBottom: 12,
                padding: '8px 12px', borderRadius: 6, background: 'var(--warning-surface)',
              }}
            >
              <strong style={{ color: 'var(--warning)' }}>{NOTICE_LEVEL_LABELS[noticeDue] ?? noticeDue} due</strong>
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
