import { useState } from 'react'
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
import { getErrorMessage } from '../../lib/getErrorMessage'
import {
  COUNSELING_OUTCOME_LABELS,
  FORBEARANCE_OUTCOMES,
  FORECLOSURE_PREVENTION_OUTCOME_LABELS,
  statusLabel,
} from '../../hooks/types'
import { fmtD, today } from './format'

interface Props {
  fpc: ForeclosurePreventionCaseResponse
}

const itemStyle = {
  padding: '8px 10px',
  background: 'var(--surface)',
  borderRadius: 4,
  fontSize: 12,
  marginBottom: 6,
} as const

const sectionTitleStyle = { fontSize: 12, fontWeight: 600, marginBottom: 8 } as const

export default function ForeclosurePreventionCaseCard({ fpc }: Props) {
  const caseId = fpc.id!
  const { success, error: showError } = useToast()
  const fail = async (err: unknown, fallback: string) => showError(await getErrorMessage(err, fallback))

  const delinquencyMutation = useRecordServicerDelinquency()
  const referralMutation = useMakeCounselingReferral()
  const referralOutcomeMutation = useRecordCounselingOutcome()
  const forbearanceMutation = useRequestForbearance()
  const forbearanceOutcomeMutation = useRecordForbearanceOutcome()
  const cureMutation = useActivateCurePeriod()
  const resolveMutation = useResolveForeclosurePreventionCase()

  const delinquencies = fpc.servicerDelinquencies ?? []
  const latestDelinquency = delinquencies.at(-1)

  // ── Local form state ──────────────────────────────────────────────────────
  const [delinquencyForm, setDelinquencyForm] = useState({
    servicerName: latestDelinquency?.servicerName ?? '',
    delinquentMonths: (latestDelinquency?.delinquentMonths ?? 0) + 1,
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

  // Interventions can be recorded until the case is resolved; resolving needs at least one (Active).
  const isOpen = fpc.status !== 'Resolved'
  const isActive = fpc.status === 'Active'

  const handleDelinquency = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await delinquencyMutation.mutateAsync({ caseId, ...delinquencyForm })
      success('Delinquency recorded')
    } catch (err) {
      await fail(err, 'Failed to record delinquency')
    }
  }

  const handleReferral = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await referralMutation.mutateAsync({ caseId, ...referralForm })
      success('Referral made')
      setReferralForm({ agencyName: '', referralDate: today() })
    } catch (err) {
      await fail(err, 'Failed to make referral')
    }
  }

  const handleForbearance = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await forbearanceMutation.mutateAsync({ caseId, ...forbearanceForm })
      success('Forbearance requested')
      setForbearanceForm({ lenderName: '', requestedDate: today() })
    } catch (err) {
      await fail(err, 'Failed to request forbearance')
    }
  }

  const handleCure = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await cureMutation.mutateAsync({ caseId, ...cureForm })
      success('Cure period activated')
      setCureForm({ lraId: '', activationDate: today(), curePeriodDays: 90 })
    } catch (err) {
      await fail(err, 'Failed to activate cure period')
    }
  }

  const handleResolve = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await resolveMutation.mutateAsync({ caseId, ...resolveForm })
      success('Case resolved')
    } catch (err) {
      await fail(err, 'Failed to resolve case')
    }
  }

  return (
    <div className="detail-section">
      <div style={{ fontSize: 12, color: 'var(--text-dim)', marginBottom: 12 }}>
        {statusLabel(fpc.trigger ?? '')} · Opened {fmtD(fpc.openedDate)}
        {fpc.resolvedDate && ` · Resolved ${fmtD(fpc.resolvedDate)}`}
        {fpc.outcome && ` · ${FORECLOSURE_PREVENTION_OUTCOME_LABELS[fpc.outcome] ?? fpc.outcome}`}
      </div>

      {/* Delinquencies */}
      <div style={{ marginBottom: 12 }}>
        <div style={sectionTitleStyle}>Delinquencies</div>
        {delinquencies.map((d, i) => (
          <div key={i} style={itemStyle}>
            <div style={{ fontFamily: 'var(--font-mono)', marginBottom: 2 }}>
              {d.servicerName} · {d.delinquentMonths} mo
            </div>
            <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>Recorded {fmtD(d.recordedDate)}</div>
          </div>
        ))}
        {isOpen && (
          <form onSubmit={handleDelinquency} style={{ marginTop: 8 }}>
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
          </form>
        )}
      </div>

      {/* Counseling Referrals */}
      <div style={{ marginBottom: 12 }}>
        <div style={sectionTitleStyle}>Counseling Referrals</div>
        {(fpc.counselingReferrals ?? []).map(r => (
          <div key={r.id} style={itemStyle}>
            <div style={{ fontFamily: 'var(--font-mono)', marginBottom: 2 }}>
              {r.agencyName}
            </div>
            <div style={{ fontSize: 11, color: 'var(--text-dim)', marginBottom: 4 }}>
              Referred {fmtD(r.referralDate)}
            </div>
            {r.outcome ? (
              <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                Outcome: {COUNSELING_OUTCOME_LABELS[r.outcome] ?? r.outcome}
                {r.outcomeDate && ` (${fmtD(r.outcomeDate)})`}
              </div>
            ) : isActive && (
              <ReferralOutcomeForm
                caseId={caseId}
                referralId={r.id!}
                mutation={referralOutcomeMutation}
                fail={fail}
                success={success}
              />
            )}
          </div>
        ))}
        {isOpen && (
          <form onSubmit={handleReferral} style={{ marginTop: 8 }}>
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
          </form>
        )}
      </div>

      {/* Forbearance Requests */}
      <div style={{ marginBottom: 12 }}>
        <div style={sectionTitleStyle}>Forbearance Requests</div>
        {(fpc.forbearanceRequests ?? []).map(req => (
          <div key={req.id} style={itemStyle}>
            <div style={{ fontFamily: 'var(--font-mono)', marginBottom: 2 }}>
              {req.lenderName}
            </div>
            <div style={{ fontSize: 11, color: 'var(--text-dim)', marginBottom: 4 }}>
              Requested {fmtD(req.requestedDate)}
            </div>
            {req.outcome ? (
              <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
                Outcome: {req.outcome}
                {req.outcomeDate && ` (${fmtD(req.outcomeDate)})`}
                {req.notes && ` — ${req.notes}`}
              </div>
            ) : isActive && (
              <ForbearanceOutcomeForm
                caseId={caseId}
                requestId={req.id!}
                mutation={forbearanceOutcomeMutation}
                fail={fail}
                success={success}
              />
            )}
          </div>
        ))}
        {isOpen && (
          <form onSubmit={handleForbearance} style={{ marginTop: 8 }}>
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
          </form>
        )}
      </div>

      {/* Cure Periods */}
      <div style={{ marginBottom: 12 }}>
        <div style={sectionTitleStyle}>Cure Periods</div>
        {(fpc.curePeriodActivations ?? []).map(cp => (
          <div key={cp.id} style={itemStyle}>
            <div style={{ fontFamily: 'var(--font-mono)', marginBottom: 2 }}>
              LRA: {cp.lraId}
            </div>
            <div style={{ fontSize: 11, color: 'var(--text-dim)' }}>
              {cp.curePeriodDays} days from {fmtD(cp.activationDate)} to {fmtD(cp.cureDeadline)}
            </div>
          </div>
        ))}
        {isOpen && (
          <form onSubmit={handleCure} style={{ marginTop: 8 }}>
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
          </form>
        )}
      </div>

      {/* Resolution */}
      {isOpen && !isActive && (
        <div style={{ marginTop: 12, paddingTop: 12, borderTop: '1px solid var(--border)', fontSize: 12, color: 'var(--text-dim)' }}>
          Make a referral, request forbearance, or activate a cure period before resolving the case.
        </div>
      )}
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
                {Object.entries(FORECLOSURE_PREVENTION_OUTCOME_LABELS).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
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
        </form>
      )}
    </div>
  )
}

