import { statusLabel } from '../../hooks/types'
import { usePurchaseWorkflow } from '../../hooks/purchaseWorkflows'
import { useHomeAssessments } from '../../hooks/homes'
import SlideOver from '../../components/SlideOver'
import PurchaseTrackSection from './PurchaseTrackSection'
import PurchaseWorkflowSnapshotSection from './PurchaseWorkflowSnapshotSection'
import PurchaseDocumentLinks from './PurchaseDocumentLinks'
import PurchaseExceptionActionsSection from './PurchaseExceptionActionsSection'
import { usePurchaseWorkflowDetailController } from './usePurchaseWorkflowDetailController'
import { DateInput } from '../../components/DateInput'
import { useEffect } from 'react'
import type { PurchaseWorkflowResponse } from '../../api'
import { formatCalendarDate, trustToday } from '../../lib/calendarDate'
import { incomeBandLabel } from '../../lib/incomeBand'
import { EligibilityIncomeCheck } from './EligibilityIncomeCheck'
import { CorrectAppraisalPanel, CorrectEligibilityPanel, CorrectFinancingPanel, CorrectionsList } from './PurchaseTrackCorrections'

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const incomeLimitDescription = (detail: PurchaseWorkflowResponse) => {
  const limit = detail.incomeLimit
  if (limit?.percentOfAmi == null) {
    const schedule = detail.incomeLimitSchedule
    return schedule?.effectiveFrom
      ? `No income limit; income band from limits effective ${formatCalendarDate(schedule.effectiveFrom)}`
      : 'No income limit'
  }
  const effective = limit.effectiveFrom ? `, limits effective ${formatCalendarDate(limit.effectiveFrom)}` : ''
  return `${limit.percentOfAmi}% of AMI${effective}`
}

const eligibilitySummary = (detail: PurchaseWorkflowResponse) => {
  const e = detail.eligibilityDetails
  if (!e) return 'Pending'
  const size = e.verifiedHouseholdSize ?? 0
  const income = `${fmtCurrency(Number(e.verifiedAnnualIncome ?? 0))} income, ${size} ${size === 1 ? 'person' : 'people'}`
  const within = e.incomeLimit == null ? '' : `, within the ${fmtCurrency(e.incomeLimit)} limit`
  const band = incomeBandLabel({ overPercentOfAmi: e.incomeBandOverPercentOfAmi, upToPercentOfAmi: e.incomeBandUpToPercentOfAmi })
  return `Verified ${e.verifiedDate ?? ''}: ${income}${within}${band ? ` · ${band}` : ''}`
}

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

interface PurchaseWorkflowDetailPanelProps {
  selectedId: string
  onClose: () => void
  showToast: (msg: string, kind: 'success' | 'error') => void
}

