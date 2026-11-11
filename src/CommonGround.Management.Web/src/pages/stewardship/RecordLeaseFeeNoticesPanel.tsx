import { useState } from 'react'
import type { LeaseFeeBalanceResponse } from '../../api'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import {
  DEFAULT_NOTICE_DELIVERIES,
  NOTICE_DELIVERIES,
  NOTICE_DELIVERY_LABELS,
  NOTICE_LEVEL_LABELS,
  useLeaseFeeNoticesDue,
  useRecordLeaseFeeNotice,
  type NoticeDelivery,
  type NoticeLevel,
} from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem } from '../../lib/getErrorMessage'

interface Props {
  onClose: () => void
}

interface Failure {
  homeowner: string
  reason: string
}

/**
 * Records the notices of a print run as sent, one per ticked lease, all with the same date and delivery. A notice of
 * default needs certified or registered mail, or hand delivery, so the run can't be recorded with another delivery
 * while one is ticked. A lease
 * that fails (paid up since printing, say) is listed with why; the others are still recorded.
 */
export default function RecordLeaseFeeNoticesPanel({ onClose }: Props) {
  const { data, isLoading } = useLeaseFeeNoticesDue()
  const notices = data?.notices ?? []
  const [unticked, setUnticked] = useState<Set<string>>(() => new Set())
  const [sentOn, setSentOn] = useState<CalendarDate>(() => trustToday())
  const [delivery, setDelivery] = useState<NoticeDelivery>('FirstClassMail')
  const [failures, setFailures] = useState<Failure[]>([])
  const [pending, setPending] = useState(false)
  const mutation = useRecordLeaseFeeNotice()
  const { success } = useToast()

  const ticked = notices.filter(n => !unticked.has(n.groundLeaseId!))
  const defaultsTicked = ticked.some(n => n.noticeDue === 'Default')
  const deliveryRefused = defaultsTicked && !DEFAULT_NOTICE_DELIVERIES.includes(delivery)

  const toggle = (id: string) => setUnticked(prev => {
    const next = new Set(prev)
    if (next.has(id)) next.delete(id)
    else next.add(id)
    return next
  })

  const record = async (n: LeaseFeeBalanceResponse): Promise<Failure | null> => {
    try {
      await mutation.mutateAsync({
        groundLeaseId: n.groundLeaseId!,
        form: { level: (n.noticeDue ?? 'First') as NoticeLevel, sentOn, delivery },
      })
      return null
    } catch (err) {
      const problem = await getApiProblem(err, 'Failed to record the notice.')
      return { homeowner: n.homeownerFullName ?? n.groundLeaseId!, reason: problem.message }
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setPending(true)
    setFailures([])
    const results = await Promise.all(ticked.map(record))
    setPending(false)
    const failed = results.filter((r): r is Failure => r !== null)
    const recorded = ticked.length - failed.length
    if (recorded > 0) success(recorded === 1 ? '1 notice recorded' : `${recorded} notices recorded`)
    if (failed.length === 0) onClose()
    else setFailures(failed)
  }

  const count = ticked.length

  return (
    <SlideOver
      title="Record Notices Sent"
      subtitle={data ? `${notices.length === 1 ? '1 notice' : `${notices.length} notices`} due` : undefined}
      onClose={onClose}
      isDirty={unticked.size > 0 || sentOn !== trustToday() || delivery !== 'FirstClassMail'}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="record-lease-fee-notices-form"
            disabled={pending || count === 0 || !sentOn || deliveryRefused}
          >
            {pending ? 'Recording…' : count === 1 ? 'Record 1 Notice' : `Record ${count} Notices`}
          </button>
        </>
      }
    >
      <form id="record-lease-fee-notices-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="notices-sent-on">Date Sent</label>
            <DateInput
              id="notices-sent-on"
              className="form-input"
              value={sentOn}
              max={trustToday()}
              onChange={setSentOn}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="notices-delivery">Delivery</label>
            <select
              id="notices-delivery"
              className="form-select"
              value={delivery}
              onChange={e => setDelivery(e.target.value as NoticeDelivery)}
            >
              {NOTICE_DELIVERIES.map(d => <option key={d} value={d}>{NOTICE_DELIVERY_LABELS[d]}</option>)}
            </select>
            {deliveryRefused && (
              <div className="form-help" style={{ color: 'var(--red)' }}>
                Notices of default must go by certified or registered mail, or by hand. Choose one of those, or untick them
                and record them separately.
              </div>
            )}
          </div>
        </div>

        {isLoading ? (
          <div className="skeleton-rows"><div className="skeleton-row" /></div>
        ) : notices.length === 0 ? (
          <p style={{ fontSize: 13, color: 'var(--text-dim)' }}>No notices are due.</p>
        ) : (
          <div className="registry-table-wrap" style={{ marginTop: 16 }}>
            <table className="registry-table" aria-label="Notices to record">
              <thead>
                <tr>
                  <th><span className="sr-only">Sent</span></th>
                  <th>Homeowner</th>
                  <th>Notice</th>
                  <th>Balance</th>
                </tr>
              </thead>
              <tbody>
                {notices.map(n => (
                  <tr key={n.groundLeaseId}>
                    <td>
                      <input
                        type="checkbox"
                        aria-label={`Sent to ${n.homeownerFullName ?? 'homeowner'}`}
                        checked={!unticked.has(n.groundLeaseId!)}
                        onChange={() => toggle(n.groundLeaseId!)}
                      />
                    </td>
                    <td>{n.homeownerFullName ?? '—'}</td>
                    <td>{NOTICE_LEVEL_LABELS[n.noticeDue ?? ''] ?? n.noticeDue}</td>
                    <td className="cell-mono">{formatPrice(n.balance ?? 0)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {failures.length > 0 && (
          <div className="form-error" role="alert" style={{ marginTop: 12 }}>
            {failures.length === 1 ? '1 notice was not recorded:' : `${failures.length} notices were not recorded:`}
            <ul style={{ margin: '4px 0 0 16px' }}>
              {failures.map(f => <li key={f.homeowner}>{f.homeowner}: {f.reason}</li>)}
            </ul>
          </div>
        )}
      </form>
    </SlideOver>
  )
}
