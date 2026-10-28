import { useState } from 'react'
import StatusBadge from '../StatusBadge'
import {
  useIdentifyViolation,
  useIssueViolationNotice,
  useCureViolation,
  useEscalateViolation,
} from '../../hooks/stewardship'
import type { LeaseViolationResponse, StewardshipCaseResponse } from '../../api/models'
import { useToast } from '../../hooks/useToast'
import { getErrorMessage } from '../../lib/getErrorMessage'
import { LEASE_VIOLATION_TYPE_LABELS } from '../../hooks/types'
import { fmtD, today } from './format'

interface Props {
  sc: StewardshipCaseResponse
}

const violationTypeLabel = (t?: string | null) => LEASE_VIOLATION_TYPE_LABELS[t ?? ''] ?? t

const ACTIVE_STATUSES = new Set(['Identified', 'NoticeIssued'])
const TERMINAL_STATUSES = new Set(['Cured', 'Escalated'])

export default function LeaseViolationSection({ sc }: Props) {
  const caseId = sc.id!
  const { success, error: showError } = useToast()

  const identifyMutation = useIdentifyViolation()
  const noticeMutation = useIssueViolationNotice()
  const cureMutation = useCureViolation()
  const escalateMutation = useEscalateViolation()

  const violations = sc.leaseViolations ?? []
  const active = violations.filter(v => ACTIVE_STATUSES.has(v.status ?? ''))
  const history = violations
    .filter(v => TERMINAL_STATUSES.has(v.status ?? ''))
    .sort((a, b) => ((b.identifiedDate?.getTime() ?? 0) - (a.identifiedDate?.getTime() ?? 0)))

  const [identifyOpen, setIdentifyOpen] = useState(false)
  const [identifyForm, setIdentifyForm] = useState({
    violationType: 'Occupancy',
    description: '',
    identifiedDate: today(),
  })
  const [historyOpen, setHistoryOpen] = useState(false)

  const handleIdentify = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await identifyMutation.mutateAsync({ caseId, ...identifyForm })
      success('Violation identified')
      setIdentifyForm({ violationType: 'Occupancy', description: '', identifiedDate: today() })
      setIdentifyOpen(false)
    } catch (err) {
      showError(await getErrorMessage(err, 'Failed to identify violation'))
    }
  }

  return (
    <div className="detail-section">
      <div
        className="detail-section__title"
        style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
      >
        <span>Lease Violations</span>
        <button
          type="button"
          className="btn btn-ghost btn-sm"
          aria-label={identifyOpen ? 'Close violation form' : 'Identify violation'}
          onClick={() => setIdentifyOpen(!identifyOpen)}
        >
          {identifyOpen ? '×' : '+'}
        </button>
      </div>

      {/* Identify form */}
      {identifyOpen && (
        <form onSubmit={handleIdentify} style={{ marginBottom: 16 }}>
          <div className="form-grid" style={{ marginBottom: 12 }}>
            <div className="form-field">
              <label className="form-label">Violation Type</label>
              <select
                className="form-select"
                value={identifyForm.violationType}
                onChange={e =>
                  setIdentifyForm(f => ({ ...f, violationType: e.target.value }))
                }
              >
                {Object.entries(LEASE_VIOLATION_TYPE_LABELS).map(([value, label]) => (
                  <option key={value} value={value}>
                    {label}
                  </option>
                ))}
              </select>
            </div>
            <div className="form-field full">
              <label className="form-label">Description</label>
              <textarea
                className="form-textarea"
                value={identifyForm.description}
                onChange={e =>
                  setIdentifyForm(f => ({ ...f, description: e.target.value }))
                }
                required
              />
            </div>
            <div className="form-field">
              <label className="form-label">Identified Date</label>
              <input
                className="form-input"
                type="date"
                value={identifyForm.identifiedDate}
                onChange={e =>
                  setIdentifyForm(f => ({ ...f, identifiedDate: e.target.value }))
                }
                required
              />
            </div>
          </div>
          <button
            type="submit"
            className="btn btn-secondary"
            disabled={identifyMutation.isPending}
          >
            {identifyMutation.isPending ? 'Identifying…' : 'Identify Violation'}
          </button>
        </form>
      )}

      {/* Active violations */}
      {active.length === 0 && !identifyOpen && (
        <div style={{ color: 'var(--text-dim)', fontSize: 13 }}>No active violations.</div>
      )}

      {active.map(v => (
        <ViolationCard
          key={v.id}
          v={v}
          caseId={caseId}
          noticeMutation={noticeMutation}
          cureMutation={cureMutation}
          escalateMutation={escalateMutation}
          showError={showError}
          success={success}
        />
      ))}

      {/* History */}
      {history.length > 0 && (
        <div style={{ marginTop: 12 }}>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => setHistoryOpen(!historyOpen)}
          >
            {historyOpen ? '↑' : '↓'} Resolved violations ({history.length})
          </button>
          {historyOpen && (
            <div style={{ marginTop: 8 }}>
              {history.map(v => (
                <ViolationHistoryRow key={v.id} v={v} />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  )
}

interface ViolationCardProps {
  v: LeaseViolationResponse
  caseId: string
  noticeMutation: ReturnType<typeof useIssueViolationNotice>
  cureMutation: ReturnType<typeof useCureViolation>
  escalateMutation: ReturnType<typeof useEscalateViolation>
  showError: (msg: string) => void
  success: (msg: string) => void
}

/** Default cure deadline: 30 days after the notice. */
const plus30Days = () => {
  const d = new Date()
  d.setDate(d.getDate() + 30)
  return d.toISOString().slice(0, 10)
}

function ViolationCard({
  v,
  caseId,
  noticeMutation,
  cureMutation,
  escalateMutation,
  showError,
  success,
}: ViolationCardProps) {
  const isCureOverdue =
    v.status === 'NoticeIssued' && (fmtD(v.cureDeadline) ?? '9999') < today()

  const [noticeForm, setNoticeForm] = useState({ noticeDate: today(), cureDeadline: plus30Days() })
  const [cureForm, setCureForm] = useState({ curedDate: today() })
  const [escalateForm, setEscalateForm] = useState({
    reason: '',
    escalatedDate: today(),
  })

  const handleNotice = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await noticeMutation.mutateAsync({ caseId, violationId: v.id!, ...noticeForm })
      success('Notice issued')
    } catch (err) {
      showError(await getErrorMessage(err, 'Failed to issue notice'))
    }
  }

  const handleCure = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await cureMutation.mutateAsync({ caseId, violationId: v.id!, ...cureForm })
      success('Violation cured')
    } catch (err) {
      showError(await getErrorMessage(err, 'Failed to cure'))
    }
  }

  const handleEscalate = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await escalateMutation.mutateAsync({ caseId, violationId: v.id!, ...escalateForm })
      success('Violation escalated')
    } catch (err) {
      showError(await getErrorMessage(err, 'Failed to escalate'))
    }
  }

  return (
    <div
      style={{
        padding: '12px 14px',
        border: '1px solid var(--border)',
        borderRadius: 6,
        marginBottom: 12,
      }}
    >
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 6 }}>
        <span style={{ fontFamily: 'var(--font-mono)', fontSize: 12 }}>{violationTypeLabel(v.violationType)}</span>
        <StatusBadge status={v.status ?? ''} />
      </div>
      <div style={{ fontSize: 13, marginBottom: 6 }}>{v.description}</div>
      <div style={{ fontFamily: 'var(--font-mono)', fontSize: 11, color: 'var(--text-dim)', marginBottom: 10 }}>
        Identified {fmtD(v.identifiedDate)}
        {v.noticeIssuedDate && ` · Notice ${fmtD(v.noticeIssuedDate)}`}
        {v.cureDeadline && ` · Cure by ${fmtD(v.cureDeadline)}`}
        {isCureOverdue && ' · OVERDUE'}
      </div>

      {/* Identified → Issue Notice */}
      {v.status === 'Identified' && (
        <form onSubmit={handleNotice}>
          <div className="form-grid" style={{ marginBottom: 12 }}>
            <div className="form-field">
              <label className="form-label">Notice Date</label>
              <input
                className="form-input"
                type="date"
                value={noticeForm.noticeDate}
                onChange={e => setNoticeForm(f => ({ ...f, noticeDate: e.target.value }))}
                required
              />
            </div>
            <div className="form-field">
              <label className="form-label">Cure Deadline</label>
              <input
                className="form-input"
                type="date"
                value={noticeForm.cureDeadline}
                onChange={e => setNoticeForm(f => ({ ...f, cureDeadline: e.target.value }))}
                required
              />
            </div>
          </div>
          <button
            type="submit"
            className="btn btn-secondary"
            disabled={noticeMutation.isPending}
          >
            {noticeMutation.isPending ? 'Issuing…' : 'Issue Notice'}
          </button>
        </form>
      )}

      {/* NoticeIssued → Cure or Escalate */}
      {v.status === 'NoticeIssued' && (
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
          <form onSubmit={handleCure}>
            <div className="form-grid" style={{ marginBottom: 12 }}>
              <div className="form-field">
                <label className="form-label">Cured Date</label>
                <input
                  className="form-input"
                  type="date"
                  value={cureForm.curedDate}
                  onChange={e => setCureForm(f => ({ ...f, curedDate: e.target.value }))}
                  required
                />
              </div>
            </div>
            <button
              type="submit"
              className="btn btn-secondary"
              disabled={cureMutation.isPending}
            >
              {cureMutation.isPending ? 'Curing…' : 'Mark Cured'}
            </button>
          </form>

          <form onSubmit={handleEscalate}>
            <div className="form-grid" style={{ marginBottom: 12 }}>
              <div className="form-field full">
                <label className="form-label">Escalation Reason</label>
                <textarea
                  className="form-textarea"
                  value={escalateForm.reason}
                  onChange={e => setEscalateForm(f => ({ ...f, reason: e.target.value }))}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Escalation Date</label>
                <input
                  className="form-input"
                  type="date"
                  value={escalateForm.escalatedDate}
                  onChange={e => setEscalateForm(f => ({ ...f, escalatedDate: e.target.value }))}
                  required
                />
              </div>
            </div>
            <button
              type="submit"
              className="btn btn-secondary btn-ghost"
              disabled={escalateMutation.isPending}
            >
              {escalateMutation.isPending ? 'Escalating…' : 'Escalate'}
            </button>
          </form>
        </div>
      )}
    </div>
  )
}

function ViolationHistoryRow({ v }: { v: LeaseViolationResponse }) {
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
        {violationTypeLabel(v.violationType)} · {fmtD(v.identifiedDate)} · <StatusBadge status={v.status ?? ''} />
        {v.resolutionDate && ` · ${fmtD(v.resolutionDate)}`}
      </div>
      <div style={{ color: 'var(--text-dim)', marginTop: 2 }}>{v.description}</div>
      {v.resolutionNotes && (
        <div style={{ color: 'var(--text-dim)', marginTop: 2 }}>{v.resolutionNotes}</div>
      )}
    </div>
  )
}
