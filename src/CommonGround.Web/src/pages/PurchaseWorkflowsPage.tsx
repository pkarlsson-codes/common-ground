import { useCallback, useState } from 'react'
import type { PurchaseWorkflowResponse } from '../api'
import {
  useFailPurchase,
  useInitiatePurchaseWorkflow,
  usePurchaseWorkflow,
  usePurchaseWorkflows,
  useRecordEligibilityVerified,
  useRecordFinancingCommitted,
  useRecordInspectionCompleted,
  useRecordLeaseExecuted,
  useRecordPurchaseClosed,
  useRecordTitleCleared,
  useScheduleClosing,
  useStallWorkflow,
  useWithdrawPurchase,
} from '../hooks/purchaseWorkflows'
import {
  statusLabel,
  WORKFLOW_PHASES,
  type FailWorkflowForm,
  type InitiateWorkflowForm,
  type RecordClosedForm,
  type RecordEligibilityForm,
  type RecordFinancingForm,
  type RecordInspectionForm,
  type RecordLeaseExecutedForm,
  type RecordTitleForm,
  type ScheduleClosingForm,
  type StallWorkflowForm,
  type WithdrawWorkflowForm,
} from '../hooks/types'
import Pagination from '../components/Pagination'
import SlideOver from '../components/SlideOver'
import StatusBadge from '../components/StatusBadge'

type Panel = 'detail' | 'initiate' | null

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const emptyInitiate = (): InitiateWorkflowForm => ({
  groundLeaseId: '',
  buyerFullName: '',
  buyerEmail: '',
  buyerPhone: '',
  purchasePrice: 150000,
})

const today = () => new Date().toISOString().slice(0, 10)

const emptyEligibility = (): RecordEligibilityForm => ({ verifiedDate: today(), notes: '' })
const emptyFinancing = (): RecordFinancingForm => ({ lenderName: '', committedAmount: 0, preapprovalLetterDate: today() })
const emptyInspection = (): RecordInspectionForm => ({ inspectionDate: today(), inspectorName: '', notes: '' })
const emptyTitle = (): RecordTitleForm => ({ clearedDate: today(), titleCompanyName: '' })
const emptyLeaseExec = (): RecordLeaseExecutedForm => ({ signedDate: today() })
const emptySchedule = (): ScheduleClosingForm => ({ scheduledDate: today() })
const emptyClose = (): RecordClosedForm => ({ actualClosingDate: today(), closingNotes: '' })
const emptyFail = (): FailWorkflowForm => ({ reason: '' })
const emptyWithdraw = (): WithdrawWorkflowForm => ({ reason: '' })
const emptyStall = (): StallWorkflowForm => ({ reason: '', resolutionTargetDate: '' })

const isTerminal = (phase?: string) =>
  phase === 'Closed' || phase === 'Failed' || phase === 'Withdrawn'

const allTracksComplete = (wf: PurchaseWorkflowResponse) =>
  !!wf.eligibilityDetails &&
  !!wf.financingDetails &&
  !!wf.inspectionResult &&
  !!wf.titleDetails &&
  !!wf.leaseExecutionDetails

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

