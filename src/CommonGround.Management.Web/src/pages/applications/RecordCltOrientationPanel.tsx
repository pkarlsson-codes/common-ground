import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useRecordCltOrientation } from '../../hooks/applications'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

type Props = {
  householdId: string
  onClose: () => void
}

export default function RecordCltOrientationPanel({ householdId, onClose }: Props) {
  const [completedDate, setCompletedDate] = useState(new Date().toISOString().slice(0, 10))
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordCltOrientation()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        householdId,
        request: { completedDate },
      })
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
            <input
              className="form-input"
              type="date"
              value={completedDate}
              onChange={e => setCompletedDate(e.target.value)}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
