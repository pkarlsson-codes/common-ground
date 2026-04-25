import { useState } from 'react'
import { useLandParcel, useUpdateLandParcelStatus } from '../../hooks/landParcels'
import SlideOver from '../../components/SlideOver'
import StatusBadge from '../../components/StatusBadge'
import { getAvailableActions, type TransitionAction } from '../../lib/transitionActions'
import { trackTransitionEvent } from '../../lib/telemetry'
import { useToast } from '../../hooks/useToast'

function SkeletonRows() {
  return (
    <div className="skeleton-rows">
      {Array.from({ length: 7 }).map((_, i) => <div key={i} className="skeleton-row" />)}
    </div>
  )
}

interface Props {
  parcelId: string
  onClose: () => void
}

export default function LandParcelDetailPanel({ parcelId, onClose }: Props) {
  const { data: detail, isLoading: detailLoading } = useLandParcel(parcelId)
  const updateStatusMutation = useUpdateLandParcelStatus()
  const [pendingActionId, setPendingActionId] = useState<string | null>(null)
  const { success } = useToast()

  const handleTransitionAction = async (action: TransitionAction) => {
    if (!detail?.status || !action.toState) return

    if (action.requiresConfirmation && !window.confirm(action.confirmationMessage ?? 'Are you sure?')) {
      return
    }

    trackTransitionEvent({
      event: 'ui.transition',
      aggregate: 'parcel',
      actionId: action.id,
      entityId: parcelId,
      fromState: detail.status,
      toState: action.toState,
      outcome: 'attempt',
    })

    setPendingActionId(action.id)

    try {
      await updateStatusMutation.mutateAsync({ id: parcelId, newStatus: action.toState })
      success(`${action.label} complete`)
      trackTransitionEvent({
        event: 'ui.transition',
        aggregate: 'parcel',
        actionId: action.id,
        entityId: parcelId,
        fromState: detail.status,
        toState: action.toState,
        outcome: 'success',
      })
    } catch (error) {
      trackTransitionEvent({
        event: 'ui.transition',
        aggregate: 'parcel',
        actionId: action.id,
        entityId: parcelId,
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
      title={detail?.legalParcelNumber ?? '…'}
      subtitle={detail?.address}
      onClose={onClose}
    >
      {detailLoading || !detail ? <SkeletonRows /> : (
        <>
          {(() => {
            const actions = getAvailableActions('parcel', detail.status)
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
              <div className="detail-field__label">Area</div>
              <div className="detail-field__value mono">{Number(detail.areaAcres).toFixed(4)} ac</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Acquisition Method</div>
              <div className="detail-field__value">{detail.acquisitionMethod}</div>
            </div>
            <div className="detail-field">
              <div className="detail-field__label">Acquisition Date</div>
              <div className="detail-field__value mono">{detail.acquisitionDate}</div>
            </div>
            <div className="detail-field full-span">
              <div className="detail-field__label">Registered By</div>
              <div className="detail-field__value mono">{detail.registeredBy}</div>
            </div>
            {detail.covenantTerms && (
              <div className="detail-field full-span">
                <div className="detail-field__label">Covenant Terms</div>
                <div className="detail-field__value">{detail.covenantTerms}</div>
              </div>
            )}
            <div className="detail-field full-span">
              <div className="detail-field__label">Registered At</div>
              <div className="detail-field__value mono">
                {new Date(detail.registeredAt).toLocaleString()}
              </div>
            </div>
          </div>
        </>
      )}
    </SlideOver>
  )
}
