import { useState } from 'react'
import StatusBadge from '../StatusBadge'
import {
  useRecordServicerDelinquency,
  useMakeCounselingReferral,
  useRecordCounselingOutcome,
  useRequestForbearance,
  useRecordForbearanceOutcome,
  useActivateCurePeriod,
  useResolveForeclosurePreventionCase,
} from '../../hooks/stewardship'
import type { ForeclosurePreventionCaseResponse } from '../../api/models'
import { useToast } from '../../hooks/useToast'

const fmtD = (d: Date | null | undefined) => d ? d.toISOString().slice(0, 10) : undefined

interface Props {
  fpc: ForeclosurePreventionCaseResponse
  groundLeaseId: string
}

const today = () => new Date().toISOString().slice(0, 10)

const OUTCOMES = [
  { value: 'InterventionSuccessful', label: 'Intervention Successful' },
  { value: 'HomeownerSold', label: 'Homeowner Sold' },
  { value: 'CurePeriodExercised', label: 'Cure Period Exercised' },
  { value: 'LeaseTerminated', label: 'Lease Terminated' },
]

const COUNSELING_OUTCOMES = ['CounselingCompleted', 'HomeownerUnresponsive', 'NotEligible']
const FORBEARANCE_OUTCOMES = ['Granted', 'Denied', 'Withdrawn']

