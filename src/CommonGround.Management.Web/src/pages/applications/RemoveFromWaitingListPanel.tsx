import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import ConfirmActionModal from '../../components/ConfirmActionModal'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useRemoveFromWaitingList } from '../../hooks/applications'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { DateInput } from '../../components/DateInput'
import { WAITING_LIST_REMOVAL_REASONS } from './waitingListRemovalReasons'

type FormState = {
  reason: string
  note: string
  removedDate: CalendarDate
}

const emptyForm = (): FormState => ({
  reason: '',
  note: '',
  removedDate: trustToday(),
})

type Props = {
  applicationId: string
  approvedDate?: CalendarDate | null
  onClose: () => void
}

export default function RemoveFromWaitingListPanel({ applicationId, approvedDate, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const [confirming, setConfirming] = useState(false)
  const mutation = useRemoveFromWaitingList()
  const { success } = useToast()

  const noteRequired = form.reason === 'Other'
  const canSubmit = !!form.reason && !!form.removedDate && (!noteRequired || !!form.note.trim())

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setConfirming(true)
  }

  const handleConfirm = async () => {
    setConfirming(false)
    setProblem(null)

    try {
      await mutation.mutateAsync({
        applicationId,
        request: {
          reason: form.reason,
          note: form.note.trim() || null,
          removedDate: form.removedDate,
        },
      })
      success('Removed from waiting list')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to remove from waiting list.'))
    }
  }

  return (
    <SlideOver
      title="Remove from Waiting List"
      subtitle="Take this approved household off the waiting list"
      onClose={onClose}
      isDirty={!!form.reason || !!form.note || form.removedDate !== emptyForm().removedDate}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="remove-from-waiting-list-form"
            disabled={mutation.isPending || !canSubmit}
          >
            {mutation.isPending ? 'Submitting…' : 'Remove'}
          </button>
        </>
      }
    >
      <form id="remove-from-waiting-list-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <fieldset className="form-field full" style={{ border: 0, padding: 0, margin: 0 }}>
            <legend className="form-label">Reason</legend>
            <div role="radiogroup" aria-label="Reason" style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {WAITING_LIST_REMOVAL_REASONS.map(r => (
                <label key={r.value} style={{ display: 'flex', alignItems: 'flex-start', gap: 8, cursor: 'pointer' }}>
                  <input
                    type="radio"
                    name="removal-reason"
                    value={r.value}
                    checked={form.reason === r.value}
                    onChange={() => setForm(f => ({ ...f, reason: r.value }))}
                    style={{ marginTop: 3 }}
                  />
                  <span>
                    <span>{r.label}</span>
                    <span className="form-hint" style={{ display: 'block' }}>{r.hint}</span>
                  </span>
                </label>
              ))}
            </div>
          </fieldset>
          <div className="form-field full">
            <label className="form-label" htmlFor="removal-note">
              Note{noteRequired ? '' : ' (optional)'}
            </label>
            <textarea
              id="removal-note"
              className="form-textarea"
              value={form.note}
              onChange={e => setForm(f => ({ ...f, note: e.target.value }))}
              required={noteRequired}
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="removal-date">Removed Date</label>
            <DateInput
              id="removal-date"
              className="form-input"
              value={form.removedDate}
              onChange={v => setForm(f => ({ ...f, removedDate: v }))}
              min={approvedDate ?? undefined}
              max={trustToday()}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>

      <ConfirmActionModal
        open={confirming}
        title="Remove from Waiting List"
        message="Remove this household from the waiting list? This action cannot be undone."
        confirmLabel="Remove"
        confirmTone="danger"
        onConfirm={handleConfirm}
        onCancel={() => setConfirming(false)}
      />
    </SlideOver>
  )
}
