import { useState } from 'react'
import { useBuilding, useRecordAssessment, useUpdateBuildingStatus } from '../../hooks/buildings'
import { BUILDING_TYPE_LABELS, type RecordBuildingAssessmentRequest } from '../../hooks/types'
import SlideOver from '../../components/SlideOver'
import StatusBadge from '../../components/StatusBadge'
import NavIdLink from '../../components/NavIdLink'
import { getAvailableActions } from '../../lib/transitionActions'
import { trackTransitionEvent } from '../../lib/telemetry'
import { useToast } from '../../hooks/useToast'

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

const fmtNumber = (n: number) => new Intl.NumberFormat('en-US').format(n)

const emptyAssessment = (): RecordBuildingAssessmentRequest => ({
  assessedValue: 0,
  assessmentDate: new Date().toISOString().slice(0, 10),
  conditionNotes: '',
})

interface Props {
  buildingId: string
  onClose: () => void
}

export default function BuildingDetailPanel({ buildingId, onClose }: Props) {
  const { data: detail, isLoading: detailLoading } = useBuilding(buildingId)
  const updateStatusMutation = useUpdateBuildingStatus()
  const assessmentMutation = useRecordAssessment()
  const [assessment, setAssessment] = useState<RecordBuildingAssessmentRequest>(emptyAssessment)
  const [pendingActionId, setPendingActionId] = useState<string | null>(null)
  const { success } = useToast()

  const handleAssessment = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await assessmentMutation.mutateAsync({
        buildingId,
        req: {
          ...assessment,
          assessedValue: Number(assessment.assessedValue),
          conditionNotes: assessment.conditionNotes || undefined,
        },
      })
      setAssessment(emptyAssessment())
      success('Assessment recorded')
    } catch { /* error shown inline */ }
  }

  const handleTransitionAction = async (action: { id: string; label: string; toState?: string; requiresConfirmation?: boolean; confirmationMessage?: string }) => {
    if (!detail?.status || !action.toState) return

    if (action.requiresConfirmation && !window.confirm(action.confirmationMessage ?? 'Are you sure?')) {
      return
    }

    trackTransitionEvent({
      event: 'ui.transition',
      aggregate: 'building',
      actionId: action.id,
      entityId: buildingId,
      fromState: detail.status,
      toState: action.toState,
      outcome: 'attempt',
    })

    setPendingActionId(action.id)

    try {
      await updateStatusMutation.mutateAsync({ id: buildingId, newStatus: action.toState })
      success(`${action.label} complete`)
      trackTransitionEvent({
        event: 'ui.transition',
        aggregate: 'building',
        actionId: action.id,
        entityId: buildingId,
        fromState: detail.status,
        toState: action.toState,
        outcome: 'success',
      })
    } catch (error) {
      trackTransitionEvent({
        event: 'ui.transition',
        aggregate: 'building',
        actionId: action.id,
        entityId: buildingId,
        fromState: detail.status,
        toState: action.toState,
        outcome: 'failure',
        error: error instanceof Error ? error.message : 'Unknown error',
      })
    } finally {
      setPendingActionId(null)
    }
  }

  return (
    <SlideOver
      title={detail ? (BUILDING_TYPE_LABELS[detail.buildingType ?? ''] ?? detail.buildingType ?? '…') : '…'}
      subtitle={detail ? `${detail.bedrooms} bd · ${Number(detail.bathrooms).toFixed(1)} ba · ${fmtNumber(Number(detail.squareFootage))} sq ft` : undefined}
      onClose={onClose}
    >
      {detailLoading || !detail ? <SkeletonRows /> : (
        <>
          {(() => {
            const actions = getAvailableActions('building', detail.status ?? '')
            return actions.length > 0 ? (
              <div className="detail-section">
                <div className="detail-section__title">Available Actions</div>
                <div className="status-update" style={{ flexWrap: 'wrap', gap: 8 }}>
                  {actions.map(action => (
                    <button
                      key={action.id}
                      type="button"
                      className="btn btn-secondary"
                      onClick={() => handleTransitionAction(action)}
                      disabled={updateStatusMutation.isPending}
                    >
                      {updateStatusMutation.isPending && pendingActionId === action.id ? 'Saving…' : action.label}
                    </button>
                  ))}
                </div>
                {updateStatusMutation.isError && (
                  <div className="form-error">Failed to update status.</div>
                )}
              </div>
            ) : null
          })()}

          <div className="detail-grid">
            <div className="detail-field">
              <div className="detail-field__label">Status</div>
              <div className="detail-field__value"><StatusBadge status={detail.status ?? ''} /></div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Year Built</div>
              <div className="detail-field__value mono">{detail.yearBuilt}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Assessed Value</div>
              <div className="detail-field__value mono">{fmtCurrency(Number(detail.assessedValue))}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Last Assessed</div>
              <div className="detail-field__value mono">{detail.lastAssessedDate ?? '—'}</div>
            </div>
            {detail.conditionNotes && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Condition Notes</div>
                <div className="detail-field__value">{detail.conditionNotes}</div>
              </div>
            )}
            <div className="detail-field full-span">
              <div className="detail-field__value">
                <NavIdLink id={detail.landParcelId ?? ''} to="/land-parcels" label="Open Land Parcel" />
              </div>
            </div>
            <div className="detail-field full-span">
              <div className="detail-field__label">Registered At</div>
              <div className="detail-field__value mono">
                {new Date(detail.registeredAt).toLocaleString()}
              </div>
            </div>
          </div>

          <div className="detail-section">
            <div className="detail-section__title">Record Assessment</div>
            <form onSubmit={handleAssessment}>
              <div className="form-grid" style={{ marginBottom: 12 }}>
                <div className="form-field">
                  <label className="form-label">Assessed Value ($)</label>
                  <input
                    className="form-input"
                    type="number"
                    min="0"
                    step="1"
                    value={assessment.assessedValue || ''}
                    onChange={e => setAssessment(a => ({ ...a, assessedValue: parseFloat(e.target.value) || 0 }))}
                    required
                  />
                </div>
                <div className="form-field">
                  <label className="form-label">Assessment Date</label>
                  <input
                    className="form-input"
                    type="date"
                    value={assessment.assessmentDate}
                    onChange={e => setAssessment(a => ({ ...a, assessmentDate: e.target.value }))}
                    required
                  />
                </div>
                <div className="form-field full">
                  <label className="form-label">Condition Notes (optional)</label>
                  <textarea
                    className="form-textarea"
                    value={assessment.conditionNotes}
                    onChange={e => setAssessment(a => ({ ...a, conditionNotes: e.target.value }))}
                    placeholder="Observations from the assessment…"
                  />
                </div>
              </div>
              <button
                type="submit"
                className="btn btn-secondary"
                disabled={assessmentMutation.isPending || !assessment.assessedValue}
              >
                {assessmentMutation.isPending ? 'Recording…' : 'Record Assessment'}
              </button>
              {assessmentMutation.isError && (
                <div className="form-error">Failed to record assessment.</div>
              )}
            </form>
          </div>
        </>
      )}
    </SlideOver>
  )
}
