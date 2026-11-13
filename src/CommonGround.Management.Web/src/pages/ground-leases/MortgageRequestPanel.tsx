import { useState, type ReactNode } from 'react'
import SlideOver from '../../components/SlideOver'
import {
  MORTGAGE_REQUEST_NOTICE_BUSINESS_DAYS,
  MORTGAGE_REQUEST_STATUS_LABELS,
  useGroundLease,
  useMortgageRequest,
} from '../../hooks/groundLeases'
import { formatCalendarDate, longDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import AddMortgageAppraisalPanel from './AddMortgageAppraisalPanel'
import ApproveMortgageRequestPanel from './ApproveMortgageRequestPanel'
import MortgageDebtCheck from './MortgageDebtCheck'
import MortgageRequestReasonPanel from './MortgageRequestReasonPanel'
import RecordMortgageLoanClosedPanel from './RecordMortgageLoanClosedPanel'
import { loanTermsText, mortgageRequestTitle, openMortgageRequestLetter } from './mortgageRequestLetter'

interface Props {
  mortgageRequestId: string
  onClose: () => void
}

type Action = 'appraisal' | 'approve' | 'deny' | 'withdraw' | 'closed'

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="detail-field">
      <div className="detail-field__label">{label}</div>
      <div className="detail-field__value">{children}</div>
    </div>
  )
}

