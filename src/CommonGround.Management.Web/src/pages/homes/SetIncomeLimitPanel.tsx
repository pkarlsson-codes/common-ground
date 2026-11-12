import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useSetHomeIncomeLimit } from '../../hooks/homes'
import { useIncomeLimitSchedules } from '../../hooks/incomeLimits'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

type Props = {
  homeId: string
  currentPercentOfAmi: number | null
  onClose: () => void
}

/** Why a selection for a home with this limit couldn't start today, or null when it could. */
function useMissingLevelWarning(percentOfAmi: number | null) {
  const { data: schedules } = useIncomeLimitSchedules()
  if (percentOfAmi == null || !schedules) return null

  const inEffect = schedules.find(s => s.isInEffect)
  if (!inEffect) {
    return 'No income limits are in effect today. A selection for this home can’t start until you enter them under Income Limits.'
  }
  if (!(inEffect.levels ?? []).some(l => l.percentOfAmi === percentOfAmi)) {
    return `The income limits in effect have no ${percentOfAmi}% of AMI level. A selection for this home can’t start until you enter it under Income Limits.`
  }
  return null
}

export default function SetIncomeLimitPanel({ homeId, currentPercentOfAmi, onClose }: Props) {
  const [percent, setPercent] = useState(currentPercentOfAmi ?? 80)
  const [noIncomeLimit, setNoIncomeLimit] = useState(currentPercentOfAmi == null)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useSetHomeIncomeLimit()
  const { success } = useToast()

  const chosen = noIncomeLimit ? null : percent
  const warning = useMissingLevelWarning(chosen != null && chosen >= 1 && chosen <= 200 ? chosen : null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({ homeId, percentOfAmi: chosen })
      success('Income limit saved')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to save the income limit.'))
    }
  }

  return (
    <SlideOver
      title="Set Income Limit"
      subtitle="The most a buying household may earn, as a percent of area median income"
      onClose={onClose}
      isDirty={chosen !== currentPercentOfAmi}
      footer={
        <>
          <SlideOverCancelButton />
          <button className="btn btn-primary" type="submit" form="set-income-limit-form" disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </button>
        </>
      }
    >
      <form id="set-income-limit-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label" htmlFor="set-income-limit-percent">Percent of AMI</label>
            <input
              id="set-income-limit-percent"
              className="form-input"
              type="number"
              min="1"
              max="200"
              value={percent}
              onChange={e => setPercent(parseInt(e.target.value, 10) || 0)}
              disabled={noIncomeLimit}
              required={!noIncomeLimit}
            />
            <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <input type="checkbox" checked={noIncomeLimit} onChange={e => setNoIncomeLimit(e.target.checked)} />
              No income limit
            </label>
          </div>
          {warning && (
            <div className="form-field full">
              <div
                role="status"
                style={{
                  padding: '10px 12px',
                  borderRadius: 8,
                  border: '1px solid var(--warning)',
                  background: 'var(--warning-surface)',
                  color: 'var(--warning)',
                  fontSize: 13,
                }}
              >
                {warning}
              </div>
            </div>
          )}
          <p className="form-hint full">
            A selection already open for this home keeps the limit it started with. Cancel it and start a new one to
            judge the candidates by the new limit.
          </p>
        </div>
        {problem && <ApiProblemAlert problem={problem} />}
      </form>
    </SlideOver>
  )
}
