import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { HEIR_NOTICE_DAYS, useOpenHeirTransfer } from '../../hooks/groundLeases'
import { useHome } from '../../hooks/homes'
import { useToast } from '../../hooks/useToast'
import { daysBetween, trustToday, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
  /** Called with the new transfer's ID once it is opened. */
  onOpened: (heirTransferId: string) => void
}

/** Opens an heir transfer when the trust hears that the homeowner has died (Art 10.3). */
export default function OpenHeirTransferPanel({ lease, onClose, onOpened }: Props) {
  const today = trustToday()
  const { data: home } = useHome(lease.homeId ?? null)
  const [diedOn, setDiedOn] = useState<CalendarDate | null>(null)
  const [noticeReceivedOn, setNoticeReceivedOn] = useState<CalendarDate | null>(today)
  const [notifiedBy, setNotifiedBy] = useState('')
  const [contact, setContact] = useState('')
  const [notes, setNotes] = useState('')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useOpenHeirTransfer()
  const { success } = useToast()

  const datesValid = !!diedOn && !!noticeReceivedOn && diedOn <= today && noticeReceivedOn >= diedOn && noticeReceivedOn <= today
  const valid = datesValid && notifiedBy.trim() !== ''
  const late = datesValid && daysBetween(diedOn!, noticeReceivedOn!) > HEIR_NOTICE_DAYS

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!valid) return
    setProblem(null)
    try {
      const result = await mutation.mutateAsync({
        groundLeaseId: lease.id!,
        diedOn: diedOn!,
        noticeReceivedOn: noticeReceivedOn!,
        notifiedBy,
        notifiedByContact: contact,
        notes,
      })
      success('Heir transfer opened')
      onOpened(result.heirTransferId!)
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to open the heir transfer.'))
    }
  }

  return (
    <SlideOver
      title="Open Heir Transfer"
      subtitle={[lease.homeownerFullName, home?.label].filter(Boolean).join(' · ') || 'Ground lease'}
      onClose={onClose}
      isDirty={!!diedOn || notifiedBy !== '' || contact !== '' || notes !== '' || noticeReceivedOn !== today}
      footer={
        <>
          <SlideOverCancelButton />
          <button className="btn btn-primary" type="submit" form="open-heir-transfer-form" disabled={mutation.isPending || !valid}>
            {mutation.isPending ? 'Opening…' : 'Open Transfer'}
          </button>
        </>
      }
    >
      <form id="open-heir-transfer-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          The executor has {HEIR_NOTICE_DAYS} days from the death to notify the trust.
        </p>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="heir-died-on">Date of Death</label>
            <DateInput id="heir-died-on" className="form-input" value={diedOn} max={today} onChange={setDiedOn} required />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="heir-notice-received-on">Notice Received On</label>
            <DateInput
              id="heir-notice-received-on"
              className="form-input"
              value={noticeReceivedOn}
              min={diedOn ?? undefined}
              max={today}
              onChange={setNoticeReceivedOn}
              required
            />
          </div>
          {late && (
            <p className="form-hint full" role="status">
              Late notice: Art 10.3 asks for notice within {HEIR_NOTICE_DAYS} days.
            </p>
          )}
          <div className="form-field">
            <label className="form-label" htmlFor="heir-notified-by">Notified By</label>
            <input
              id="heir-notified-by"
              className="form-input"
              value={notifiedBy}
              onChange={e => setNotifiedBy(e.target.value)}
              placeholder="The executor's name"
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="heir-contact">Contact (optional)</label>
            <input id="heir-contact" className="form-input" value={contact} onChange={e => setContact(e.target.value)} />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="heir-notes">Notes (optional)</label>
            <textarea id="heir-notes" className="form-textarea" value={notes} onChange={e => setNotes(e.target.value)} />
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