// ── Inline sub-forms for existing list items ──────────────────────────────────

function ReferralOutcomeForm({
  caseId,
  referralId,
  mutation,
  fail,
  success,
}: {
  caseId: string
  referralId: string
  mutation: ReturnType<typeof useRecordCounselingOutcome>
  fail: (err: unknown, fallback: string) => Promise<unknown>
  success: (m: string) => void
}) {
  const [form, setForm] = useState({ outcome: 'CounselingCompleted', outcomeDate: today() })
  const handle = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await mutation.mutateAsync({ caseId, referralId, ...form })
      success('Outcome recorded')
    } catch (err) {
      await fail(err, 'Failed to record outcome')
    }
  }
  return (
    <form onSubmit={handle} style={{ marginTop: 8, display: 'flex', gap: 8, alignItems: 'flex-end' }}>
      <div className="form-field" style={{ margin: 0 }}>
        <select
          className="form-select"
          aria-label="Counseling outcome"
          value={form.outcome}
          onChange={e => setForm(f => ({ ...f, outcome: e.target.value }))}
        >
          {Object.entries(COUNSELING_OUTCOME_LABELS).map(([value, label]) => (
            <option key={value} value={value}>
              {label}
            </option>
          ))}
        </select>
      </div>
      <div className="form-field" style={{ margin: 0 }}>
        <input
          className="form-input"
          type="date"
          aria-label="Outcome date"
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
  fail,
  success,
}: {
  caseId: string
  requestId: string
  mutation: ReturnType<typeof useRecordForbearanceOutcome>
  fail: (err: unknown, fallback: string) => Promise<unknown>
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
    } catch (err) {
      await fail(err, 'Failed to record outcome')
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
          aria-label="Forbearance outcome"
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
          aria-label="Outcome date"
          value={form.outcomeDate}
          onChange={e => setForm(f => ({ ...f, outcomeDate: e.target.value }))}
        />
      </div>
      <div className="form-field" style={{ margin: 0, flex: '1 1 100%' }}>
        <input
          className="form-input"
          placeholder="Notes (optional)"
          aria-label="Outcome notes"
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
