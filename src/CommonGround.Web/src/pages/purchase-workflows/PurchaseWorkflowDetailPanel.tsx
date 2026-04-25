import { statusLabel } from '../../hooks/types'
import { usePurchaseWorkflow } from '../../hooks/purchaseWorkflows'
import NavIdLink from '../../components/NavIdLink'
import SlideOver from '../../components/SlideOver'
import StatusBadge from '../../components/StatusBadge'
import { usePurchaseWorkflowDetailController } from './usePurchaseWorkflowDetailController'

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

function TrackRow({
  label,
  complete,
  children,
}: {
  label: string
  complete: boolean
  children: React.ReactNode
}) {
  return (
    <div
      style={{
        borderLeft: `3px solid ${complete ? 'var(--green-text)' : 'var(--border-bright)'}`,
        paddingLeft: 12,
        marginBottom: 16,
      }}
    >
      <div
        style={{
          fontFamily: 'var(--font-mono)',
          fontSize: 11,
          color: complete ? 'var(--green-text)' : 'var(--text-secondary)',
          marginBottom: 6,
          textTransform: 'uppercase',
          letterSpacing: '0.06em',
        }}
      >
        {complete ? '✓ ' : ''}{label}
      </div>
      {!complete && children}
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
  
  const detailPanelState = usePurchaseWorkflowDetailController({
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
          <div className="detail-grid">
            <div className="detail-field">
              <div className="detail-field__label">Phase</div>
              <div className="detail-field__value"><StatusBadge status={detail.phase ?? ''} /></div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Purchase Price</div>
              <div className="detail-field__value mono">{fmtCurrency(Number(detail.purchasePrice))}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Email</div>
              <div className="detail-field__value">{detail.buyerEmail}</div>
            </div>
            {detail.buyerPhone && (
              <div className="detail-field">
                <div className="detail-field__label">Phone</div>
                <div className="detail-field__value mono">{detail.buyerPhone}</div>
              </div>
            )}
            <div className="detail-field full-span">
              <div className="detail-field__value">
                <NavIdLink id={detail.groundLeaseId} to="/ground-leases" label="Open Ground Lease" />
              </div>
            </div>
            {detail.scheduledClosingDate && (
              <div className="detail-field">
                <div className="detail-field__label">Scheduled Closing</div>
                <div className="detail-field__value mono">{detail.scheduledClosingDate}</div>
              </div>
            )}
            {detail.actualClosingDate && (
              <div className="detail-field">
                <div className="detail-field__label">Actual Closing</div>
                <div className="detail-field__value mono">{detail.actualClosingDate}</div>
              </div>
            )}
            {detail.closingNotes && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Closing Notes</div>
                <div className="detail-field__value">{detail.closingNotes}</div>
              </div>
            )}
            {detail.failureReason && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Failure Reason</div>
                <div className="detail-field__value" style={{ color: 'var(--red)' }}>{detail.failureReason}</div>
              </div>
            )}
            {detail.withdrawalReason && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Withdrawal Reason</div>
                <div className="detail-field__value">{detail.withdrawalReason}</div>
              </div>
            )}
            {detail.stalledReason && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Stall Reason</div>
                <div className="detail-field__value" style={{ color: 'var(--orange-text)' }}>{detail.stalledReason}</div>
              </div>
            )}
            {detail.resolutionTargetDate && (
              <div className="detail-field">
                <div className="detail-field__label">Resolution Target</div>
                <div className="detail-field__value mono">{detail.resolutionTargetDate}</div>
              </div>
            )}
            <div className="detail-field">
              <div className="detail-field__label">Created</div>
              <div className="detail-field__value mono">
                {new Date(detail.createdAt!).toLocaleString()}
              </div>
            </div>
          </div>

          {detailPanelState.canAct && (
            <div className="detail-section">
              <div className="detail-section__title">Purchase Tracks</div>
              <div style={{ marginBottom: 4, fontSize: 12, color: 'var(--text-secondary)' }}>
                All five tracks must be complete before closing can be scheduled.
              </div>

              <TrackRow label="Eligibility" complete={!!detail.eligibilityDetails}>
                <form onSubmit={e => detailPanelState.handleElig(e, detailPanelState.eligForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Verified Date</label>
                      <input
                        className="form-input"
                        type="date"
                        value={detailPanelState.eligForm.verifiedDate}
                        onChange={e => detailPanelState.setEligForm(f => ({ ...f, verifiedDate: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field full">
                      <label className="form-label">Notes (optional)</label>
                      <input
                        className="form-input"
                        value={detailPanelState.eligForm.notes}
                        onChange={e => detailPanelState.setEligForm(f => ({ ...f, notes: e.target.value }))}
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={detailPanelState.eligPending}>
                    {detailPanelState.eligPending ? 'Saving…' : 'Record Eligibility'}
                  </button>
                </form>
              </TrackRow>

              <TrackRow label="Financing" complete={!!detail.financingDetails}>
                <form onSubmit={e => detailPanelState.handleFin(e, detailPanelState.finForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Lender Name</label>
                      <input
                        className="form-input"
                        value={detailPanelState.finForm.lenderName}
                        onChange={e => detailPanelState.setFinForm(f => ({ ...f, lenderName: e.target.value }))}
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
                        value={detailPanelState.finForm.committedAmount || ''}
                        onChange={e => detailPanelState.setFinForm(f => ({ ...f, committedAmount: parseFloat(e.target.value) || 0 }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label">Pre-Approval Letter Date</label>
                      <input
                        className="form-input"
                        type="date"
                        value={detailPanelState.finForm.preapprovalLetterDate}
                        onChange={e => detailPanelState.setFinForm(f => ({ ...f, preapprovalLetterDate: e.target.value }))}
                        required
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={detailPanelState.finPending}>
                    {detailPanelState.finPending ? 'Saving…' : 'Record Financing'}
                  </button>
                </form>
              </TrackRow>

              <TrackRow label="Inspection" complete={!!detail.inspectionResult}>
                <form onSubmit={e => detailPanelState.handleInsp(e, detailPanelState.inspForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Inspection Date</label>
                      <input
                        className="form-input"
                        type="date"
                        value={detailPanelState.inspForm.inspectionDate}
                        onChange={e => detailPanelState.setInspForm(f => ({ ...f, inspectionDate: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label">Inspector Name</label>
                      <input
                        className="form-input"
                        value={detailPanelState.inspForm.inspectorName}
                        onChange={e => detailPanelState.setInspForm(f => ({ ...f, inspectorName: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field full">
                      <label className="form-label">Notes (optional)</label>
                      <input
                        className="form-input"
                        value={detailPanelState.inspForm.notes}
                        onChange={e => detailPanelState.setInspForm(f => ({ ...f, notes: e.target.value }))}
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={detailPanelState.inspPending}>
                    {detailPanelState.inspPending ? 'Saving…' : 'Record Inspection'}
                  </button>
                </form>
              </TrackRow>

              <TrackRow label="Title" complete={!!detail.titleDetails}>
                <form onSubmit={e => detailPanelState.handleTitle(e, detailPanelState.titleForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Cleared Date</label>
                      <input
                        className="form-input"
                        type="date"
                        value={detailPanelState.titleForm.clearedDate}
                        onChange={e => detailPanelState.setTitleForm(f => ({ ...f, clearedDate: e.target.value }))}
                        required
                      />
                    </div>
                    <div className="form-field">
                      <label className="form-label">Title Company (optional)</label>
                      <input
                        className="form-input"
                        value={detailPanelState.titleForm.titleCompanyName}
                        onChange={e => detailPanelState.setTitleForm(f => ({ ...f, titleCompanyName: e.target.value }))}
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={detailPanelState.titlePending}>
                    {detailPanelState.titlePending ? 'Saving…' : 'Record Title Cleared'}
                  </button>
                </form>
              </TrackRow>

              <TrackRow label="Lease Execution" complete={!!detail.leaseExecutionDetails}>
                <form onSubmit={e => detailPanelState.handleLeaseEx(e, detailPanelState.leaseExecForm)}>
                  <div className="form-grid" style={{ marginBottom: 10 }}>
                    <div className="form-field">
                      <label className="form-label">Signed Date</label>
                      <input
                        className="form-input"
                        type="date"
                        value={detailPanelState.leaseExecForm.signedDate}
                        onChange={e => detailPanelState.setLeaseExecForm(f => ({ ...f, signedDate: e.target.value }))}
                        required
                      />
                    </div>
                  </div>
                  <button type="submit" className="btn btn-secondary btn-sm" disabled={detailPanelState.leaseExecPending}>
                    {detailPanelState.leaseExecPending ? 'Saving…' : 'Record Lease Executed'}
                  </button>
                </form>
              </TrackRow>
            </div>
          )}

          {detailPanelState.canSchedule && (
            <div className="detail-section">
              <div className="detail-section__title">Schedule Closing</div>
              <form onSubmit={e => detailPanelState.handleSchedule(e, detailPanelState.scheduleForm)}>
                <div className="form-grid" style={{ marginBottom: 12 }}>
                  <div className="form-field">
                    <label className="form-label">Scheduled Date</label>
                    <input
                      className="form-input"
                      type="date"
                      value={detailPanelState.scheduleForm.scheduledDate}
                      onChange={e => detailPanelState.setScheduleForm(f => ({ ...f, scheduledDate: e.target.value }))}
                      required
                    />
                  </div>
                </div>
                <button type="submit" className="btn btn-primary" disabled={detailPanelState.schedulePending}>
                  {detailPanelState.schedulePending ? 'Scheduling…' : 'Schedule Closing'}
                </button>
                {detailPanelState.scheduleError && <div className="form-error">Failed to schedule closing.</div>}
              </form>
            </div>
          )}

          {detailPanelState.canClose && (
            <div className="detail-section">
              <div className="detail-section__title">Record Closing</div>
              <form onSubmit={e => detailPanelState.handleClose(e, detailPanelState.closeForm)}>
                <div className="form-grid" style={{ marginBottom: 12 }}>
                  <div className="form-field">
                    <label className="form-label">Actual Closing Date</label>
                    <input
                      className="form-input"
                      type="date"
                      value={detailPanelState.closeForm.actualClosingDate}
                      onChange={e => detailPanelState.setCloseForm(f => ({ ...f, actualClosingDate: e.target.value }))}
                      required
                    />
                  </div>
                  <div className="form-field full">
                    <label className="form-label">Closing Notes (optional)</label>
                    <textarea
                      className="form-textarea"
                      value={detailPanelState.closeForm.closingNotes}
                      onChange={e => detailPanelState.setCloseForm(f => ({ ...f, closingNotes: e.target.value }))}
                    />
                  </div>
                </div>
                <button type="submit" className="btn btn-primary" disabled={detailPanelState.closePending}>
                  {detailPanelState.closePending ? 'Recording…' : 'Record Purchase Closed'}
                </button>
                {detailPanelState.closeError && <div className="form-error">Failed to record closing.</div>}
              </form>
            </div>
          )}

          {detailPanelState.canAct && (
            <div className="detail-section">
              <div className="detail-section__title">Workflow Actions</div>

              {detailPanelState.hasWorkflowAction('workflow:fail') && (
              <form onSubmit={e => detailPanelState.handleFail(e, detailPanelState.failForm)} style={{ marginBottom: 16 }}>
                <div className="form-grid" style={{ marginBottom: 8 }}>
                  <div className="form-field full">
                    <label className="form-label">Fail Reason</label>
                    <input
                      className="form-input"
                      value={detailPanelState.failForm.reason}
                      onChange={e => detailPanelState.setFailForm(f => ({ ...f, reason: e.target.value }))}
                      placeholder="Why is this purchase failing?"
                      required
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  className="btn btn-ghost"
                  style={{ color: 'var(--red)', borderColor: 'var(--red-dim)' }}
                  disabled={detailPanelState.failPending}
                >
                  {detailPanelState.failPending ? 'Failing…' : 'Mark as Failed'}
                </button>
              </form>
              )}

              {detailPanelState.hasWorkflowAction('workflow:withdraw') && (
              <form onSubmit={e => detailPanelState.handleWithdraw(e, detailPanelState.withdrawForm)} style={{ marginBottom: 16 }}>
                <div className="form-grid" style={{ marginBottom: 8 }}>
                  <div className="form-field full">
                    <label className="form-label">Withdrawal Reason</label>
                    <input
                      className="form-input"
                      value={detailPanelState.withdrawForm.reason}
                      onChange={e => detailPanelState.setWithdrawForm(f => ({ ...f, reason: e.target.value }))}
                      placeholder="Why is the buyer withdrawing?"
                      required
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  className="btn btn-ghost"
                  disabled={detailPanelState.withdrawPending}
                >
                  {detailPanelState.withdrawPending ? 'Withdrawing…' : 'Withdraw Purchase'}
                </button>
              </form>
              )}

              {detailPanelState.hasWorkflowAction('workflow:stall') && (
              <form onSubmit={e => detailPanelState.handleStall(e, detailPanelState.stallForm)}>
                <div className="form-grid" style={{ marginBottom: 8 }}>
                  <div className="form-field full">
                    <label className="form-label">Stall Reason</label>
                    <input
                      className="form-input"
                      value={detailPanelState.stallForm.reason}
                      onChange={e => detailPanelState.setStallForm(f => ({ ...f, reason: e.target.value }))}
                      placeholder="What is blocking progress?"
                      required
                    />
                  </div>
                  <div className="form-field">
                    <label className="form-label">Resolution Target Date (optional)</label>
                    <input
                      className="form-input"
                      type="date"
                      value={detailPanelState.stallForm.resolutionTargetDate}
                      onChange={e => detailPanelState.setStallForm(f => ({ ...f, resolutionTargetDate: e.target.value }))}
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  className="btn btn-ghost"
                  style={{ color: 'var(--orange-text)' }}
                  disabled={detailPanelState.stallPending}
                >
                  {detailPanelState.stallPending ? 'Stalling…' : 'Stall Workflow'}
                </button>
              </form>
              )}
            </div>
          )}
        </>
      )}
    </SlideOver>
  )
}
