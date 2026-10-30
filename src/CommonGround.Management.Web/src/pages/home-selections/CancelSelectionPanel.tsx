import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useCancelHomeSelection } from '../../hooks/homeSelections'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

type Props = {
  selectionId: string
  onClose: () => void
}

export default function CancelSelectionPanel({ selectionId, onClose }: Props) {
  const [reason, setReason] = useState('')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useCancelHomeSelection()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({ selectionId, request: { reason } })
      success('Selection cancelled')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to cancel the selection.'))
    }
  }

  return (
    <SlideOver
      title="Cancel Selection"
      subtitle="Nobody is selected or passed over"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Back</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="cancel-selection-form"
            disabled={mutation.isPending || !reason.trim()}
          >
            {mutation.isPending ? 'Cancelling…' : 'Cancel Selection'}
          </button>
        </>
      }
    >
      <form id="cancel-selection-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Reason</label>
            <textarea
              className="form-textarea"
              value={reason}
              onChange={e => setReason(e.target.value)}
              placeholder="e.g. No candidate suits this home"
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
