import { useState, type ReactNode } from 'react'
import SlideOver from '../../components/SlideOver'
import { downloadDocument, useEntityDocuments } from '../../hooks/documents'
import {
  IMPROVEMENT_REPLY_DAYS,
  IMPROVEMENT_REQUEST_STATUS_LABELS,
  useGroundLease,
  useImprovementRequest,
} from '../../hooks/groundLeases'
import { daysBetween, formatCalendarDate, trustToday } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import ApproveImprovementRequestPanel from './ApproveImprovementRequestPanel'
import DenyImprovementRequestPanel from './DenyImprovementRequestPanel'
import RecordImprovementCompletedPanel from './RecordImprovementCompletedPanel'
import WithdrawImprovementRequestPanel from './WithdrawImprovementRequestPanel'
import { improvementLetterApplies, openImprovementRequestLetter } from './improvementRequestLetter'

interface Props {
  improvementRequestId: string
  onClose: () => void
}

type Action = 'approve' | 'deny' | 'complete' | 'withdraw'

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="detail-field">
      <div className="detail-field__label">{label}</div>
      <div className="detail-field__value">{children}</div>
    </div>
  )
}

const money = (n: number | null | undefined) => (n == null ? '—' : <span className="cell-mono">{formatPrice(n)}</span>)

