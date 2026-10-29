import { useState } from 'react'
import StatusBadge from '../StatusBadge'
import {
  useSchedulePropertyInspection,
  useCompletePropertyInspection,
  useCancelPropertyInspection,
} from '../../hooks/stewardship'
import type { PropertyInspectionResponse, StewardshipCaseResponse } from '../../api/models'
import { useToast } from '../../hooks/useToast'
import { getErrorMessage } from '../../lib/getErrorMessage'
import { trustToday } from '../../lib/calendarDate'
import { DateInput } from '../DateInput'

interface Props {
  sc: StewardshipCaseResponse
}

export default function PropertyInspectionSection({ sc }: Props) {
  const caseId = sc.id!
  const { success, error: showError } = useToast()

  const scheduleMutation = useSchedulePropertyInspection()
  const completeMutation = useCompletePropertyInspection()
  const cancelMutation = useCancelPropertyInspection()

  const fail = async (err: unknown, fallback: string) => showError(await getErrorMessage(err, fallback))

  const scheduled = (sc.propertyInspections ?? []).find(i => i.status === 'Scheduled') ?? null
  const history = (sc.propertyInspections ?? [])
    .filter(i => i.status !== 'Scheduled')
    .sort((a, b) => (b.scheduledDate ?? '').localeCompare(a.scheduledDate ?? ''))

  // ── Forms ─────────────────────────────────────────────────────────────────
  const [scheduleFormOpen, setScheduleFormOpen] = useState(false)
  const [scheduleForm, setScheduleForm] = useState({ scheduledDate: trustToday(), inspectorName: '' })
  const [completeForm, setCompleteForm] = useState({
    occupancyConfirmed: true,
    conditionNotes: '',
    completedDate: trustToday(),
  })
  const [cancelForm, setCancelForm] = useState({ reason: '' })
  const [historyOpen, setHistoryOpen] = useState(false)

  const isOverdue =
    scheduled && (scheduled.scheduledDate ?? '') < trustToday()

  const handleSchedule = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await scheduleMutation.mutateAsync({ caseId, ...scheduleForm })
      success('Inspection scheduled')
      setScheduleForm({ scheduledDate: trustToday(), inspectorName: '' })
      setScheduleFormOpen(false)
    } catch (err) {
      await fail(err, 'Failed to schedule inspection')
    }
  }

  const handleComplete = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!scheduled) return
    try {
      await completeMutation.mutateAsync({
        caseId,
        inspectionId: scheduled.id!,
        occupancyConfirmed: completeForm.occupancyConfirmed,
        conditionNotes: completeForm.conditionNotes || undefined,
        completedDate: completeForm.completedDate,
      })
      success('Inspection completed')
    } catch (err) {
      await fail(err, 'Failed to complete inspection')
    }
  }

  const handleCancel = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!scheduled) return
    try {
      await cancelMutation.mutateAsync({
        caseId,
        inspectionId: scheduled.id!,
        reason: cancelForm.reason,
      })
      success('Inspection cancelled')
      setCancelForm({ reason: '' })
    } catch (err) {
      await fail(err, 'Failed to cancel inspection')
    }
  }

  return (
    <div className="detail-section">
      <div
        className="detail-section__title"
        style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}
      >
        <span>Property Inspection</span>
        {!scheduled && (
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            aria-label={scheduleFormOpen ? 'Close inspection form' : 'Schedule inspection'}
            onClick={() => setScheduleFormOpen(!scheduleFormOpen)}
          >
            {scheduleFormOpen ? '×' : '+'}
          </button>
        )}
      </div>

      {!scheduled && !scheduleFormOpen && (
        <div style={{ color: 'var(--text-dim)', fontSize: 13 }}>No inspection scheduled.</div>
      )}

      {/* Schedule form */}
      {scheduleFormOpen && !scheduled && (
        <form onSubmit={handleSchedule}>
          <div className="form-grid" style={{ marginBottom: 12 }}>
            <div className="form-field">
              <label className="form-label">Scheduled Date</label>
              <DateInput
                className="form-input"
                value={scheduleForm.scheduledDate}
                onChange={v => setScheduleForm(f => ({ ...f, scheduledDate: v }))}
                required
              />
            </div>
            <div className="form-field">
              <label className="form-label">Inspector Name</label>
              <input
                className="form-input"
                value={scheduleForm.inspectorName}
                onChange={e => setScheduleForm(f => ({ ...f, inspectorName: e.target.value }))}
                required
              />
            </div>
          </div>
          <button
            type="submit"
            className="btn btn-secondary"
            disabled={scheduleMutation.isPending}
          >
            {scheduleMutation.isPending ? 'Scheduling…' : 'Schedule Inspection'}
          </button>
        </form>
      )}

      {/* Scheduled inspection */}
      {scheduled && (
        <>
          <div
            style={{
              padding: '10px 14px',
              background: isOverdue ? 'var(--surface)' : 'var(--amber-dim)',
              borderLeft: `3px solid ${isOverdue ? 'var(--red)' : 'var(--amber)'}`,
              borderRadius: 4,
              marginBottom: 12,
              fontSize: 13,
            }}
          >
            <div style={{ fontFamily: 'var(--font-mono)' }}>
              {scheduled.scheduledDate} · {scheduled.inspectorName}
              {isOverdue && ' · OVERDUE'}
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <form onSubmit={handleComplete}>
              <div className="form-grid" style={{ marginBottom: 12 }}>
                <div className="form-field">
                  <label className="form-label">Occupancy Confirmed</label>
                  <input
                    type="checkbox"
                    checked={completeForm.occupancyConfirmed}
                    onChange={e =>
                      setCompleteForm(f => ({ ...f, occupancyConfirmed: e.target.checked }))
                    }
                  />
                </div>
                <div className="form-field full">
                  <label className="form-label">Condition Notes (optional)</label>
                  <textarea
                    className="form-textarea"
                    value={completeForm.conditionNotes}
                    onChange={e =>
                      setCompleteForm(f => ({ ...f, conditionNotes: e.target.value }))
                    }
                  />
                </div>
                <div className="form-field">
                  <label className="form-label">Completed Date</label>
                  <DateInput
                    className="form-input"
                    value={completeForm.completedDate}
                    onChange={v =>
                      setCompleteForm(f => ({ ...f, completedDate: v }))
                    }
                    required
                  />
                </div>
              </div>
              <button
                type="submit"
                className="btn btn-secondary"
                disabled={completeMutation.isPending}
              >
                {completeMutation.isPending ? 'Completing…' : 'Complete'}
              </button>
            </form>

            <form onSubmit={handleCancel}>
              <div className="form-grid" style={{ marginBottom: 12 }}>
                <div className="form-field full">
                  <label className="form-label">Cancellation Reason</label>
                  <textarea
                    className="form-textarea"
                    value={cancelForm.reason}
                    onChange={e => setCancelForm(f => ({ ...f, reason: e.target.value }))}
                    required
                  />
                </div>
              </div>
              <button
                type="submit"
                className="btn btn-secondary btn-ghost"
                disabled={cancelMutation.isPending}
              >
                {cancelMutation.isPending ? 'Cancelling…' : 'Cancel'}
              </button>
            </form>
          </div>
        </>
      )}

      {/* History */}
      {history.length > 0 && (
        <div style={{ marginTop: 16 }}>
          <button
            type="button"
            className="btn btn-ghost btn-sm"
            onClick={() => setHistoryOpen(!historyOpen)}
          >
            {historyOpen ? '↑' : '↓'} Past inspections ({history.length})
          </button>
          {historyOpen && (
            <div style={{ marginTop: 8 }}>
              {history.map(i => (
                <InspectionHistoryRow key={i.id} i={i} />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  )
}

function InspectionHistoryRow({ i }: { i: PropertyInspectionResponse }) {
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
        {i.completedDate ?? i.scheduledDate} · {i.inspectorName} · <StatusBadge status={i.status ?? ''} />
      </div>
      {i.status === 'Completed' && (
        <div style={{ color: 'var(--text-dim)', marginTop: 2 }}>
          {i.occupancyConfirmed ? 'Occupancy confirmed' : 'Occupancy not confirmed'}
        </div>
      )}
      {i.conditionNotes && (
        <div style={{ color: 'var(--text-dim)', marginTop: 2 }}>{i.conditionNotes}</div>
      )}
      {i.cancellationReason && (
        <div style={{ color: 'var(--text-dim)', marginTop: 2 }}>Cancelled: {i.cancellationReason}</div>
      )}
    </div>
  )
}
