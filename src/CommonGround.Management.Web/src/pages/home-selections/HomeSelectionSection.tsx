import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import StatusBadge from '../../components/StatusBadge'
import { homeSelectionPath, useHomeSelectionsForHome, useStartHomeSelection } from '../../hooks/homeSelections'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

type Props = {
  homeId: string
  homeStatus: string
}

/**
 * The home's open or completed selection, if any, otherwise a way to start one while the home
 * is available. Cancelled selections are left out: the home can be selected for again.
 */
export default function HomeSelectionSection({ homeId, homeStatus }: Props) {
  const navigate = useNavigate()
  const { data: selections, isLoading } = useHomeSelectionsForHome(homeId)
  const startMutation = useStartHomeSelection()
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)

  const active = selections?.find(s => s.status !== 'Cancelled')
  const cancelledCount = (selections ?? []).filter(s => s.status === 'Cancelled').length

  const handleStart = async () => {
    setProblem(null)
    try {
      const { selectionId } = await startMutation.mutateAsync(homeId)
      navigate(homeSelectionPath(selectionId!))
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to start the selection.'))
    }
  }

  if (isLoading) return null
  if (!active && homeStatus !== 'Available') return null

  return (
    <div className="detail-section" style={{ marginTop: 16 }}>
      <div className="detail-section__title">Home Selection</div>
      {active ? (
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <StatusBadge status={active.status ?? ''} />
          <span className="detail-field__value">
            {active.candidateCount} candidate{active.candidateCount !== 1 ? 's' : ''} · started {active.startedAt?.toLocaleDateString()}
          </span>
          <button
            type="button"
            className="btn btn-secondary btn-sm"
            style={{ marginLeft: 'auto' }}
            onClick={() => navigate(homeSelectionPath(active.id!))}
          >
            Open Selection
          </button>
        </div>
      ) : (
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <span className="detail-field__value">
            Choose a household from the waiting list for this home.
            {cancelledCount > 0 && ` ${cancelledCount} earlier selection${cancelledCount !== 1 ? 's were' : ' was'} cancelled.`}
          </span>
          <button
            type="button"
            className="btn btn-primary btn-sm"
            style={{ marginLeft: 'auto' }}
            disabled={startMutation.isPending}
            onClick={() => { void handleStart() }}
          >
            {startMutation.isPending ? 'Starting…' : 'Start Selection'}
          </button>
        </div>
      )}
      <ApiProblemAlert problem={problem} />
    </div>
  )
}
