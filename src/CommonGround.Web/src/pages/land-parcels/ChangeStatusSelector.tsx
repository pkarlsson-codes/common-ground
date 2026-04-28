import type { TransitionAction } from '../../lib/transitionActions'

interface Props {
  actions: TransitionAction[]
  isPending: boolean
  pendingActionId: string | null
  isError: boolean
  errorMessage: string
  onSelectAction: (action: TransitionAction) => void
}

export default function ChangeStatusSelector({
  actions,
  isPending,
  pendingActionId,
  isError,
  errorMessage,
  onSelectAction,
}: Props) {
  if (actions.length === 0) {
    return null
  }

  return (
    <div className="detail-section">
      <div className="detail-section__title">Change Status</div>
      <div className="status-update" style={{ flexWrap: 'wrap', gap: 8 }}>
        {actions.map(action => (
          <button
            key={action.id}
            type="button"
            className="btn btn-secondary"
            onClick={() => onSelectAction(action)}
            disabled={isPending}
          >
            {isPending && pendingActionId === action.id ? 'Updating…' : action.label}
          </button>
        ))}
      </div>
      {isError && (
        <div className="form-error">{errorMessage}</div>
      )}
    </div>
  )
}