export default function PurchaseWorkflowDetailPanel({
  selectedId,
  onClose,
  showToast,
}: PurchaseWorkflowDetailPanelProps) {
  const { data: detail, isLoading: detailLoading } = usePurchaseWorkflow(selectedId)

  const s = usePurchaseWorkflowDetailController({
    selectedId,
    detail,
    showToast: (msg: string, kind?: 'success' | 'error') => showToast(msg, kind ?? 'success'),
    isDetailOpen: true,
  })

  // Appraised Value starts from the home's latest assessment; staff confirm or correct it.
  const { data: assessments } = useHomeAssessments(detail?.homeId ?? null, 1, 1)
  const latestAssessedValue = assessments?.items?.[0]?.assessedValue
  const { setAppraisalForm } = s
  useEffect(() => {
    if (latestAssessedValue) {
      setAppraisalForm(f => (f.appraisedValue ? f : { ...f, appraisedValue: Number(latestAssessedValue) }))
    }
  }, [latestAssessedValue, selectedId, setAppraisalForm])

  return (
    <SlideOver
      title={detail?.buyerFullName ?? 'Purchase Workflow'}
      subtitle={detail ? `${fmtCurrency(Number(detail.purchasePrice))} · ${statusLabel(detail.phase ?? '')}` : undefined}
      onClose={onClose}
      isDirty={s.canAct && !!(s.failForm.reason || s.withdrawForm.reason)}
    >
      {detailLoading || !detail ? <SkeletonRows /> : (
        <>
          <PurchaseWorkflowSnapshotSection detail={detail} />

          {s.canAct && (
            <div className="detail-section">
              <div className="detail-section__title">Purchase Tracks</div>
              <div style={{ marginBottom: 12, fontSize: 12, color: 'var(--text-secondary)' }}>
                All seven tracks must be complete before closing can be scheduled.
              </div>

              <PurchaseTrackSection
                label="Eligibility"
                summary={eligibilitySummary(detail)}
                complete={!!detail.eligibilityDetails}
                actionLabel={detail.eligibilityDetails ? 'Correct' : undefined}
                expanded={s.expandedTracks.has('eligibility')}
                onToggle={() => s.toggleTrack('eligibility')}
              >
                {detail.eligibilityDetails ? (
                  <CorrectEligibilityPanel detail={detail} showToast={showToast} onDone={() => s.collapseTrack('eligibility')} />
                ) : (
                <form onSubmit={e => s.handleElig(e, s.eligForm)}>
                  <div style={{ marginBottom: 10, fontSize: 12, color: 'var(--text-secondary)' }}>
                    Income limit: {incomeLimitDescription(detail)}
                  </div>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Verified Date</label>
                      <DateInput
                        className="form-input"
                        value={s.eligForm.verifiedDate}
                        onChange={v => s.setEligForm(f => ({ ...f, verifiedDate: v }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label" htmlFor="elig-income">Verified Annual Income ($)</label>
                      <input
                        id="elig-income"
                        className="form-input"
                        type="number"
                        min="0"
                        step="0.01"
                        value={s.eligForm.verifiedAnnualIncome}
                        onChange={e => s.setEligForm(f => ({ ...f, verifiedAnnualIncome: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label" htmlFor="elig-size">Verified Household Size</label>
                      <input
                        id="elig-size"
                        className="form-input"
                        type="number"
                        min="1"
                        step="1"
                        value={s.eligForm.verifiedHouseholdSize}
                        onChange={e => s.setEligForm(f => ({ ...f, verifiedHouseholdSize: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field full">
                      <EligibilityIncomeCheck detail={detail} form={s.eligForm} />
                    </div>
                    <div className="form-field full">
                      <label className="form-label" htmlFor="elig-income-files">Income Documents</label>
                      <input
                        id="elig-income-files"
                        type="file"
                        multiple
                        accept="application/pdf,image/*"
                        className="form-input"
                        style={{ cursor: 'pointer' }}
                        onChange={e => s.setEligForm(f => ({ ...f, incomeFiles: Array.from(e.target.files ?? []) }))}
                      />
                      <p className="form-hint">
                        {s.eligForm.incomeFiles.length > 0
                          ? `${s.eligForm.incomeFiles.map(f => f.name).join(', ')} go to Documents, attached to this purchase.`
                          : 'Pay stubs, tax returns or benefit letters. At least one file.'}
                      </p>
                    </div>
                    <div className="form-field full">
                      <label className="form-label">Notes (optional)</label>
                      <input
                        className="form-input"
                        value={s.eligForm.notes}
                        onChange={e => s.setEligForm(f => ({ ...f, notes: e.target.value }))}
                      />
                    </div>
                  </div>
                  <button
                    type="submit"
                    className="btn btn-secondary btn-sm"
                    disabled={s.eligPending || s.eligForm.incomeFiles.length === 0}
                  >
                    {s.eligPending ? 'Saving…' : 'Record Eligibility'}
                  </button>
                </form>
                )}
              </PurchaseTrackSection>

              <PurchaseTrackSection
                label="Appraisal"
                summary={
                  detail.appraisalDetails
                    ? `${fmtCurrency(Number(detail.appraisalDetails.appraisedValue ?? 0))} · appraised ${detail.appraisalDetails.appraisalDate ?? ''} by ${detail.appraisalDetails.appraiserName ?? ''}`
                    : 'Pending'
                }
                complete={!!detail.appraisalDetails}
                actionLabel={detail.appraisalDetails ? 'Correct' : undefined}
                expanded={s.expandedTracks.has('appraisal')}
                onToggle={() => s.toggleTrack('appraisal')}
              >
                {detail.appraisalDetails ? (
                  <CorrectAppraisalPanel detail={detail} showToast={showToast} onDone={() => s.collapseTrack('appraisal')} />
                ) : (
                <form onSubmit={e => s.handleAppraisal(e, s.appraisalForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Appraised Value ($)</label>
                      <input
                        className="form-input"
                        type="number"
                        min="1"
                        step="1"
                        value={s.appraisalForm.appraisedValue || ''}
                        onChange={e => s.setAppraisalForm(f => ({ ...f, appraisedValue: parseFloat(e.target.value) || 0 }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label">Appraisal Date</label>
                      <DateInput
                        className="form-input"
                        value={s.appraisalForm.appraisalDate}
                        onChange={v => s.setAppraisalForm(f => ({ ...f, appraisalDate: v }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label">Appraiser Name</label>
                      <input
                        className="form-input"
                        value={s.appraisalForm.appraiserName}
                        onChange={e => s.setAppraisalForm(f => ({ ...f, appraiserName: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field full">
                      <label className="form-label">Notes (optional)</label>
                      <input
                        className="form-input"
                        value={s.appraisalForm.notes}
                        onChange={e => s.setAppraisalForm(f => ({ ...f, notes: e.target.value }))}
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={s.appraisalPending}>
                    {s.appraisalPending ? 'Saving…' : 'Record Appraisal'}
                  </button>
                </form>
                )}
              </PurchaseTrackSection>

              <PurchaseTrackSection
                label="Financing"
                summary={
                  detail.financingDetails
                    ? `${detail.financingDetails.lenderName ?? ''} · ${fmtCurrency(Number(detail.financingDetails.committedAmount ?? 0))} committed`
                    : 'Pending'
                }
                complete={!!detail.financingDetails}
                actionLabel={detail.financingDetails ? 'Correct' : undefined}
                expanded={s.expandedTracks.has('financing')}
                onToggle={() => s.toggleTrack('financing')}
              >
                {detail.financingDetails ? (
                  <CorrectFinancingPanel detail={detail} showToast={showToast} onDone={() => s.collapseTrack('financing')} />
                ) : (
                <form onSubmit={e => s.handleFin(e, s.finForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Lender Name</label>
                      <input
                        className="form-input"
                        value={s.finForm.lenderName}
                        onChange={e => s.setFinForm(f => ({ ...f, lenderName: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label">Committed Amount ($)</label>
                      <input
                        className="form-input"
                        type="number"
                        min="0"
                        step="1"
                        value={s.finForm.committedAmount || ''}
                        onChange={e => s.setFinForm(f => ({ ...f, committedAmount: parseFloat(e.target.value) || 0 }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label">Pre-Approval Letter Date</label>
                      <DateInput
                        className="form-input"
                        value={s.finForm.preapprovalLetterDate}
                        onChange={v => s.setFinForm(f => ({ ...f, preapprovalLetterDate: v }))}
                        required
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={s.finPending}>
                    {s.finPending ? 'Saving…' : 'Record Financing'}
                  </button>
                </form>
                )}
              </PurchaseTrackSection>

              <PurchaseTrackSection
                label="Inspection"
                summary={
                  detail.inspectionResult
                    ? `Inspected ${detail.inspectionResult.inspectionDate ?? ''} by ${detail.inspectionResult.inspectorName ?? ''}`
                    : 'Pending'
                }
                complete={!!detail.inspectionResult}
                expandable={!detail.inspectionResult}
                expanded={s.expandedTracks.has('inspection')}
                onToggle={() => s.toggleTrack('inspection')}
              >
                <form onSubmit={e => s.handleInsp(e, s.inspForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Inspection Date</label>
                      <DateInput
                        className="form-input"
                        value={s.inspForm.inspectionDate}
                        onChange={v => s.setInspForm(f => ({ ...f, inspectionDate: v }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label">Inspector Name</label>
                      <input
                        className="form-input"
                        value={s.inspForm.inspectorName}
                        onChange={e => s.setInspForm(f => ({ ...f, inspectorName: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field full">
                      <label className="form-label">Notes (optional)</label>
                      <input
                        className="form-input"
                        value={s.inspForm.notes}
                        onChange={e => s.setInspForm(f => ({ ...f, notes: e.target.value }))}
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={s.inspPending}>
                    {s.inspPending ? 'Saving…' : 'Record Inspection'}
                  </button>
                </form>
              </PurchaseTrackSection>

              <PurchaseTrackSection
                label="Title"
                summary={
                  detail.titleDetails
                    ? `Cleared ${detail.titleDetails.clearedDate ?? ''}${detail.titleDetails.titleCompanyName ? ` · ${detail.titleDetails.titleCompanyName}` : ''}`
                    : 'Pending'
                }
                complete={!!detail.titleDetails}
                expandable={!detail.titleDetails}
                expanded={s.expandedTracks.has('title')}
                onToggle={() => s.toggleTrack('title')}
              >
                <form onSubmit={e => s.handleTitle(e, s.titleForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Cleared Date</label>
                      <DateInput
                        className="form-input"
                        value={s.titleForm.clearedDate}
                        onChange={v => s.setTitleForm(f => ({ ...f, clearedDate: v }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label">Title Company (optional)</label>
                      <input
                        className="form-input"
                        value={s.titleForm.titleCompanyName}
                        onChange={e => s.setTitleForm(f => ({ ...f, titleCompanyName: e.target.value }))}
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={s.titlePending}>
                    {s.titlePending ? 'Saving…' : 'Record Title Cleared'}
                  </button>
                </form>
              </PurchaseTrackSection>

              <PurchaseTrackSection
                label="Lease Execution"
                summary={
                  detail.leaseExecutionDetails
                    ? `Signed ${detail.leaseExecutionDetails.signedDate ?? ''}`
                    : 'Pending'
                }
                complete={!!detail.leaseExecutionDetails}
                expandable={!detail.leaseExecutionDetails}
                expanded={s.expandedTracks.has('leaseExecution')}
                onToggle={() => s.toggleTrack('leaseExecution')}
              >
                <form onSubmit={e => s.handleLeaseEx(e, s.leaseExecForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Signed Date</label>
                      <DateInput
                        className="form-input"
                        value={s.leaseExecForm.signedDate}
                        onChange={v => s.setLeaseExecForm(f => ({ ...f, signedDate: v }))}
                        required
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={s.leaseExecPending}>
                    {s.leaseExecPending ? 'Saving…' : 'Record Lease Executed'}
                  </button>
                </form>
              </PurchaseTrackSection>

              <PurchaseTrackSection
                label="Disclosure"
                summary={
                  detail.disclosureDetails
                    ? `Acknowledged ${detail.disclosureDetails.acknowledgedDate ?? ''}`
                    : 'Pending'
                }
                complete={!!detail.disclosureDetails}
                expandable={!detail.disclosureDetails}
                expanded={s.expandedTracks.has('disclosure')}
                onToggle={() => s.toggleTrack('disclosure')}
              >
                <form onSubmit={e => s.handleDisclosure(e, s.disclosureForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label" htmlFor="disclosure-date">Acknowledged Date</label>
                      <DateInput
                        id="disclosure-date"
                        className="form-input"
                        value={s.disclosureForm.acknowledgedDate}
                        max={trustToday()}
                        onChange={v => s.setDisclosureForm(f => ({ ...f, acknowledgedDate: v }))}
                        required
                      />
                    </div>
                    <div className="form-field full">
                      <label className="form-label" htmlFor="disclosure-file">Signed Disclosure</label>
                      <input
                        id="disclosure-file"
                        type="file"
                        accept="application/pdf,image/*"
                        className="form-input"
                        style={{ cursor: 'pointer' }}
                        onChange={e => s.setDisclosureForm(f => ({ ...f, file: e.target.files?.[0] ?? null }))}
                      />
                      <p className="form-hint">
                        {s.disclosureForm.file
                          ? `${s.disclosureForm.file.name} goes to Documents, attached to this purchase.`
                          : 'The buyer-signed acknowledgment. PDF or image.'}
                      </p>
                    </div>
                    <div className="form-field full">
                      <label className="form-label" htmlFor="disclosure-notes">Notes (optional)</label>
                      <input
                        id="disclosure-notes"
                        className="form-input"
                        value={s.disclosureForm.notes}
                        onChange={e => s.setDisclosureForm(f => ({ ...f, notes: e.target.value }))}
                      />
                    </div>
                  </div>
                  <button
                    type="submit"
                    className="btn btn-secondary btn-sm"
                    disabled={s.disclosurePending || !s.disclosureForm.file}
                  >
                    {s.disclosurePending ? 'Saving…' : 'Record Disclosure'}
                  </button>
                </form>
              </PurchaseTrackSection>

              {(s.canSchedule || detail.scheduledClosingDate || detail.actualClosingDate) && (() => {
                const closed = !!detail.actualClosingDate
                const scheduled = !closed && !!detail.scheduledClosingDate
                const summary = closed
                  ? `Closed ${detail.actualClosingDate}`
                  : scheduled
                    ? `Scheduled for ${detail.scheduledClosingDate}`
                    : 'Awaiting schedule'
                return (
                  <PurchaseTrackSection
                    label="Closing"
                    summary={summary}
                    complete={closed}
                    expandable={!closed}
                    expanded={s.expandedTracks.has('closing')}
                    onToggle={() => s.toggleTrack('closing')}
                  >
                    {s.canClose ? (
                      <form onSubmit={e => s.handleClose(e, s.closeForm)}>
                        <div className="form-grid" style={{ marginBottom: 12 }}>
                          <div className="form-field">
                            <label className="form-label">Actual Closing Date</label>
                            <DateInput
                              className="form-input"
                              value={s.closeForm.actualClosingDate}
                              onChange={v => s.setCloseForm(f => ({ ...f, actualClosingDate: v }))}
                              required
                            />
                          </div>
                          <div className="form-field full">
                            <label className="form-label">Closing Notes (optional)</label>
                            <textarea
                              className="form-textarea"
                              value={s.closeForm.closingNotes}
                              onChange={e => s.setCloseForm(f => ({ ...f, closingNotes: e.target.value }))}
                            />
                          </div>
                        </div>
                        <button type="submit" className="btn btn-primary" disabled={s.closePending}>
                          {s.closePending ? 'Recording…' : 'Record Purchase Closed'}
                        </button>
                        {s.closeError && <div className="form-error">Failed to record closing.</div>}
                      </form>
                    ) : s.canSchedule ? (
                      <form onSubmit={e => s.handleSchedule(e, s.scheduleForm)}>
                        <div className="form-grid" style={{ marginBottom: 12 }}>
                          <div className="form-field">
                            <label className="form-label">Scheduled Date</label>
                            <DateInput
                              className="form-input"
                              value={s.scheduleForm.scheduledDate}
                              onChange={v => s.setScheduleForm(f => ({ ...f, scheduledDate: v }))}
                              required
                            />
                          </div>
                        </div>
                        <button type="submit" className="btn btn-primary" disabled={s.schedulePending}>
                          {s.schedulePending ? 'Scheduling…' : 'Schedule Closing'}
                        </button>
                        {s.scheduleError && <div className="form-error">Failed to schedule closing.</div>}
                      </form>
                    ) : null}
                  </PurchaseTrackSection>
                )
              })()}
            </div>
          )}

          {(detail.corrections?.length ?? 0) > 0 && (
            <div className="detail-section">
              <CorrectionsList corrections={detail.corrections ?? []} />
            </div>
          )}

          <PurchaseDocumentLinks detail={detail} />

          {s.canAct && (
            <PurchaseExceptionActionsSection
              canFail={s.hasWorkflowAction('workflow:fail')}
              canWithdraw={s.hasWorkflowAction('workflow:withdraw')}
              failForm={s.failForm}
              setFailForm={s.setFailForm}
              withdrawForm={s.withdrawForm}
              setWithdrawForm={s.setWithdrawForm}
              failPending={s.failPending}
              withdrawPending={s.withdrawPending}
              onFail={s.handleFail}
              onWithdraw={s.handleWithdraw}
            />
          )}
        </>
      )}
    </SlideOver>
  )
}
