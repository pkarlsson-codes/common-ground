import { useState } from 'react'
import SlideOver from '../../components/SlideOver'
import NavIdLink from '../../components/NavIdLink'
import StatusBadge from '../../components/StatusBadge'
import { useHomeownerApplication } from '../../hooks/applicantSelection'
import ApproveEligibilityPanel from './ApproveEligibilityPanel'
import DenyEligibilityPanel from './DenyEligibilityPanel'
import RecordHousingCounselingPanel from './RecordHousingCounselingPanel'
import RecordCltOrientationPanel from './RecordCltOrientationPanel'
import RecordMortgagePrequalificationPanel from './RecordMortgagePrequalificationPanel'
import SelectApplicantPanel from './SelectApplicantPanel'
import DenyApplicationPanel from './DenyApplicationPanel'
import WithdrawApplicationPanel from './WithdrawApplicationPanel'
import { getResponseStatus } from '../../lib/getErrorMessage'

type Props = {
  applicationId: string
  onClose: () => void
}

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtDateTime = (value?: string | null) => {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString()
}

const fmtDate = (value?: string | null) => {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleDateString()
}

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 8 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

export default function ApplicationDetailPanel({ applicationId, onClose }: Props) {
  const { data: detail, isLoading, isError, error, refetch } = useHomeownerApplication(applicationId)
  const [actionPanel, setActionPanel] = useState<
    | 'approve-eligibility'
    | 'deny-eligibility'
    | 'record-housing-counseling'
    | 'record-clt-orientation'
    | 'record-mortgage-prequalification'
    | 'select-applicant'
    | 'deny-application'
    | 'withdraw-application'
    | null
  >(null)

  const phase = (detail?.phase ?? '').toLowerCase()
  const status = getResponseStatus(error)
  const isEligibilityPhase = phase === 'submitted'
  const isActivePhase = phase === 'active'
  const isWaitlistedPhase = phase === 'waitlisted'
  const isSelectedOrCompleted = phase === 'selected'
  const isClosed = phase === 'denied' || phase === 'eligibilitydenied' || phase === 'withdrawn'

  return (
    <SlideOver
      title={detail?.applicantFullName ?? 'Application'}
      subtitle={detail?.applicantEmail ?? detail?.applicantPhone ?? undefined}
      onClose={onClose}
      footer={<button className="btn btn-ghost" onClick={onClose}>Close</button>}
    >
      {isLoading && <SkeletonRows />}

      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load application</div>
          <div className="error-state__sub">
            {status === 404
              ? 'This application no longer exists or is inaccessible.'
              : status === 409
                ? 'A conflict occurred while loading this application.'
                : 'Try again or close and reopen this panel.'}
          </div>
          <div style={{ marginTop: 10 }}>
            <button className="btn btn-ghost btn-sm" type="button" onClick={() => { void refetch() }}>Retry</button>
          </div>
        </div>
      )}

      {!isLoading && !isError && !detail && (
        <div className="empty-state">
          <div className="empty-state__title">Application not found</div>
          <div className="empty-state__sub">This record may have been removed.</div>
        </div>
      )}

      {!isLoading && !isError && detail && (
        <>
          <div className="detail-grid">
            <div className="detail-field">
              <div className="detail-field__label">Phase</div>
              <div className="detail-field__value"><StatusBadge status={detail.phase ?? 'Unknown'} /></div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Submitted At</div>
              <div className="detail-field__value mono">{fmtDateTime(detail.submittedAt)}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Submitted By</div>
              <div className="detail-field__value">{detail.submittedBy ?? '—'}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Phone</div>
              <div className="detail-field__value">{detail.applicantPhone ?? '—'}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Household Size</div>
              <div className="detail-field__value mono">{detail.householdSize ?? '—'}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Annual Income</div>
              <div className="detail-field__value mono">
                {typeof detail.annualIncome === 'number' ? fmtCurrency(detail.annualIncome) : '—'}
              </div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Accessibility Required</div>
              <div className="detail-field__value">{detail.accessibilityRequired ? 'Yes' : 'No'}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Eligibility Approved Date</div>
              <div className="detail-field__value mono">{fmtDateTime(detail.eligibilityApprovedDate)}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Selected Date</div>
              <div className="detail-field__value mono">{fmtDateTime(detail.selectedDate)}</div>
            </div>
            <div className="detail-field full-span">
              <div className="detail-field__label">Denial Reason</div>
              <div className="detail-field__value">{detail.denialReason ?? '—'}</div>
            </div>
            <div className="detail-field full-span">
              <div className="detail-field__label">Withdrawal Reason</div>
              <div className="detail-field__value">{detail.withdrawalReason ?? '—'}</div>
            </div>
            {detail.selectedLandParcelId && (
              <div className="detail-field full-span">
                <div className="detail-field__value">
                  <NavIdLink id={detail.selectedLandParcelId} to="/land-parcels" label="Open Selected Land Parcel" />
                </div>
              </div>
            )}
            {detail.selectedHomeId && (
              <div className="detail-field full-span">
                <div className="detail-field__value">
                  <NavIdLink id={detail.selectedHomeId} to="/homes" label="Open Selected Home" />
                </div>
              </div>
            )}
          </div>

          {!isEligibilityPhase && (
            <div className="detail-section" style={{ marginTop: 16 }}>
              <div className="detail-section__title">Buyer Preparation Checklist</div>
              <div className="checklist">
                <div className={`checklist-item${detail.isHousingCounselingCompleted ? ' checklist-item--done' : ''}`}>
                  <div className={`checklist-item__icon${detail.isHousingCounselingCompleted ? ' checklist-item__icon--done' : ' checklist-item__icon--pending'}`}>
                    {detail.isHousingCounselingCompleted ? '✓' : '○'}
                  </div>
                  <span className="checklist-item__label">Housing Counseling</span>
                  {detail.isHousingCounselingCompleted && (
                    <span className="checklist-item__status">Completed</span>
                  )}
                  {!detail.isHousingCounselingCompleted && (isActivePhase || isWaitlistedPhase) && (
                    <button className="btn btn-secondary btn-sm" onClick={() => setActionPanel('record-housing-counseling')}>
                      Record
                    </button>
                  )}
                </div>

                <div className={`checklist-item${detail.isCltOrientationCompleted ? ' checklist-item--done' : ''}`}>
                  <div className={`checklist-item__icon${detail.isCltOrientationCompleted ? ' checklist-item__icon--done' : ' checklist-item__icon--pending'}`}>
                    {detail.isCltOrientationCompleted ? '✓' : '○'}
                  </div>
                  <span className="checklist-item__label">CLT Orientation</span>
                  {detail.isCltOrientationCompleted && (
                    <span className="checklist-item__status">Completed</span>
                  )}
                  {!detail.isCltOrientationCompleted && (isActivePhase || isWaitlistedPhase) && (
                    <button className="btn btn-secondary btn-sm" onClick={() => setActionPanel('record-clt-orientation')}>
                      Record
                    </button>
                  )}
                </div>

                <div className={`checklist-item${detail.mortgagePrequalification ? ' checklist-item--done' : ''}`}>
                  <div className={`checklist-item__icon${detail.mortgagePrequalification ? ' checklist-item__icon--done' : ' checklist-item__icon--pending'}`}>
                    {detail.mortgagePrequalification ? '✓' : '○'}
                  </div>
                  <span className="checklist-item__label">Mortgage Prequalification</span>
                  {detail.mortgagePrequalification && (
                    <span className="checklist-item__status">
                      {detail.mortgagePrequalification.lenderName} · expires {fmtDate(detail.mortgagePrequalification.expiryDate)}
                    </span>
                  )}
                  {(isActivePhase || isWaitlistedPhase) && (
                    <button className="btn btn-secondary btn-sm" onClick={() => setActionPanel('record-mortgage-prequalification')}>
                      {detail.mortgagePrequalification ? 'Update' : 'Record'}
                    </button>
                  )}
                </div>
              </div>
            </div>
          )}

          {detail.mortgagePrequalification && !isEligibilityPhase && (
            <div className="detail-section" style={{ marginTop: 16 }}>
              <div className="detail-section__title">Mortgage Prequalification</div>
              <div className="detail-grid" style={{ marginTop: 10 }}>
                <div className="detail-field">
                  <div className="detail-field__label">Lender Name</div>
                  <div className="detail-field__value">{detail.mortgagePrequalification.lenderName ?? '—'}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Prequalified Amount</div>
                  <div className="detail-field__value mono">
                    {typeof detail.mortgagePrequalification.approvedAmount === 'number'
                      ? fmtCurrency(detail.mortgagePrequalification.approvedAmount)
                      : '—'}
                  </div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Prequalification Expiry</div>
                  <div className="detail-field__value mono">
                    {fmtDate(detail.mortgagePrequalification.expiryDate)}
                  </div>
                </div>
              </div>
            </div>
          )}

          <div className="detail-section" style={{ marginTop: 16 }}>
            <div className="detail-section__title">Workflow Actions</div>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
              {isEligibilityPhase && (
                <>
                  <button className="btn btn-secondary btn-sm" onClick={() => setActionPanel('approve-eligibility')}>
                    Approve Eligibility
                  </button>
                  <button className="btn btn-ghost btn-sm" onClick={() => setActionPanel('deny-eligibility')}>
                    Deny Eligibility
                  </button>
                </>
              )}

              {(isActivePhase || isWaitlistedPhase) && (
                <>
                  {isWaitlistedPhase && (
                    <button className="btn btn-primary btn-sm" onClick={() => setActionPanel('select-applicant')}>
                      Select Applicant
                    </button>
                  )}
                  <button className="btn btn-ghost btn-sm" onClick={() => setActionPanel('deny-application')}>
                    Deny Application
                  </button>
                  <button className="btn btn-ghost btn-sm" onClick={() => setActionPanel('withdraw-application')}>
                    Withdraw Application
                  </button>
                </>
              )}

              {(isClosed || isSelectedOrCompleted) && (
                <div className="detail-field__value">No actions available for this phase.</div>
              )}
            </div>
          </div>
        </>
      )}

      {actionPanel === 'approve-eligibility' && (
        <ApproveEligibilityPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'deny-eligibility' && (
        <DenyEligibilityPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'record-housing-counseling' && (
        <RecordHousingCounselingPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'record-clt-orientation' && (
        <RecordCltOrientationPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'record-mortgage-prequalification' && (
        <RecordMortgagePrequalificationPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'select-applicant' && (
        <SelectApplicantPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'deny-application' && (
        <DenyApplicationPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'withdraw-application' && (
        <WithdrawApplicationPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
    </SlideOver>
  )
}