export default function PurchaseWorkflowsPage() {
  const [page, setPage] = useState(1)
  const [phaseFilter, setPhaseFilter] = useState('')
  const [groundLeaseFilter, setGroundLeaseFilter] = useState('')
  const [groundLeaseInput, setGroundLeaseInput] = useState('')
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [panel, setPanel] = useState<Panel>(null)

  const [initiateForm, setInitiateForm] = useState<InitiateWorkflowForm>(emptyInitiate)
  const [eligForm, setEligForm] = useState<RecordEligibilityForm>(emptyEligibility)
  const [finForm, setFinForm] = useState<RecordFinancingForm>(emptyFinancing)
  const [inspForm, setInspForm] = useState<RecordInspectionForm>(emptyInspection)
  const [titleForm, setTitleForm] = useState<RecordTitleForm>(emptyTitle)
  const [leaseExecForm, setLeaseExecForm] = useState<RecordLeaseExecutedForm>(emptyLeaseExec)
  const [scheduleForm, setScheduleForm] = useState<ScheduleClosingForm>(emptySchedule)
  const [closeForm, setCloseForm] = useState<RecordClosedForm>(emptyClose)
  const [failForm, setFailForm] = useState<FailWorkflowForm>(emptyFail)
  const [withdrawForm, setWithdrawForm] = useState<WithdrawWorkflowForm>(emptyWithdraw)
  const [stallForm, setStallForm] = useState<StallWorkflowForm>(emptyStall)

  const [toast, setToast] = useState<string | null>(null)
  const [toastKind, setToastKind] = useState<'success' | 'error'>('success')

  const { data, isLoading, isError } = usePurchaseWorkflows(page, groundLeaseFilter || undefined, phaseFilter || undefined)
  const { data: detail, isLoading: detailLoading } = usePurchaseWorkflow(
    panel === 'detail' ? selectedId : null,
  )

  const initiateMutation     = useInitiatePurchaseWorkflow()
  const eligMutation         = useRecordEligibilityVerified()
  const finMutation          = useRecordFinancingCommitted()
  const inspMutation         = useRecordInspectionCompleted()
  const titleMutation        = useRecordTitleCleared()
  const leaseExecMutation    = useRecordLeaseExecuted()
  const scheduleMutation     = useScheduleClosing()
  const closeMutation        = useRecordPurchaseClosed()
  const failMutation         = useFailPurchase()
  const withdrawMutation     = useWithdrawPurchase()
  const stallMutation        = useStallWorkflow()

  const showToast = useCallback((msg: string, kind: 'success' | 'error' = 'success') => {
    setToast(msg)
    setToastKind(kind)
    setTimeout(() => setToast(null), 3200)
  }, [])

  const openDetail = (id: string) => {
    setSelectedId(id)
    setEligForm(emptyEligibility())
    setFinForm(emptyFinancing())
    setInspForm(emptyInspection())
    setTitleForm(emptyTitle())
    setLeaseExecForm(emptyLeaseExec())
    setScheduleForm(emptySchedule())
    setCloseForm(emptyClose())
    setFailForm(emptyFail())
    setWithdrawForm(emptyWithdraw())
    setStallForm(emptyStall())
    setPanel('detail')
  }

  const openInitiate = () => {
    setInitiateForm(emptyInitiate())
    setPanel('initiate')
  }

  const closePanel = () => {
    setPanel(null)
    setSelectedId(null)
  }

  const handleInitiate = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await initiateMutation.mutateAsync({ ...initiateForm, purchasePrice: Number(initiateForm.purchasePrice) })
      closePanel()
      showToast('Purchase workflow initiated')
    } catch { /* error shown inline */ }
  }

  const makeTrackHandler = <T,>(
    mutate: (args: { workflowId: string; form: T }) => Promise<unknown>,
    successMsg: string,
  ) => async (e: React.FormEvent, form: T) => {
    e.preventDefault()
    if (!selectedId) return
    try {
      await mutate({ workflowId: selectedId, form })
      showToast(successMsg)
    } catch { showToast('Action failed', 'error') }
  }

  const handleElig     = makeTrackHandler(eligMutation.mutateAsync,      'Eligibility recorded')
  const handleFin      = makeTrackHandler(finMutation.mutateAsync,       'Financing committed')
  const handleInsp     = makeTrackHandler(inspMutation.mutateAsync,      'Inspection recorded')
  const handleTitle    = makeTrackHandler(titleMutation.mutateAsync,     'Title cleared')
  const handleLeaseEx  = makeTrackHandler(leaseExecMutation.mutateAsync, 'Lease executed')
  const handleSchedule = makeTrackHandler(scheduleMutation.mutateAsync,  'Closing scheduled')
  const handleClose    = makeTrackHandler(closeMutation.mutateAsync,     'Purchase closed')
  const handleFail     = makeTrackHandler(failMutation.mutateAsync,      'Workflow marked failed')
  const handleWithdraw = makeTrackHandler(withdrawMutation.mutateAsync,  'Workflow withdrawn')
  const handleStall    = makeTrackHandler(stallMutation.mutateAsync,     'Workflow stalled')

  const applyGLFilter = (e: React.FormEvent) => {
    e.preventDefault()
    setGroundLeaseFilter(groundLeaseInput.trim())
    setPage(1)
  }

  const clearGLFilter = () => {
    setGroundLeaseFilter('')
    setGroundLeaseInput('')
    setPage(1)
  }

  const workflows = data?.items ?? []
  const canSchedule = detail && allTracksComplete(detail) && detail.phase === 'InProgress'
  const canClose    = detail?.phase === 'ClosingScheduled'
  const canAct      = detail && !isTerminal(detail.phase)

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <div className="page-header__eyebrow">Homeownership</div>
          <h1 className="page-header__title">Purchase Workflows</h1>
          {data && (
            <div className="page-header__count">
              {data.totalCount?.toLocaleString()} workflow{data.totalCount !== 1 ? 's' : ''}
            </div>
          )}
        </div>
        <button className="btn btn-primary" onClick={openInitiate}>
          <svg width="13" height="13" viewBox="0 0 13 13" fill="none">
            <path d="M6.5 1v11M1 6.5h11" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"/>
          </svg>
          Initiate Workflow
        </button>
      </div>

      <div className="filter-bar">
        <select
          className="select"
          value={phaseFilter}
          onChange={e => { setPhaseFilter(e.target.value); setPage(1) }}
        >
          <option value="">All Phases</option>
          {WORKFLOW_PHASES.map(p => (
            <option key={p} value={p}>{statusLabel(p)}</option>
          ))}
        </select>

        <form onSubmit={applyGLFilter} style={{ display: 'flex', gap: 6 }}>
          <input
            className="form-input"
            style={{ width: 280 }}
            placeholder="Filter by ground lease ID…"
            value={groundLeaseInput}
            onChange={e => setGroundLeaseInput(e.target.value)}
          />
          <button type="submit" className="btn btn-secondary btn-sm">Filter</button>
          {groundLeaseFilter && (
            <button type="button" className="btn btn-ghost btn-sm" onClick={clearGLFilter}>
              Clear
            </button>
          )}
        </form>
      </div>

      <div className="registry-table-wrap">
        {isLoading && <SkeletonRows />}

        {isError && (
          <div className="error-state">
            <div className="error-state__title">Could not load purchase workflows</div>
            <div className="error-state__sub">Ensure the API and gateway are running.</div>
          </div>
        )}

        {!isLoading && !isError && workflows.length === 0 && (
          <div className="empty-state">
            <div className="empty-state__glyph">⊡</div>
            <div className="empty-state__title">No workflows found</div>
            <div className="empty-state__sub">Initiate a purchase workflow on an active ground lease.</div>
          </div>
        )}

        {!isLoading && !isError && workflows.length > 0 && (
          <>
            <table className="registry-table">
              <thead>
                <tr>
                  <th>Buyer</th>
                  <th>Purchase Price</th>
                  <th>Phase</th>
                  <th>Ground Lease</th>
                  <th>Created</th>
                </tr>
              </thead>
              <tbody>
                {workflows.map(w => (
                  <tr
                    key={w.id}
                    onClick={() => openDetail(w.id!)}
                    className={selectedId === w.id && panel === 'detail' ? 'row--selected' : ''}
                  >
                    <td>{w.buyerFullName}</td>
                    <td><span className="cell-mono">{fmtCurrency(Number(w.purchasePrice))}</span></td>
                    <td><StatusBadge status={w.phase ?? ''} /></td>
                    <td>
                      <span className="cell-id" title={w.groundLeaseId}>
                        {w.groundLeaseId?.slice(0, 8)}…
                      </span>
                    </td>
                    <td><span className="cell-mono">{new Date(w.createdAt!).toLocaleDateString()}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
            {data && (data.totalPages ?? 1) > 1 && (
              <Pagination
                page={page}
                totalPages={Number(data.totalPages)}
                totalCount={Number(data.totalCount)}
                pageSize={20}
                onPage={setPage}
              />
            )}
          </>
        )}
      </div>

      {/* ── Detail panel ─── */}
      {panel === 'detail' && (
        <SlideOver
          title={detail?.buyerFullName ?? 'Purchase Workflow'}
          subtitle={detail ? `${fmtCurrency(Number(detail.purchasePrice))} · ${statusLabel(detail.phase ?? '')}` : undefined}
          onClose={closePanel}
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
                  <div className="detail-field__label">Ground Lease ID</div>
                  <div className="detail-field__value"><span className="cell-id">{detail.groundLeaseId}</span></div>
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

              {/* ── 5 parallel tracks ─── */}
              {canAct && (
                <div className="detail-section">
                  <div className="detail-section__title">Purchase Tracks</div>
                  <div style={{ marginBottom: 4, fontSize: 12, color: 'var(--text-secondary)' }}>
                    All five tracks must be complete before closing can be scheduled.
                  </div>

                  {/* Eligibility */}
                  <TrackRow label="Eligibility" complete={!!detail.eligibilityDetails}>
                    <form onSubmit={e => handleElig(e, eligForm)}>
                      <div className="form-grid" style={{ marginBottom: 10 }}>
                        <div className="form-field">
                          <label className="form-label">Verified Date</label>
                          <input
                            className="form-input"
                            type="date"
                            value={eligForm.verifiedDate}
                            onChange={e => setEligForm(f => ({ ...f, verifiedDate: e.target.value }))}
                            required
                          />
                        </div>
                        <div className="form-field full">
                          <label className="form-label">Notes (optional)</label>
                          <input
                            className="form-input"
                            value={eligForm.notes}
                            onChange={e => setEligForm(f => ({ ...f, notes: e.target.value }))}
                          />
                        </div>
                      </div>
                      <button type="submit" className="btn btn-secondary btn-sm" disabled={eligMutation.isPending}>
                        {eligMutation.isPending ? 'Saving…' : 'Record Eligibility'}
                      </button>
                    </form>
                  </TrackRow>

                  {/* Financing */}
                  <TrackRow label="Financing" complete={!!detail.financingDetails}>
                    <form onSubmit={e => handleFin(e, finForm)}>
                      <div className="form-grid" style={{ marginBottom: 10 }}>
                        <div className="form-field">
                          <label className="form-label">Lender Name</label>
                          <input
                            className="form-input"
                            value={finForm.lenderName}
                            onChange={e => setFinForm(f => ({ ...f, lenderName: e.target.value }))}
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
                            value={finForm.committedAmount || ''}
                            onChange={e => setFinForm(f => ({ ...f, committedAmount: parseFloat(e.target.value) || 0 }))}
                            required
                          />
                        </div>
                        <div className="form-field">
                          <label className="form-label">Pre-Approval Letter Date</label>
                          <input
                            className="form-input"
                            type="date"
                            value={finForm.preapprovalLetterDate}
                            onChange={e => setFinForm(f => ({ ...f, preapprovalLetterDate: e.target.value }))}
                            required
                          />
                        </div>
                      </div>
                      <button type="submit" className="btn btn-secondary btn-sm" disabled={finMutation.isPending}>
                        {finMutation.isPending ? 'Saving…' : 'Record Financing'}
                      </button>
                    </form>
                  </TrackRow>

                  {/* Inspection */}
                  <TrackRow label="Inspection" complete={!!detail.inspectionResult}>
                    <form onSubmit={e => handleInsp(e, inspForm)}>
                      <div className="form-grid" style={{ marginBottom: 10 }}>
                        <div className="form-field">
                          <label className="form-label">Inspection Date</label>
                          <input
                            className="form-input"
                            type="date"
                            value={inspForm.inspectionDate}
                            onChange={e => setInspForm(f => ({ ...f, inspectionDate: e.target.value }))}
                            required
                          />
                        </div>
                        <div className="form-field">
                          <label className="form-label">Inspector Name</label>
                          <input
                            className="form-input"
                            value={inspForm.inspectorName}
                            onChange={e => setInspForm(f => ({ ...f, inspectorName: e.target.value }))}
                            required
                          />
                        </div>
                        <div className="form-field full">
                          <label className="form-label">Notes (optional)</label>
                          <input
                            className="form-input"
                            value={inspForm.notes}
                            onChange={e => setInspForm(f => ({ ...f, notes: e.target.value }))}
                          />
                        </div>
                      </div>
                      <button type="submit" className="btn btn-secondary btn-sm" disabled={inspMutation.isPending}>
                        {inspMutation.isPending ? 'Saving…' : 'Record Inspection'}
                      </button>
                    </form>
                  </TrackRow>

                  {/* Title */}
                  <TrackRow label="Title" complete={!!detail.titleDetails}>
                    <form onSubmit={e => handleTitle(e, titleForm)}>
                      <div className="form-grid" style={{ marginBottom: 10 }}>
                        <div className="form-field">
                          <label className="form-label">Cleared Date</label>
                          <input
                            className="form-input"
                            type="date"
                            value={titleForm.clearedDate}
                            onChange={e => setTitleForm(f => ({ ...f, clearedDate: e.target.value }))}
                            required
                          />
                        </div>
                        <div className="form-field">
                          <label className="form-label">Title Company (optional)</label>
                          <input
                            className="form-input"
                            value={titleForm.titleCompanyName}
                            onChange={e => setTitleForm(f => ({ ...f, titleCompanyName: e.target.value }))}
                          />
                        </div>
                      </div>
                      <button type="submit" className="btn btn-secondary btn-sm" disabled={titleMutation.isPending}>
                        {titleMutation.isPending ? 'Saving…' : 'Record Title Cleared'}
                      </button>
                    </form>
                  </TrackRow>

                  {/* Lease Execution */}
                  <TrackRow label="Lease Execution" complete={!!detail.leaseExecutionDetails}>
                    <form onSubmit={e => handleLeaseEx(e, leaseExecForm)}>
                      <div className="form-grid" style={{ marginBottom: 10 }}>
                        <div className="form-field">
                          <label className="form-label">Signed Date</label>
                          <input
                            className="form-input"
                            type="date"
                            value={leaseExecForm.signedDate}
                            onChange={e => setLeaseExecForm(f => ({ ...f, signedDate: e.target.value }))}
                            required
                          />
                        </div>
                      </div>
                      <button type="submit" className="btn btn-secondary btn-sm" disabled={leaseExecMutation.isPending}>
                        {leaseExecMutation.isPending ? 'Saving…' : 'Record Lease Executed'}
                      </button>
                    </form>
                  </TrackRow>
                </div>
              )}

              {/* ── Schedule closing ─── */}
              {canSchedule && (
                <div className="detail-section">
                  <div className="detail-section__title">Schedule Closing</div>
                  <form onSubmit={e => handleSchedule(e, scheduleForm)}>
                    <div className="form-grid" style={{ marginBottom: 12 }}>
                      <div className="form-field">
                        <label className="form-label">Scheduled Date</label>
                        <input
                          className="form-input"
                          type="date"
                          value={scheduleForm.scheduledDate}
                          onChange={e => setScheduleForm(f => ({ ...f, scheduledDate: e.target.value }))}
                          required
                        />
                      </div>
                    </div>
                    <button type="submit" className="btn btn-primary" disabled={scheduleMutation.isPending}>
                      {scheduleMutation.isPending ? 'Scheduling…' : 'Schedule Closing'}
                    </button>
                    {scheduleMutation.isError && <div className="form-error">Failed to schedule closing.</div>}
                  </form>
                </div>
              )}

              {/* ── Record closing ─── */}
              {canClose && (
                <div className="detail-section">
                  <div className="detail-section__title">Record Closing</div>
                  <form onSubmit={e => handleClose(e, closeForm)}>
                    <div className="form-grid" style={{ marginBottom: 12 }}>
                      <div className="form-field">
                        <label className="form-label">Actual Closing Date</label>
                        <input
                          className="form-input"
                          type="date"
                          value={closeForm.actualClosingDate}
                          onChange={e => setCloseForm(f => ({ ...f, actualClosingDate: e.target.value }))}
                          required
                        />
                      </div>
                      <div className="form-field full">
                        <label className="form-label">Closing Notes (optional)</label>
                        <textarea
                          className="form-textarea"
                          value={closeForm.closingNotes}
                          onChange={e => setCloseForm(f => ({ ...f, closingNotes: e.target.value }))}
                        />
                      </div>
                    </div>
                    <button type="submit" className="btn btn-primary" disabled={closeMutation.isPending}>
                      {closeMutation.isPending ? 'Recording…' : 'Record Purchase Closed'}
                    </button>
                    {closeMutation.isError && <div className="form-error">Failed to record closing.</div>}
                  </form>
                </div>
              )}

              {/* ── Terminal actions ─── */}
              {canAct && (
                <div className="detail-section">
                  <div className="detail-section__title">Workflow Actions</div>

                  <form onSubmit={e => handleFail(e, failForm)} style={{ marginBottom: 16 }}>
                    <div className="form-grid" style={{ marginBottom: 8 }}>
                      <div className="form-field full">
                        <label className="form-label">Fail Reason</label>
                        <input
                          className="form-input"
                          value={failForm.reason}
                          onChange={e => setFailForm(f => ({ ...f, reason: e.target.value }))}
                          placeholder="Why is this purchase failing?"
                          required
                        />
                      </div>
                    </div>
                    <button
                      type="submit"
                      className="btn btn-ghost"
                      style={{ color: 'var(--red)', borderColor: 'var(--red-dim)' }}
                      disabled={failMutation.isPending}
                    >
                      {failMutation.isPending ? 'Failing…' : 'Mark as Failed'}
                    </button>
                  </form>

                  <form onSubmit={e => handleWithdraw(e, withdrawForm)} style={{ marginBottom: 16 }}>
                    <div className="form-grid" style={{ marginBottom: 8 }}>
                      <div className="form-field full">
                        <label className="form-label">Withdrawal Reason</label>
                        <input
                          className="form-input"
                          value={withdrawForm.reason}
                          onChange={e => setWithdrawForm(f => ({ ...f, reason: e.target.value }))}
                          placeholder="Why is the buyer withdrawing?"
                          required
                        />
                      </div>
                    </div>
                    <button
                      type="submit"
                      className="btn btn-ghost"
                      disabled={withdrawMutation.isPending}
                    >
                      {withdrawMutation.isPending ? 'Withdrawing…' : 'Withdraw Purchase'}
                    </button>
                  </form>

                  <form onSubmit={e => handleStall(e, stallForm)}>
                    <div className="form-grid" style={{ marginBottom: 8 }}>
                      <div className="form-field full">
                        <label className="form-label">Stall Reason</label>
                        <input
                          className="form-input"
                          value={stallForm.reason}
                          onChange={e => setStallForm(f => ({ ...f, reason: e.target.value }))}
                          placeholder="What is blocking progress?"
                          required
                        />
                      </div>
                      <div className="form-field">
                        <label className="form-label">Resolution Target Date (optional)</label>
                        <input
                          className="form-input"
                          type="date"
                          value={stallForm.resolutionTargetDate}
                          onChange={e => setStallForm(f => ({ ...f, resolutionTargetDate: e.target.value }))}
                        />
                      </div>
                    </div>
                    <button
                      type="submit"
                      className="btn btn-ghost"
                      style={{ color: 'var(--orange-text)' }}
                      disabled={stallMutation.isPending}
                    >
                      {stallMutation.isPending ? 'Stalling…' : 'Stall Workflow'}
                    </button>
                  </form>
                </div>
              )}
            </>
          )}
        </SlideOver>
      )}

      {/* ── Initiate panel ─── */}
      {panel === 'initiate' && (
        <SlideOver
          title="Initiate Purchase Workflow"
          subtitle="Start a new purchase process on an active ground lease"
          onClose={closePanel}
          footer={
            <>
              <button className="btn btn-ghost" onClick={closePanel}>Cancel</button>
              <button
                className="btn btn-primary"
                type="submit"
                form="initiate-workflow-form"
                disabled={initiateMutation.isPending}
              >
                {initiateMutation.isPending ? 'Initiating…' : 'Initiate Workflow'}
              </button>
            </>
          }
        >
          <form id="initiate-workflow-form" onSubmit={handleInitiate}>
            <div className="form-grid">
              <div className="form-field full">
                <label className="form-label">Ground Lease ID</label>
                <input
                  className="form-input"
                  value={initiateForm.groundLeaseId}
                  onChange={e => setInitiateForm(f => ({ ...f, groundLeaseId: e.target.value }))}
                  placeholder="Paste the ground lease UUID"
                  required
                />
              </div>
              <div className="form-field full">
                <label className="form-label">Buyer Full Name</label>
                <input
                  className="form-input"
                  value={initiateForm.buyerFullName}
                  onChange={e => setInitiateForm(f => ({ ...f, buyerFullName: e.target.value }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Buyer Email</label>
                <input
                  className="form-input"
                  type="email"
                  value={initiateForm.buyerEmail}
                  onChange={e => setInitiateForm(f => ({ ...f, buyerEmail: e.target.value }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Buyer Phone (optional)</label>
                <input
                  className="form-input"
                  value={initiateForm.buyerPhone}
                  onChange={e => setInitiateForm(f => ({ ...f, buyerPhone: e.target.value }))}
                />
              </div>
              <div className="form-field full">
                <label className="form-label">Purchase Price ($)</label>
                <input
                  className="form-input"
                  type="number"
                  min="0"
                  step="1"
                  value={initiateForm.purchasePrice}
                  onChange={e => setInitiateForm(f => ({ ...f, purchasePrice: parseFloat(e.target.value) || 0 }))}
                  required
                />
              </div>
            </div>
            {initiateMutation.isError && (
              <div className="form-error">Failed to initiate workflow. Check the ground lease ID.</div>
            )}
          </form>
        </SlideOver>
      )}

      {toast && (
        <div className={`toast toast--${toastKind}`}>
          {toastKind === 'success' ? (
            <svg width="15" height="15" viewBox="0 0 15 15" fill="none">
              <circle cx="7.5" cy="7.5" r="6.5" stroke="#4ec48a" strokeWidth="1.4"/>
              <path d="M4.5 7.5l2 2 4-4" stroke="#4ec48a" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round"/>
            </svg>
          ) : (
            <svg width="15" height="15" viewBox="0 0 15 15" fill="none">
              <circle cx="7.5" cy="7.5" r="6.5" stroke="#c44040" strokeWidth="1.4"/>
              <path d="M5 5l5 5M10 5l-5 5" stroke="#c44040" strokeWidth="1.4" strokeLinecap="round"/>
            </svg>
          )}
          {toast}
        </div>
      )}
    </div>
  )
}