export default function ForeclosurePreventionCaseCard({
  fpc,
  groundLeaseId,
}: Props) {
  const [expanded, setExpanded] = useState(false)
  const { success, error: showError } = useToast()

  const delinquencyMutation = useRecordServicerDelinquency(groundLeaseId)
  const referralMutation = useMakeCounselingReferral(groundLeaseId)
  const referralOutcomeMutation = useRecordCounselingOutcome(groundLeaseId)
  const forbearanceMutation = useRequestForbearance(groundLeaseId)
  const forbearanceOutcomeMutation = useRecordForbearanceOutcome(groundLeaseId)
  const cureMutation = useActivateCurePeriod(groundLeaseId)
  const resolveMutation = useResolveForeclosurePreventionCase(groundLeaseId)

  // ── Local form state ──────────────────────────────────────────────────────
  const [delinquencyForm, setDelinquencyForm] = useState({
    servicerName: fpc.trigger ?? '',
    delinquentMonths: 1,
    recordedDate: today(),
  })

  const [referralForm, setReferralForm] = useState({ agencyName: '', referralDate: today() })
  const [forbearanceForm, setForbearanceForm] = useState({
    lenderName: '',
    requestedDate: today(),
  })
  const [cureForm, setCureForm] = useState({
    lraId: '',
    activationDate: today(),
    curePeriodDays: 90,
  })
  const [resolveForm, setResolveForm] = useState({
    outcome: 'InterventionSuccessful',
    resolvedDate: today(),
  })

  const isActive = fpc.status === 'Active' || fpc.status === 'Opened'

  const handleDelinquency = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await delinquencyMutation.mutateAsync({ caseId: fpc.id!, ...delinquencyForm })
      success('Delinquency recorded')
    } catch {
      showError('Failed to record delinquency')
    }
  }

  const handleReferral = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await referralMutation.mutateAsync({ caseId: fpc.id!, ...referralForm })
      success('Referral made')
      setReferralForm({ agencyName: '', referralDate: today() })
    } catch {
      showError('Failed to make referral')
    }
  }

  const handleForbearance = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await forbearanceMutation.mutateAsync({ caseId: fpc.id!, ...forbearanceForm })
      success('Forbearance requested')
      setForbearanceForm({ lenderName: '', requestedDate: today() })
    } catch {
      showError('Failed to request forbearance')
    }
  }

  const handleCure = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await cureMutation.mutateAsync({ caseId: fpc.id!, ...cureForm })
      success('Cure period activated')
      setCureForm({ lraId: '', activationDate: today(), curePeriodDays: 90 })
    } catch {
      showError('Failed to activate cure period')
    }
  }

  const handleResolve = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await resolveMutation.mutateAsync({ caseId: fpc.id!, ...resolveForm })
      success('Case resolved')
    } catch {
      showError('Failed to resolve case')
    }
  }

  return (
    <div
      style={{
        border: '1px solid var(--border)',
        borderRadius: 6,
        marginBottom: 12,
      }}
    >
      {/* Card header — always visible */}
      <div
        style={{
          padding: '12px 14px',
          cursor: 'pointer',
          userSelect: 'none',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
        onClick={() => setExpanded(!expanded)}
        aria-expanded={expanded}
      >
        <div>
          <div style={{ fontFamily: 'var(--font-mono)', fontSize: 12, marginBottom: 4 }}>
            {fpc.trigger} · <StatusBadge status={fpc.status ?? ''} />
          </div>
          <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
            Opened {fmtD(fpc.openedDate)}
            {fpc.resolvedDate && ` · Resolved ${fmtD(fpc.resolvedDate)}`}
          </div>
        </div>
        <span style={{ fontSize: 14, color: 'var(--text-dim)' }}>{expanded ? '↑' : '↓'}</span>
      </div>

      {/* Expanded detail */}
      {expanded && (
        <div style={{ padding: '0 14px 14px', borderTop: '1px solid var(--border)' }}>
          {/* Delinquencies */}
          <div style={{ marginTop: 12, marginBottom: 12 }}>
            <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 8 }}>Delinquencies</div>
            {isActive && (
              <form onSubmit={handleDelinquency} style={{ marginBottom: 12 }}>
                <div className="form-grid" style={{ marginBottom: 12 }}>
                  <div className="form-field">
                    <label className="form-label">Servicer Name</label>
                    <input
                      className="form-input"
                      value={delinquencyForm.servicerName}
                      onChange={e => setDelinquencyForm(f => ({ ...f, servicerName: e.target.value }))}
                      required
                    />
                  </div>
                  <div className="form-field">
                    <label className="form-label">Delinquent Months</label>
                    <input
                      className="form-input"
                      type="number"
                      min="1"
                      value={delinquencyForm.delinquentMonths}
                      onChange={e =>
                        setDelinquencyForm(f => ({
                          ...f,
                          delinquentMonths: parseInt(e.target.value) || 1,
                        }))
                      }
                      required
                    />
                  </div>
                  <div className="form-field">
                    <label className="form-label">Recorded Date</label>
                    <input
                      className="form-input"
                      type="date"
                      value={delinquencyForm.recordedDate}
                      onChange={e =>
                        setDelinquencyForm(f => ({ ...f, recordedDate: e.target.value }))
                      }
                      required
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  className="btn btn-secondary btn-sm"
                  disabled={delinquencyMutation.isPending}
                >
                  {delinquencyMutation.isPending ? 'Recording…' : 'Record Delinquency'}
                </button>
                {delinquencyMutation.isError && <div className="form-error">Failed.</div>}
              </form>
            )}
          </div>

          {/* Counseling Referrals */}
          <div style={{ marginBottom: 12 }}>
            <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 8 }}>Counseling Referrals</div>
            {isActive && (
              <form onSubmit={handleReferral} style={{ marginBottom: 12 }}>
                <div className="form-grid" style={{ marginBottom: 12 }}>
                  <div className="form-field">
                    <label className="form-label">Agency Name</label>
                    <input
                      className="form-input"
                      value={referralForm.agencyName}
                      onChange={e => setReferralForm(f => ({ ...f, agencyName: e.target.value }))}
                      required
                    />
                  </div>
                  <div className="form-field">
                    <label className="form-label">Referral Date</label>
                    <input
                      className="form-input"
                      type="date"
                      value={referralForm.referralDate}
                      onChange={e => setReferralForm(f => ({ ...f, referralDate: e.target.value }))}
                      required
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  className="btn btn-secondary btn-sm"
                  disabled={referralMutation.isPending}
                >
                  {referralMutation.isPending ? 'Recording…' : 'Make Referral'}
                </button>
                {referralMutation.isError && <div className="form-error">Failed.</div>}
              </form>
            )}
            {(fpc.counselingReferrals ?? []).map(r => (
              <div
                key={r.id}
                style={{
                  padding: '8px 10px',
                  background: 'var(--surface)',
                  borderRadius: 4,
                  fontSize: 12,
                  marginBottom: 6,
                }}
              >
                <div style={{ fontFamily: 'var(--font-mono)', marginBottom: 2 }}>
                  {r.agencyName}
                </div>
                <div style={{ fontSize: 11, color: 'var(--text-dim)', marginBottom: 4 }}>
                  Referred {fmtD(r.referralDate)}
                </div>
                {r.outcome ? (
                  <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                    Outcome: {r.outcome}
                  </div>
                ) : (
                  <ReferralOutcomeForm
                    caseId={fpc.id!}
                    referralId={r.id!}
                    mutation={referralOutcomeMutation}
                    showError={showError}
                    success={success}
                  />
                )}
              </div>
            ))}
          </div>

          {/* Forbearance Requests */}
          <div style={{ marginBottom: 12 }}>
            <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 8 }}>Forbearance Requests</div>
            {isActive && (
              <form onSubmit={handleForbearance} style={{ marginBottom: 12 }}>
                <div className="form-grid" style={{ marginBottom: 12 }}>
                  <div className="form-field">
                    <label className="form-label">Lender Name</label>
                    <input
                      className="form-input"
                      value={forbearanceForm.lenderName}
                      onChange={e =>
                        setForbearanceForm(f => ({ ...f, lenderName: e.target.value }))
                      }
                      required
                    />
                  </div>
                  <div className="form-field">
                    <label className="form-label">Requested Date</label>
                    <input
                      className="form-input"
                      type="date"
                      value={forbearanceForm.requestedDate}
                      onChange={e =>
                        setForbearanceForm(f => ({ ...f, requestedDate: e.target.value }))
                      }
                      required
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  className="btn btn-secondary btn-sm"
                  disabled={forbearanceMutation.isPending}
                >
                  {forbearanceMutation.isPending ? 'Recording…' : 'Request Forbearance'}
                </button>
                {forbearanceMutation.isError && <div className="form-error">Failed.</div>}
              </form>
            )}
            {(fpc.forbearanceRequests ?? []).map(req => (
              <div
                key={req.id}
                style={{
                  padding: '8px 10px',
                  background: 'var(--surface)',
                  borderRadius: 4,
                  fontSize: 12,
                  marginBottom: 6,
                }}
              >
                <div style={{ fontFamily: 'var(--font-mono)', marginBottom: 2 }}>
                  {req.lenderName}
                </div>
                <div style={{ fontSize: 11, color: 'var(--text-dim)', marginBottom: 4 }}>
                  Requested {fmtD(req.requestedDate)}
                </div>
                {req.outcome ? (
                  <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                    Outcome: {req.outcome}
                    {req.notes && ` — ${req.notes}`}
                  </div>
                ) : (
                  <ForbearanceOutcomeForm
                    caseId={fpc.id!}
                    requestId={req.id!}
                    mutation={forbearanceOutcomeMutation}
                    showError={showError}
                    success={success}
                  />
                )}
              </div>
            ))}
          </div>

          {/* Cure Periods */}
          <div style={{ marginBottom: 12 }}>
            <div style={{ fontSize: 12, fontWeight: 600, marginBottom: 8 }}>Cure Periods</div>
            {isActive && (
              <form onSubmit={handleCure} style={{ marginBottom: 12 }}>
                <div className="form-grid" style={{ marginBottom: 12 }}>
                  <div className="form-field">
                    <label className="form-label">LRA ID</label>
                    <input
                      className="form-input"
                      value={cureForm.lraId}
                      onChange={e => setCureForm(f => ({ ...f, lraId: e.target.value }))}
                      required
                    />
                  </div>
                  <div className="form-field">
                    <label className="form-label">Activation Date</label>
                    <input
                      className="form-input"
                      type="date"
                      value={cureForm.activationDate}
                      onChange={e =>
                        setCureForm(f => ({ ...f, activationDate: e.target.value }))
                      }
                      required
                    />
                  </div>
                  <div className="form-field">
                    <label className="form-label">Cure Period (days)</label>
                    <input
                      className="form-input"
                      type="number"
                      min="1"
                      value={cureForm.curePeriodDays}
                      onChange={e =>
                        setCureForm(f => ({
                          ...f,
                          curePeriodDays: parseInt(e.target.value) || 90,
                        }))
                      }
                      required
                    />
                  </div>
                </div>
                <button
                  type="submit"
                  className="btn btn-secondary btn-sm"
                  disabled={cureMutation.isPending}
                >
                  {cureMutation.isPending ? 'Activating…' : 'Activate Cure Period'}
                </button>
                {cureMutation.isError && <div className="form-error">Failed.</div>}
              </form>
            )}
            {(fpc.curePeriodActivations ?? []).map(cp => (
              <div
                key={cp.id}
                style={{
                  padding: '8px 10px',
                  background: 'var(--surface)',
                  borderRadius: 4,
                  fontSize: 12,
                  marginBottom: 6,
                }}
              >
                <div style={{ fontFamily: 'var(--font-mono)', marginBottom: 2 }}>
                  LRA: {cp.lraId}
                </div>
                <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                  {cp.curePeriodDays} days from {fmtD(cp.activationDate)} to {fmtD(cp.cureDeadline)}
                </div>
              </div>
            ))}
          </div>

          {/* Resolution */}
          {isActive && (
            <form onSubmit={handleResolve} style={{ marginTop: 12, paddingTop: 12, borderTop: '1px solid var(--border)' }}>
              <div className="form-grid" style={{ marginBottom: 12 }}>
                <div className="form-field">
                  <label className="form-label">Resolution Outcome</label>
                  <select
                    className="form-select"
                    value={resolveForm.outcome}
                    onChange={e => setResolveForm(f => ({ ...f, outcome: e.target.value }))}
                  >
                    {OUTCOMES.map(o => (
                      <option key={o.value} value={o.value}>
                        {o.label}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="form-field">
                  <label className="form-label">Resolution Date</label>
                  <input
                    className="form-input"
                    type="date"
                    value={resolveForm.resolvedDate}
                    onChange={e => setResolveForm(f => ({ ...f, resolvedDate: e.target.value }))}
                    required
                  />
                </div>
              </div>
              <button
                type="submit"
                className="btn btn-secondary"
                disabled={resolveMutation.isPending}
              >
                {resolveMutation.isPending ? 'Resolving…' : 'Resolve Case'}
              </button>
              {resolveMutation.isError && <div className="form-error">Failed.</div>}
            </form>
          )}
        </div>
      )}
    </div>
  )
}

