import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { DateInput } from '../../components/DateInput'
import { useConfirmQualification, useHomeownerApplication } from '../../hooks/applications'
import { useToast } from '../../hooks/useToast'
import { formatCalendarDate, trustToday, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { qualificationChanges } from './qualification'

type Props = {
  applicationId: string
  onClose: () => void
}

/**
 * Confirms an approved household still qualifies on its changed income or size. Staff check the figures outside the
 * app, as at approval; a household that no longer qualifies is removed from the waiting list instead.
 */
export default function ConfirmQualificationPanel({ applicationId, onClose }: Props) {
  const { data: application } = useHomeownerApplication(applicationId)
  const [confirmedDate, setConfirmedDate] = useState<CalendarDate>(trustToday)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useConfirmQualification()
  const { success } = useToast()

  const changes = application ? qualificationChanges(application) : []
  const since = application?.qualificationToConfirmSince

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({ applicationId, request: { confirmedDate } })
      success('Qualification confirmed')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to confirm qualification.'))
    }
  }

  return (
    <SlideOver
      title="Confirm Qualification"
      subtitle={application?.primaryContact?.name ?? 'Check the household still qualifies'}
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="confirm-qualification-form"
            disabled={mutation.isPending || !confirmedDate}
          >
            {mutation.isPending ? 'Confirming…' : 'Confirm Still Qualifies'}
          </button>
        </>
      }
    >
      <form id="confirm-qualification-form" onSubmit={handleSubmit}>
        <div className="detail-section" style={{ marginBottom: 16 }}>
          <div className="detail-section__title">
            {since ? `Changed since ${formatCalendarDate(since)}` : 'What changed'}
          </div>
          {changes.length > 0
            ? changes.map(change => <div key={change} className="detail-field__value">{change}</div>)
            : <div className="detail-field__value">—</div>}
          <div className="form-hint" style={{ marginTop: 8 }}>
            Check the new figures against the eligibility the trust approved this household on. The household keeps its
            place on the waiting list. If it no longer qualifies, remove it from the waiting list instead.
          </div>
        </div>

        <div className="form-grid">
          <div className="form-field">
            <label className="form-label">Confirmed Date</label>
            <DateInput
              className="form-input"
              value={confirmedDate}
              onChange={setConfirmedDate}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
