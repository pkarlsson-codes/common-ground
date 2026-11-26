import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useSetReminder } from '../../hooks/reminders'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { REMINDER_SECTIONS } from '../../lib/today'

/** The most characters a note holds, as the API allows. */
const NOTE_MAX_LENGTH = 500

type Props = {
  /** The Today section to preselect; none puts the reminder in the Reminders section. */
  section?: string | null
  onClose: () => void
}

/** Sets a reminder: a dated note that shows on Today from its date until someone marks it done. */
export default function NewReminderPanel({ section = null, onClose }: Props) {
  const [remindOn, setRemindOn] = useState<CalendarDate>(() => trustToday())
  const [note, setNote] = useState('')
  const [chosenSection, setChosenSection] = useState<string>(section ?? '')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useSetReminder()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({ remindOn, note: note.trim(), section: chosenSection || null })
      success('Reminder set')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Could not set the reminder.'))
    }
  }

  return (
    <SlideOver
      title="New reminder"
      subtitle="Shows on Today from its date until someone marks it done. Everyone in the trust sees it."
      onClose={onClose}
      isDirty={note.trim().length > 0}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="new-reminder-form"
            disabled={mutation.isPending || !note.trim() || !remindOn}
          >
            {mutation.isPending ? 'Setting…' : 'Set reminder'}
          </button>
        </>
      }
    >
      <form id="new-reminder-form" onSubmit={e => { void handleSubmit(e) }}>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="reminder-date">Date</label>
            <DateInput
              id="reminder-date"
              className="form-input"
              value={remindOn}
              min={trustToday()}
              onChange={setRemindOn}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="reminder-section">Section</label>
            <select
              id="reminder-section"
              className="form-select"
              value={chosenSection}
              onChange={e => setChosenSection(e.target.value)}
            >
              <option value="">None (Reminders)</option>
              {REMINDER_SECTIONS.map(s => <option key={s.area} value={s.area}>{s.label}</option>)}
            </select>
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="reminder-note">Note</label>
            <textarea
              id="reminder-note"
              className="form-textarea"
              value={note}
              maxLength={NOTE_MAX_LENGTH}
              placeholder="What to do, e.g. call the lender about the rate lock"
              onChange={e => setNote(e.target.value)}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
