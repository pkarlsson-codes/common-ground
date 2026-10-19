import { useState } from 'react'
import SlideOver from '../../components/SlideOver'
import NavIdLink from '../../components/NavIdLink'
import StatusBadge from '../../components/StatusBadge'
import { useHomeownerApplication, useHousehold } from '../../hooks/applicantSelection'
import ApproveApplicationPanel from './ApproveApplicationPanel'
import RecordHousingCounselingPanel from './RecordHousingCounselingPanel'
import RecordCltOrientationPanel from './RecordCltOrientationPanel'
import RecordMortgagePrequalificationPanel from './RecordMortgagePrequalificationPanel'
import DenyApplicationPanel from './DenyApplicationPanel'
import WithdrawApplicationPanel from './WithdrawApplicationPanel'
import { getResponseStatus } from '../../lib/getErrorMessage'
import { formatPhoneDisplay } from '../../lib/phone'

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
  const { data: household, isError: isHouseholdError } = useHousehold(detail?.householdId)
  const [actionPanel, setActionPanel] = useState<
    | 'approve-application'
    | 'record-housing-counseling'
    | 'record-clt-orientation'
    | 'record-mortgage-prequalification'
    | 'deny-application'
    | 'withdraw-application'
    | null
  >(null)

  const phase = (detail?.phase ?? '').toLowerCase()
  const status = getResponseStatus(error)
  const isSubmittedPhase = phase === 'submitted'
  const isApprovedPhase = phase === 'approved'
  const isOpen = isSubmittedPhase || isApprovedPhase
  const prequalification = household?.mortgagePrequalification

  return (
    <SlideOver
      title={detail?.primaryContact?.name ?? 'Application'}
      subtitle={detail?.primaryContact?.email ?? formatPhoneDisplay(detail?.primaryContact?.phone)}
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
              <div className="detail-field__value">{formatPhoneDisplay(detail.primaryContact?.phone) ?? '—'}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Declared Household Size</div>
              <div className="detail-field__value mono">{detail.householdSize ?? '—'}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Declared Annual Income</div>
              <div className="detail-field__value mono">
                {typeof detail.annualIncome === 'number' ? fmtCurrency(detail.annualIncome) : '—'}
              </div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Declared Accessibility Need</div>
              <div className="detail-field__value">{detail.accessibilityRequired ? 'Yes' : 'No'}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Approved Date</div>
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

          <div className="detail-section" style={{ marginTop: 16 }}>
            <div className="detail-section__title">Household Members</div>
            {isHouseholdError && (
              <div className="detail-field__value">Could not load household.</div>
            )}
            {household && (
              <div className="detail-grid" style={{ marginTop: 10 }}>
                {(household.members ?? []).map((member, i) => (
                  <div className="detail-field full-span" key={`${member.email}-${i}`}>
                    <div className="detail-field__label">{member.isPrimary ? 'Primary Contact' : 'Member'}</div>
                    <div className="detail-field__value">
                      {[member.name, member.email, formatPhoneDisplay(member.phone)].filter(Boolean).join(' · ')}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {household && (
            <div className="detail-section" style={{ marginTop: 16 }}>
              <div className="detail-section__title">Buyer Preparation Checklist</div>
              <div className="checklist">
                <div className={`checklist-item${household.isHousingCounselingCompleted ? ' checklist-item--done' : ''}`}>
                  <div className={`checklist-item__icon${household.isHousingCounselingCompleted ? ' checklist-item__icon--done' : ' checklist-item__icon--pending'}`}>
                    {household.isHousingCounselingCompleted ? '✓' : '○'}
                  </div>
                  <span className="checklist-item__label">Housing Counseling</span>
                  {household.isHousingCounselingCompleted && (
                    <span className="checklist-item__status">Completed {fmtDate(household.housingCounselingCompletedDate)}</span>
                  )}
                  {!household.isHousingCounselingCompleted && isOpen && (
                    <button className="btn btn-secondary btn-sm" onClick={() => setActionPanel('record-housing-counseling')}>
                      Record
                    </button>
                  )}
                </div>

                <div className={`checklist-item${household.isCltOrientationCompleted ? ' checklist-item--done' : ''}`}>
                  <div className={`checklist-item__icon${household.isCltOrientationCompleted ? ' checklist-item__icon--done' : ' checklist-item__icon--pending'}`}>
                    {household.isCltOrientationCompleted ? '✓' : '○'}
                  </div>
                  <span className="checklist-item__label">CLT Orientation</span>
                  {household.isCltOrientationCompleted && (
                    <span className="checklist-item__status">Completed {fmtDate(household.cltOrientationCompletedDate)}</span>
                  )}
                  {!household.isCltOrientationCompleted && isOpen && (
                    <button className="btn btn-secondary btn-sm" onClick={() => setActionPanel('record-clt-orientation')}>
                      Record
                    </button>
                  )}
                </div>

                <div className={`checklist-item${prequalification ? ' checklist-item--done' : ''}`}>
                  <div className={`checklist-item__icon${prequalification ? ' checklist-item__icon--done' : ' checklist-item__icon--pending'}`}>
                    {prequalification ? '✓' : '○'}
                  </div>
                  <span className="checklist-item__label">Mortgage Prequalification</span>
                  {prequalification && (
                    <span className="checklist-item__status">
                      {prequalification.lenderName} · expires {fmtDate(prequalification.expiryDate)}
                    </span>
                  )}
                  {isOpen && (
                    <button className="btn btn-secondary btn-sm" onClick={() => setActionPanel('record-mortgage-prequalification')}>
                      {prequalification ? 'Update' : 'Record'}
                    </button>
                  )}
                </div>
              </div>
            </div>
          )}

          {prequalification && (
            <div className="detail-section" style={{ marginTop: 16 }}>
              <div className="detail-section__title">Mortgage Prequalification</div>
              <div className="detail-grid" style={{ marginTop: 10 }}>
                <div className="detail-field">
                  <div className="detail-field__label">Lender Name</div>
                  <div className="detail-field__value">{prequalification.lenderName ?? '—'}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Prequalified Amount</div>
                  <div className="detail-field__value mono">
                    {typeof prequalification.approvedAmount === 'number'
                      ? fmtCurrency(prequalification.approvedAmount)
                      : '—'}
                  </div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Prequalification Expiry</div>
                  <div className="detail-field__value mono">
                    {fmtDate(prequalification.expiryDate)}
                  </div>
                </div>
              </div>
            </div>
          )}

          <div className="detail-section" style={{ marginTop: 16 }}>
            <div className="detail-section__title">Workflow Actions</div>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
              {isSubmittedPhase && (
                <button className="btn btn-secondary btn-sm" onClick={() => setActionPanel('approve-application')}>
                  Approve Application
                </button>
              )}

              {isOpen && (
                <>
                  <button className="btn btn-ghost btn-sm" onClick={() => setActionPanel('deny-application')}>
                    Deny Application
                  </button>
                  <button className="btn btn-ghost btn-sm" onClick={() => setActionPanel('withdraw-application')}>
                    Withdraw Application
                  </button>
                </>
              )}

              {!isOpen && (
                <div className="detail-field__value">No actions available for this phase.</div>
              )}
            </div>
          </div>
        </>
      )}

      {actionPanel === 'approve-application' && (
        <ApproveApplicationPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'record-housing-counseling' && detail?.householdId && (
        <RecordHousingCounselingPanel householdId={detail.householdId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'record-clt-orientation' && detail?.householdId && (
        <RecordCltOrientationPanel householdId={detail.householdId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'record-mortgage-prequalification' && detail?.householdId && (
        <RecordMortgagePrequalificationPanel householdId={detail.householdId} onClose={() => setActionPanel(null)} />
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
