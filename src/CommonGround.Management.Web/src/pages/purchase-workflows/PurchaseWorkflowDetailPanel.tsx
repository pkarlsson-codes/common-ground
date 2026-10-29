import { statusLabel } from '../../hooks/types'
import { usePurchaseWorkflow } from '../../hooks/purchaseWorkflows'
import SlideOver from '../../components/SlideOver'
import PurchaseTrackSection from './PurchaseTrackSection'
import PurchaseWorkflowSnapshotSection from './PurchaseWorkflowSnapshotSection'
import PurchaseExceptionActionsSection from './PurchaseExceptionActionsSection'
import { usePurchaseWorkflowDetailController } from './usePurchaseWorkflowDetailController'
import { DateInput } from '../../components/DateInput'

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

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

  return (
    <SlideOver
      title={detail?.buyerFullName ?? 'Purchase Workflow'}
      subtitle={detail ? `${fmtCurrency(Number(detail.purchasePrice))} · ${statusLabel(detail.phase ?? '')}` : undefined}
      onClose={onClose}
    >
      {detailLoading || !detail ? <SkeletonRows /> : (
        <>
          <PurchaseWorkflowSnapshotSection detail={detail} />

          {s.canAct && (
            <div className="detail-section">
              <div className="detail-section__title">Purchase Tracks</div>
              <div style={{ marginBottom: 12, fontSize: 12, color: 'var(--text-secondary)' }}>
                All five tracks must be complete before closing can be scheduled.
              </div>

              <PurchaseTrackSection
                label="Eligibility"
                summary={
                  detail.eligibilityDetails
                    ? `Verified ${detail.eligibilityDetails.verifiedDate ?? ''}`.trim()
                    : 'Pending'
                }
                complete={!!detail.eligibilityDetails}
                expandable={!detail.eligibilityDetails}
                expanded={s.expandedTracks.has('eligibility')}
                onToggle={() => s.toggleTrack('eligibility')}
              >
                <form onSubmit={e => s.handleElig(e, s.eligForm)}>
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
                    <div className="form-field full">
                      <label className="form-label">Notes (optional)</label>
                      <input
                        className="form-input"
                        value={s.eligForm.notes}
                        onChange={e => s.setEligForm(f => ({ ...f, notes: e.target.value }))}
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={s.eligPending}>
                    {s.eligPending ? 'Saving…' : 'Record Eligibility'}
                  </button>
                </form>
              </PurchaseTrackSection>

              <PurchaseTrackSection
                label="Financing"
                summary={
                  detail.financingDetails
                    ? `${detail.financingDetails.lenderName ?? ''} · ${fmtCurrency(Number(detail.financingDetails.committedAmount ?? 0))} committed`
                    : 'Pending'
                }
                complete={!!detail.financingDetails}
                expandable={!detail.financingDetails}
                expanded={s.expandedTracks.has('financing')}
                onToggle={() => s.toggleTrack('financing')}
              >
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
