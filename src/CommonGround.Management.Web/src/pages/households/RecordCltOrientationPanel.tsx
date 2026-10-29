import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useRecordCltOrientation } from '../../hooks/households'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { trustToday } from '../../lib/calendarDate'
import { DateInput } from '../../components/DateInput'

type Props = {
  householdId: string
  onClose: () => void
}

export default function RecordCltOrientationPanel({ householdId, onClose }: Props) {
  const [completedDate, setCompletedDate] = useState(trustToday())
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordCltOrientation(householdId)
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({ completedDate })
      success('CLT orientation recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record CLT orientation.'))
    }
  }

  return (
    <SlideOver
      title="Record CLT Orientation"
      subtitle="Capture orientation completion date"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="record-clt-orientation-form"
            disabled={mutation.isPending || !completedDate}
          >
            {mutation.isPending ? 'Recording…' : 'Record Orientation'}
          </button>
        </>
      }
    >
      <form id="record-clt-orientation-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label">Completed Date</label>
            <DateInput
              className="form-input"
              value={completedDate}
              onChange={v => setCompletedDate(v)}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