/** A homeowner's request for a new loan on the home: its terms, the debt check, and the trust's decision. */
export default function MortgageRequestPanel({ mortgageRequestId, onClose }: Props) {
  const { data: request, isLoading, isError } = useMortgageRequest(mortgageRequestId)
  const { data: lease } = useGroundLease(request?.groundLeaseId ?? null)
  const [action, setAction] = useState<Action | null>(null)
  const closeAction = () => setAction(null)

  const status = request?.status ?? ''
  const pending = status === 'Pending'
  const approved = status === 'Approved'
  // A withdrawn approval keeps its decision on view, but its letter no longer applies.
  const letterApplies = status === 'Approved' || status === 'Denied' || status === 'Closed'
  const hasAppraisal = request?.purchaseOptionPrice != null
  const indexBased = lease?.formulaKind === 'IndexBased'
  const paidOffLenders = (request?.paidOffMortgageIds ?? [])
    .map(id => lease?.mortgages?.permitted?.find(m => m.mortgageId === id))
    .filter(m => !!m)

  return (
    <SlideOver
      title={request ? mortgageRequestTitle(request) : 'Mortgage Request'}
      subtitle={request ? `${request.homeownerFullName ?? 'Homeowner'} · ${MORTGAGE_REQUEST_STATUS_LABELS[status] ?? status}` : undefined}
      onClose={onClose}
    >
      {isLoading && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {isError && <div className="form-error" role="alert">Could not load the request.</div>}
      {request && (
        <>
          <div className="detail-section">
            <div className="detail-section__title">Loan</div>
            <div className="detail-grid">
              <Field label="Received On"><span className="cell-mono">{formatCalendarDate(request.receivedOn!)}</span></Field>
              <Field label="Expected Closing">
                <span className="cell-mono">{formatCalendarDate(request.expectedClosingOn!)}</span>
                <div style={{ fontSize: 12, color: request.shortNotice ? 'var(--warning)' : 'var(--text-secondary)' }}>
                  {request.businessDaysBeforeClosing} business days after receipt
                  {request.shortNotice && ` (the lease asks for ${MORTGAGE_REQUEST_NOTICE_BUSINESS_DAYS})`}
                </div>
              </Field>
              <Field label="Principal"><span className="cell-mono">{formatPrice(request.principal ?? 0)}</span></Field>
              <Field label="Total Debt After Closing"><span className="cell-mono">{formatPrice(request.totalDebtAfterClosing ?? 0)}</span></Field>
              <Field label="Rate and Term">{loanTermsText(request)}</Field>
              <Field label="Closing Costs">
                {request.closingCosts != null ? <span className="cell-mono">{formatPrice(request.closingCosts)}</span> : '—'}
              </Field>
              <Field label="Appraisal">
                {request.appraisedValue != null && request.appraisalDate
                  ? <>{formatPrice(request.appraisedValue)} on {formatCalendarDate(request.appraisalDate, longDate)}</>
                  : 'Not received'}
              </Field>
            </div>
          </div>

          <div className="detail-section">
            <div className="detail-section__title">Debt Check</div>
            {hasAppraisal ? (
              <MortgageDebtCheck request={request} />
            ) : (
              <div style={{ display: 'flex', gap: 12, alignItems: 'center' }}>
                <span style={{ fontSize: 13, color: 'var(--text-secondary)' }}>The debt check needs the loan's appraisal.</span>
                {pending && (
                  <button type="button" className="btn btn-secondary btn-sm" onClick={() => setAction('appraisal')} disabled={!lease}>
                    Add Appraisal
                  </button>
                )}
              </div>
            )}
          </div>

          {pending && (
            <div style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap' }}>
              <button type="button" className="btn btn-primary" onClick={() => setAction('approve')} disabled={!hasAppraisal}>
                Approve
              </button>
              <button type="button" className="btn btn-secondary" onClick={() => setAction('deny')}>Deny</button>
              <button type="button" className="btn btn-ghost" onClick={() => setAction('withdraw')}>Withdraw</button>
              {!hasAppraisal && <span style={{ fontSize: 12, color: 'var(--text-secondary)' }}>Add the loan's appraisal first.</span>}
            </div>
          )}

          {request.decidedOn && (
            <div className="detail-section">
              <div className="detail-section__title">Decision</div>
              <div className="detail-grid">
                <Field label={status === 'Denied' ? 'Denied On' : 'Approved On'}>
                  <span className="cell-mono">{formatCalendarDate(request.decidedOn)}</span>
                </Field>
                {request.decisionReason && <Field label="Reason">{request.decisionReason}</Field>}
              </div>
              <div style={{ display: 'flex', gap: 8, marginTop: 12 }}>
                {letterApplies && (
                  <button type="button" className="btn btn-secondary" onClick={() => openMortgageRequestLetter(request.id!)}>
                    Print Letter
                  </button>
                )}
                {approved && (
                  <>
                    <button type="button" className="btn btn-primary" onClick={() => setAction('closed')} disabled={!lease}>
                      Record Loan Closed
                    </button>
                    <button type="button" className="btn btn-ghost" onClick={() => setAction('withdraw')}>Withdraw</button>
                  </>
                )}
              </div>
            </div>
          )}

          {status === 'Closed' && request.closedOn && (
            <div className="detail-section">
              <div className="detail-section__title">Closing</div>
              <div className="detail-grid">
                <Field label="Closed On"><span className="cell-mono">{formatCalendarDate(request.closedOn)}</span></Field>
                <Field label="Paid Off">
                  {paidOffLenders.length === 0
                    ? 'None'
                    : paidOffLenders.map(m => <div key={m.mortgageId}>{m.lender} · {formatPrice(m.originalPrincipal ?? 0)}</div>)}
                </Field>
              </div>
            </div>
          )}

          {status === 'Withdrawn' && request.withdrawnOn && (
            <div className="detail-section">
              <div className="detail-section__title">Withdrawn</div>
              <div className="detail-grid">
                <Field label="Withdrawn On"><span className="cell-mono">{formatCalendarDate(request.withdrawnOn)}</span></Field>
                {request.withdrawnReason && <Field label="Reason">{request.withdrawnReason}</Field>}
              </div>
            </div>
          )}

          {action === 'appraisal' && <AddMortgageAppraisalPanel request={request} indexBased={indexBased} onClose={closeAction} />}
          {action === 'approve' && <ApproveMortgageRequestPanel request={request} onClose={closeAction} />}
          {(action === 'deny' || action === 'withdraw') && (
            <MortgageRequestReasonPanel request={request} action={action} onClose={closeAction} />
          )}
          {action === 'closed' && lease && <RecordMortgageLoanClosedPanel request={request} lease={lease} onClose={closeAction} />}
        </>
      )}
    </SlideOver>
  )
}