// ── Inline sub-forms for existing list items ──────────────────────────────────

function ReferralOutcomeForm({
  caseId,
  referralId,
  mutation,
  showError,
  success,
}: {
  caseId: string
  referralId: string
  mutation: ReturnType<typeof useRecordCounselingOutcome>
  showError: (m: string) => void
  success: (m: string) => void
}) {
  const [form, setForm] = useState({ outcome: 'CounselingCompleted', outcomeDate: today() })
  const handle = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await mutation.mutateAsync({ caseId, referralId, ...form })
      success('Outcome recorded')
    } catch {
      showError('Failed to record outcome')
    }
  }
  return (
    <form onSubmit={handle} style={{ marginTop: 8, display: 'flex', gap: 8, alignItems: 'flex-end' }}>
      <div className="form-field" style={{ margin: 0 }}>
        <select
          className="form-select"
          value={form.outcome}
          onChange={e => setForm(f => ({ ...f, outcome: e.target.value }))}
        >
          {COUNSELING_OUTCOMES.map(o => (
            <option key={o} value={o}>
              {o}
            </option>
          ))}
        </select>
      </div>
      <div className="form-field" style={{ margin: 0 }}>
        <input
          className="form-input"
          type="date"
          value={form.outcomeDate}
          onChange={e => setForm(f => ({ ...f, outcomeDate: e.target.value }))}
        />
      </div>
      <button
        type="submit"
        className="btn btn-ghost btn-sm"
        disabled={mutation.isPending}
      >
        {mutation.isPending ? '…' : 'Record'}
      </button>
    </form>
  )
}