/** A homeowner's request to alter or add to the home: what was asked, the plans, and the trust's decision. */
export default function ImprovementRequestPanel({ improvementRequestId, onClose }: Props) {
  const { data: request, isLoading, isError } = useImprovementRequest(improvementRequestId)
  const { data: lease } = useGroundLease(request?.groundLeaseId ?? null)
  const { data: documents } = useEntityDocuments('ImprovementRequest', improvementRequestId)
  const [action, setAction] = useState<Action | null>(null)
  const closeAction = () => setAction(null)

  const status = request?.status ?? ''
  const waiting = status === 'Waiting for decision'
  const approved = status === 'Approved'
  const receipts = (documents?.items ?? []).filter(d => d.role === 'improvement-receipt')
  const plans = (documents?.items ?? []).filter(d => d.role === 'improvement-plans')
  const lateDays = request?.replyDueOn && waiting ? daysBetween(request.replyDueOn, trustToday()) : 0

  return (
    <SlideOver
      title={request?.description ?? 'Improvement Request'}
      subtitle={request ? `${request.homeownerFullName ?? 'Homeowner'} · ${IMPROVEMENT_REQUEST_STATUS_LABELS[status] ?? status}` : undefined}
      onClose={onClose}
    >
      {isLoading && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {isError && <div className="form-error" role="alert">Could not load the request.</div>}
      {request && (
        <>
          <div className="detail-section">
            <div className="detail-section__title">Request</div>
            <div className="detail-grid">
              <Field label="Received On"><span className="cell-mono">{formatCalendarDate(request.receivedOn!)}</span></Field>
              <Field label="Reply Due">
                <span className="cell-mono">{formatCalendarDate(request.replyDueOn!)}</span>
                <div style={{ fontSize: 12, color: lateDays > 0 ? 'var(--danger)' : 'var(--text-secondary)' }}>
                  {lateDays > 0
                    ? `Reply overdue by ${lateDays} ${lateDays === 1 ? 'day' : 'days'}`
                    : `${IMPROVEMENT_REPLY_DAYS} days after receipt`}
                </div>
              </Field>
              <Field label="Estimated Cost">{money(request.estimatedCost)}</Field>
              <Field label="Plans">
                {plans.length === 0
                  ? 'None'
                  : plans.map(p => (
                    <div key={p.documentId}>
                      <button type="button" className="btn btn-ghost btn-sm" onClick={() => downloadDocument(p.documentId!)}>
                        {p.currentFileName ?? p.title}
                      </button>
                    </div>
                  ))}
              </Field>
            </div>
          </div>

          {waiting && (
            <div style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap' }}>
              <button type="button" className="btn btn-primary" onClick={() => setAction('approve')} disabled={!lease}>
                Approve
              </button>
              <button type="button" className="btn btn-secondary" onClick={() => setAction('deny')}>Deny</button>
              <button type="button" className="btn btn-ghost" onClick={() => setAction('withdraw')}>Withdraw</button>
            </div>
          )}

          {approved && (
            <div style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap' }}>
              <button type="button" className="btn btn-primary" onClick={() => setAction('complete')}>Record Completed</button>
              <button type="button" className="btn btn-ghost" onClick={() => setAction('withdraw')}>Withdraw</button>
            </div>
          )}

          {request.approvedOn && (
            <div className="detail-section">
              <div className="detail-section__title">Approval</div>
              <div className="detail-grid">
                <Field label="Approved On"><span className="cell-mono">{formatCalendarDate(request.approvedOn)}</span></Field>
                <Field label="Pre-Approved Credit">{money(request.preApprovedCredit)}</Field>
                <Field label="Improvements Cap When Approved">
                  {request.leaseImprovementsCap == null ? 'No cap' : money(request.leaseImprovementsCap)}
                </Field>
                <Field label="Credit on the Lease Then">{money(request.creditOnLeaseBefore)}</Field>
                {request.approvalNotes && <Field label="Notes">{request.approvalNotes}</Field>}
              </div>
            </div>
          )}

          {request.deniedOn && (
            <div className="detail-section">
              <div className="detail-section__title">Denial</div>
              <div className="detail-grid">
                <Field label="Denied On"><span className="cell-mono">{formatCalendarDate(request.deniedOn)}</span></Field>
                {request.denialReason && <Field label="Reason">{request.denialReason}</Field>}
              </div>
            </div>
          )}

          {request.completedOn && (
            <div className="detail-section">
              <div className="detail-section__title">Completed Work</div>
              <div className="detail-grid">
                <Field label="Completed On"><span className="cell-mono">{formatCalendarDate(request.completedOn)}</span></Field>
                <Field label="Actual Cost">{money(request.actualCost)}</Field>
                <Field label="Receipts">
                  {receipts.length === 0
                    ? 'None'
                    : receipts.map(r => (
                      <div key={r.documentId}>
                        <button type="button" className="btn btn-ghost btn-sm" onClick={() => downloadDocument(r.documentId!)}>
                          {r.currentFileName ?? r.title}
                        </button>
                      </div>
                    ))}
                </Field>
                <Field label="Confirmed Credit">{money(request.confirmedCredit)}</Field>
                {request.completionNotes && <Field label="Notes">{request.completionNotes}</Field>}
              </div>
            </div>
          )}

          {request.withdrawnOn && (
            <div className="detail-section">
              <div className="detail-section__title">Withdrawn</div>
              <div className="detail-grid">
                <Field label="Withdrawn On"><span className="cell-mono">{formatCalendarDate(request.withdrawnOn)}</span></Field>
                {request.withdrawnReason && <Field label="Reason">{request.withdrawnReason}</Field>}
              </div>
            </div>
          )}

          {improvementLetterApplies(status) && (
            <div style={{ marginTop: 12 }}>
              <button type="button" className="btn btn-secondary" onClick={() => openImprovementRequestLetter(request.id!)}>
                Print Letter
              </button>
            </div>
          )}

          {action === 'approve' && lease && <ApproveImprovementRequestPanel request={request} lease={lease} onClose={closeAction} />}
          {action === 'complete' && <RecordImprovementCompletedPanel request={request} onClose={closeAction} />}
          {action === 'withdraw' && <WithdrawImprovementRequestPanel request={request} onClose={closeAction} />}
          {action === 'deny' && <DenyImprovementRequestPanel request={request} onClose={closeAction} />}
        </>
      )}
    </SlideOver>
  )
}
