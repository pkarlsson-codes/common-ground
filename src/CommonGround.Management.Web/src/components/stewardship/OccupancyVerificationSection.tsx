import { useState } from 'react'
import StatusBadge from '../StatusBadge'
import {
  useInitiateOccupancyVerification,
  useSubmitOccupancyVerification,
  useMarkOccupancyVerificationOverdue,
  useVerifyOccupancy,
  useFailOccupancy,
} from '../../hooks/stewardship'
import type { OccupancyVerificationResponse, StewardshipCaseResponse } from '../../api/models'
import { useToast } from '../../hooks/useToast'
import { getErrorMessage } from '../../lib/getErrorMessage'
import { trustToday, yearOf, type CalendarDate } from '../../lib/calendarDate'
import { DateInput } from '../DateInput'

interface Props {
  sc: StewardshipCaseResponse
}

const currentYear = () => yearOf(trustToday())

export default function OccupancyVerificationSection({ sc }: Props) {
  const caseId = sc.id!
  const { success, error: showError } = useToast()

  const initiateMutation = useInitiateOccupancyVerification()
  const submitMutation = useSubmitOccupancyVerification()
  const overdueMutation = useMarkOccupancyVerificationOverdue()
  const verifyMutation = useVerifyOccupancy()
  const failMutation = useFailOccupancy()

  const fail = async (err: unknown, fallback: string) => showError(await getErrorMessage(err, fallback))

  // ── Current-year verification ─────────────────────────────────────────────
  const year = currentYear()
  const current = sc.occupancyVerifications?.find(v => v.periodYear === year) ?? null
  const history = (sc.occupancyVerifications ?? [])
    .filter(v => v.periodYear !== year)
    .sort((a, b) => (b.periodYear ?? 0) - (a.periodYear ?? 0))

  // ── Initiate form ─────────────────────────────────────────────────────────
  const [initiateFormOpen, setInitiateFormOpen] = useState(false)
  const [initiateForm, setInitiateForm] = useState<{ surveyDeadline: CalendarDate | null }>({ surveyDeadline: null })
  const handleInitiate = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await initiateMutation.mutateAsync({
        caseId,
        periodYear: year,
        surveyDeadline: initiateForm.surveyDeadline ?? undefined,
      })
      success(`${year} occupancy verification initiated`)
      setInitiateForm({ surveyDeadline: null })
      setInitiateFormOpen(false)
    } catch (err) {
      await fail(err, 'Failed to initiate verification')
    }
  }

  // ── Submit form ───────────────────────────────────────────────────────────
  const [submitForm, setSubmitForm] = useState({
    isPrimaryResident: true,
    subletDisclosed: false,
    majorChanges: '',
    submittedDate: trustToday(),
  })
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!current) return
    try {
      await submitMutation.mutateAsync({
        caseId,
        verificationId: current.id!,
        isPrimaryResident: submitForm.isPrimaryResident,
        subletDisclosed: submitForm.subletDisclosed,
        majorChanges: submitForm.majorChanges || undefined,
        submittedDate: submitForm.submittedDate,
      })
      success('Verification response submitted')
    } catch (err) {
      await fail(err, 'Failed to submit response')
    }
  }

  // ── Overdue form ──────────────────────────────────────────────────────────
  const [overdueForm, setOverdueForm] = useState({ deadline: trustToday() })
  const handleOverdue = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!current) return
    try {
      await overdueMutation.mutateAsync({
        caseId,
        verificationId: current.id!,
        deadline: overdueForm.deadline,
      })
      success('Verification marked overdue')
    } catch (err) {
      await fail(err, 'Failed to mark overdue')
    }
  }

  // ── Verify form ───────────────────────────────────────────────────────────
  const [verifyForm, setVerifyForm] = useState({ verifiedDate: trustToday() })
  const handleVerify = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!current) return
    try {
      await verifyMutation.mutateAsync({
        caseId,
        verificationId: current.id!,
        verifiedDate: verifyForm.verifiedDate,
      })
      success('Occupancy verified')
    } catch (err) {
      await fail(err, 'Failed to verify')
    }
  }

  // ── Fail form ─────────────────────────────────────────────────────────────
  const [failForm, setFailForm] = useState({ reason: '', failedDate: trustToday() })
  const handleFail = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!current) return
    try {
      await failMutation.mutateAsync({
        caseId,
        verificationId: current.id!,
        reason: failForm.reason,
        failedDate: failForm.failedDate,
      })
      success('Verification failed')
    } catch (err) {
      await fail(err, 'Failed to record failure')
    }
  }

  // ── History ───────────────────────────────────────────────────────────────
  const [historyOpen, setHistoryOpen] = useState(false)

  const awaitingResponse = current?.status === 'Pending' || current?.status === 'Overdue'

  return (
    <div className="detail-section">
      <div
        className="detail-section__title"
        style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
      >
        <span>Occupancy Verification</span>
        {!current && (
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            aria-label={initiateFormOpen ? 'Close verification form' : 'Start verification'}
            onClick={() => setInitiateFormOpen(!initiateFormOpen)}
          >
            {initiateFormOpen ? '×' : '+'}
          </button>
        )}
      </div>

      {!current && !initiateFormOpen && (
        <div style={{ color: 'var(--text-dim)', fontSize: 13 }}>No verification started for {year}.</div>
      )}

      {/* Initiate form */}
      {initiateFormOpen && !current && (
        <form onSubmit={handleInitiate}>
          <div className="form-grid" style={{ marginBottom: 10 }}>
            <div className="form-field">
              <label className="form-label">Survey Deadline (optional)</label>
              <DateInput
                className="form-input"
                value={initiateForm.surveyDeadline}
                onChange={v => setInitiateForm(f => ({ ...f, surveyDeadline: v }))}
              />
            </div>
          </div>
          <button
            type="submit"
            className="btn btn-secondary"
            disabled={initiateMutation.isPending}
          >
            {initiateMutation.isPending ? 'Starting…' : 'Start Verification'}
          </button>
        </form>
      )}

      {/* Pending or Overdue — submit response (and mark overdue while Pending) */}
      {awaitingResponse && current && (
        <>
          <div style={{ marginBottom: 12, fontSize: 13, color: 'var(--text-secondary)' }}>
            {current.status === 'Overdue'
              ? <><StatusBadge status="Overdue" /> No response by the deadline. A late response can still be submitted.</>
              : 'Verification initiated. Awaiting homeowner response.'}
            {current.surveyDeadline && ` Deadline: ${current.surveyDeadline}`}
          </div>

          <form onSubmit={handleSubmit} style={{ marginBottom: 12 }}>
            <div className="form-grid" style={{ marginBottom: 12 }}>
              <div className="form-field">
                <label className="form-label">Primary Resident</label>
                <input
                  type="checkbox"
                  checked={submitForm.isPrimaryResident}
                  onChange={e => setSubmitForm(f => ({ ...f, isPrimaryResident: e.target.checked }))}
                />
              </div>
              <div className="form-field">
                <label className="form-label">Sublet Disclosed</label>
                <input
                  type="checkbox"
                  checked={submitForm.subletDisclosed}
                  onChange={e => setSubmitForm(f => ({ ...f, subletDisclosed: e.target.checked }))}
                />
              </div>
              <div className="form-field full">
                <label className="form-label">Major Changes (optional)</label>
                <textarea
                  className="form-textarea"
                  value={submitForm.majorChanges}
                  onChange={e => setSubmitForm(f => ({ ...f, majorChanges: e.target.value }))}
                />
              </div>
              <div className="form-field">
                <label className="form-label">Submitted Date</label>
                <DateInput
                  className="form-input"
                  value={submitForm.submittedDate}
                  onChange={v => setSubmitForm(f => ({ ...f, submittedDate: v }))}
                  required
                />
              </div>
            </div>
            <button
              type="submit"
              className="btn btn-secondary"
              disabled={submitMutation.isPending}
            >
              {submitMutation.isPending ? 'Submitting…' : 'Submit Response'}
            </button>
          </form>

          {current.status === 'Pending' && (
            <form onSubmit={handleOverdue}>
              <div className="form-grid" style={{ marginBottom: 12 }}>
                <div className="form-field">
                  <label className="form-label">Mark Overdue as of Date</label>
                  <DateInput
                    className="form-input"
                    value={overdueForm.deadline}
                    onChange={v => setOverdueForm(f => ({ ...f, deadline: v }))}
                    required
                  />
                </div>
              </div>
              <button
                type="submit"
                className="btn btn-secondary btn-ghost"
                disabled={overdueMutation.isPending}
              >
                {overdueMutation.isPending ? 'Marking…' : 'Mark Overdue'}
              </button>
            </form>
          )}
        </>
      )}

      {/* Submitted — verify or fail side by side */}
      {current?.status === 'Submitted' && (
        <>
          <div style={{ marginBottom: 12, fontSize: 13 }}>
            Response submitted {current.submittedDate}
            {' · '}{current.isPrimaryResident ? 'Primary resident' : 'Not primary resident'}
            {current.subletDisclosed && ' · Sublet disclosed'}
            {current.majorChanges && ` · Changes: ${current.majorChanges}`}
            . Verify or reject.
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <form onSubmit={handleVerify}>
              <div className="form-grid" style={{ marginBottom: 12 }}>
                <div className="form-field">
                  <label className="form-label">Verified Date</label>
                  <DateInput
                    className="form-input"
                    value={verifyForm.verifiedDate}
                    onChange={v => setVerifyForm(f => ({ ...f, verifiedDate: v }))}
                    required
                  />
                </div>
              </div>
              <button
                type="submit"
                className="btn btn-secondary"
                disabled={verifyMutation.isPending}
              >
                {verifyMutation.isPending ? 'Verifying…' : 'Verify'}
              </button>
            </form>

            <form onSubmit={handleFail}>
              <div className="form-grid" style={{ marginBottom: 12 }}>
                <div className="form-field full">
                  <label className="form-label">Reason for Rejection</label>
                  <textarea
                    className="form-textarea"
                    value={failForm.reason}
                    onChange={e => setFailForm(f => ({ ...f, reason: e.target.value }))}
                    required
                  />
                </div>
                <div className="form-field">
                  <label className="form-label">Failed Date</label>
                  <DateInput
                    className="form-input"
                    value={failForm.failedDate}
                    onChange={v => setFailForm(f => ({ ...f, failedDate: v }))}
                    required
                  />
                </div>
              </div>
              <button
                type="submit"
                className="btn btn-secondary btn-ghost"
                disabled={failMutation.isPending}
              >
                {failMutation.isPending ? 'Failing…' : 'Reject'}
              </button>
            </form>
          </div>
        </>
      )}

      {/* Verified — green confirmation */}
      {current?.status === 'Verified' && (
        <div
          style={{
            padding: '10px 14px',
            background: 'var(--success-dim)',
            borderRadius: 6,
            fontSize: 13,
            color: 'var(--success)',
          }}
        >
          Verified {current.decisionDate} — occupancy confirmed for {year}.
        </div>
      )}

      {/* Failed — show reason */}
      {current?.status === 'Failed' && (
        <div
          style={{
            padding: '10px 14px',
            background: 'var(--surface)',
            borderRadius: 6,
            borderLeft: '3px solid var(--red)',
          }}
        >
          <StatusBadge status={current.status} />
          <span style={{ fontSize: 12, marginLeft: 8 }}>{current.decisionDate}</span>
          {current.reason && <div style={{ fontSize: 12, marginTop: 4 }}>{current.reason}</div>}
        </div>
      )}

      {/* History */}
      {history.length > 0 && (
        <div style={{ marginTop: 16 }}>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => setHistoryOpen(!historyOpen)}
          >
            {historyOpen ? '↑' : '↓'} Prior years ({history.length})
          </button>
          {historyOpen && (
            <div style={{ marginTop: 8 }}>
              {history.map(v => (
                <VerificationHistoryRow key={v.id} v={v} />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  )
}

function VerificationHistoryRow({ v }: { v: OccupancyVerificationResponse }) {
  return (
    <div
      style={{
        padding: '8px 12px',
        borderLeft: '2px solid var(--border)',
        marginBottom: 6,
        fontSize: 12,
      }}
    >
      <div style={{ fontFamily: 'var(--font-mono)', fontSize: 11, color: 'var(--text-secondary)' }}>
        {v.periodYear} · <StatusBadge status={v.status ?? ''} />
      </div>
      {v.decisionDate && (
        <div style={{ color: 'var(--text-dim)', marginTop: 2 }}>{v.decisionDate}</div>
      )}
      {v.reason && <div style={{ color: 'var(--text-dim)', marginTop: 2 }}>{v.reason}</div>}
    </div>
  )
}