function ForbearanceOutcomeForm({
  caseId,
  requestId,
  mutation,
  showError,
  success,
}: {
  caseId: string
  requestId: string
  mutation: ReturnType<typeof useRecordForbearanceOutcome>
  showError: (m: string) => void
  success: (m: string) => void
}) {
  const [form, setForm] = useState({
    outcome: 'Granted',
    outcomeDate: today(),
    notes: '',
  })
  const handle = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await mutation.mutateAsync({
        caseId,
        requestId,
        ...form,
        notes: form.notes || undefined,
      })
      success('Outcome recorded')
    } catch {
      showError('Failed to record outcome')
    }
  }
  return (
    <form
      onSubmit={handle}
      style={{
        marginTop: 8,
        display: 'flex',
        gap: 8,
        alignItems: 'flex-end',
        flexWrap: 'wrap',
      }}
    >
      <div className="form-field" style={{ margin: 0 }}>
        <select
          className="form-select"
          value={form.outcome}
          onChange={e => setForm(f => ({ ...f, outcome: e.target.value }))}
        >
          {FORBEARANCE_OUTCOMES.map(o => (
            <option key={o} value={o}>
              {o}
            </option>
          ))}
        </select>
      </div>
      <div className="form-field" style={{ margin: 0 }}>
        <input
          className="form-input"
          type="date"
          value={form.outcomeDate}
          onChange={e => setForm(f => ({ ...f, outcomeDate: e.target.value }))}
        />
      </div>
      <div className="form-field" style={{ margin: 0, flex: '1 1 100%' }}>
        <input
          className="form-input"
          placeholder="Notes (optional)"
          value={form.notes}
          onChange={e => setForm(f => ({ ...f, notes: e.target.value }))}
        />
      </div>
      <button
        type="submit"
        className="btn btn-ghost btn-sm"
        disabled={mutation.isPending}
      >
        {mutation.isPending ? '…' : 'Record'}
      </button>
    </form>
  )
}
