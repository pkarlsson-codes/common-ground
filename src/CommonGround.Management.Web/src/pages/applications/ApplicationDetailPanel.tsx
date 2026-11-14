import { useState } from 'react'
import { Link } from 'react-router-dom'
import { formatCalendarDate, formatInstant, numericDate, type CalendarDate } from '../../lib/calendarDate'
import SlideOver from '../../components/SlideOver'
import NavIdLink from '../../components/NavIdLink'
import { useHomeownerApplication } from '../../hooks/applications'
import { homeSelectionPath } from '../../hooks/homeSelections'
import ApproveApplicationPanel from './ApproveApplicationPanel'
import DenyApplicationPanel from './DenyApplicationPanel'
import ApplicationPhaseBadge from './ApplicationPhaseBadge'
import WithdrawApplicationPanel from './WithdrawApplicationPanel'
import RemoveFromWaitingListPanel from './RemoveFromWaitingListPanel'
import { waitingListRemovalReasonLabel } from './waitingListRemovalReasons'
import { getResponseStatus } from '../../lib/getErrorMessage'
import { formatPhoneDisplay } from '../../lib/phone'

type Props = {
  applicationId: string
  onClose: () => void
}

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtDateTime = (value?: Date | null) => (value ? formatInstant(value) : '—')

const fmtDate = (value?: CalendarDate | null) => (value ? formatCalendarDate(value, numericDate) : '—')

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
    | 'approve-application'
    | 'deny-application'
    | 'withdraw-application'
    | 'remove-from-waiting-list'
    | null
  >(null)

  const phase = (detail?.phase ?? '').toLowerCase()
  const status = getResponseStatus(error)
  const isSubmittedPhase = phase === 'submitted'
  const isApprovedPhase = phase === 'approved'
  const isOpen = isSubmittedPhase || isApprovedPhase
  const isSelectedPhase = phase === 'selected'
  const isRemovedPhase = phase === 'removed'
  const passOvers = detail?.passOvers ?? []

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
              <div className="detail-field__value"><ApplicationPhaseBadge phase={detail.phase} /></div>
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
            {(isApprovedPhase || isSelectedPhase || isRemovedPhase) && (
              <div className="detail-field">
                <div className="detail-field__label">Approved Date</div>
                <div className="detail-field__value mono">{fmtDate(detail.approvedDate)}</div>
              </div>
            )}
            {isSelectedPhase && (
              <div className="detail-field">
                <div className="detail-field__label">Selected Date</div>
                <div className="detail-field__value mono">{fmtDate(detail.selectedDate)}</div>
              </div>
            )}
            {phase === 'denied' && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Denial Reason</div>
                <div className="detail-field__value">{detail.denialReason ?? '—'}</div>
              </div>
            )}
            {isRemovedPhase && (
              <>
                <div className="detail-field">
                  <div className="detail-field__label">Removed Date</div>
                  <div className="detail-field__value mono">{fmtDate(detail.removedDate)}</div>
                </div>
                <div className="detail-field">
                  <div className="detail-field__label">Removal Reason</div>
                  <div className="detail-field__value">{waitingListRemovalReasonLabel(detail.removalReason)}</div>
                </div>
                {detail.removalNote && (
                  <div className="detail-field full-span">
                    <div className="detail-field__label">Removal Note</div>
                    <div className="detail-field__value">{detail.removalNote}</div>
                  </div>
                )}
              </>
            )}
            {phase === 'withdrawn' && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Withdrawal Reason</div>
                <div className="detail-field__value">{detail.withdrawalReason ?? '—'}</div>
              </div>
            )}
            <div className="detail-field full-span">
              <div className="detail-field__value">
                <NavIdLink id={detail.householdId} to="/households" label="Open Household" />
              </div>
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

          {passOvers.length > 0 && (
            <div className="detail-section" style={{ marginTop: 16 }}>
              <div className="detail-section__title">Passed Over</div>
              <table className="registry-table" style={{ marginTop: 10 }}>
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Selection Method</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  {passOvers.map(passOver => (
                    <tr key={passOver.selectionId}>
                      <td><span className="cell-mono">{fmtDateTime(passOver.passedOverAt)}</span></td>
                      <td>{passOver.selectionMethod ?? '—'}</td>
                      <td style={{ textAlign: 'right' }}>
                        <Link className="btn btn-link btn-sm" to={homeSelectionPath(passOver.selectionId!)}>
                          Open Selection
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          <div className="detail-section" style={{ marginTop: 16 }}>
            <div className="detail-section__title">Workflow Actions</div>
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8 }}>
              {isSubmittedPhase && (
                <button className="btn btn-primary btn-sm" onClick={() => setActionPanel('approve-application')}>
                  Approve Application
                </button>
              )}

              {!isOpen && (
                <div className="detail-field__value">No actions available for this phase.</div>
              )}
            </div>
            {isOpen && (
              <div className="workflow-actions__danger">
                {isSubmittedPhase && (
                  <button className="btn btn-danger-ghost btn-sm" onClick={() => setActionPanel('deny-application')}>
                    Deny Application
                  </button>
                )}
                {isApprovedPhase && (
                  <button className="btn btn-danger-ghost btn-sm" onClick={() => setActionPanel('remove-from-waiting-list')}>
                    Remove from Waiting List
                  </button>
                )}
                <button className="btn btn-danger-ghost btn-sm" onClick={() => setActionPanel('withdraw-application')}>
                  Withdraw Application
                </button>
              </div>
            )}
          </div>
        </>
      )}

      {actionPanel === 'approve-application' && (
        <ApproveApplicationPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'deny-application' && (
        <DenyApplicationPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
      {actionPanel === 'remove-from-waiting-list' && (
        <RemoveFromWaitingListPanel
          applicationId={applicationId}
          approvedDate={detail?.approvedDate}
          onClose={() => setActionPanel(null)}
        />
      )}
      {actionPanel === 'withdraw-application' && (
        <WithdrawApplicationPanel applicationId={applicationId} onClose={() => setActionPanel(null)} />
      )}
    </SlideOver>
  )
}
