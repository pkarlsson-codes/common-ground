import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import {
  NOTICE_DELIVERIES,
  NOTICE_DELIVERY_LABELS,
  NOTICE_LEVEL_LABELS,
  NOTICE_LEVELS,
  useRecordLeaseFeeNotice,
  type NoticeDelivery,
  type NoticeLevel,
  type RecordLeaseFeeNoticeForm,
} from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { formatBalance } from './leaseFeeFormat'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
}

/** Records that a late notice was sent on one lease; the level defaults to the one due. */
export default function RecordLeaseFeeNoticePanel({ lease, onClose }: Props) {
  const due = (lease.leaseFees?.noticeDue ?? 'First') as NoticeLevel
  const [form, setForm] = useState<RecordLeaseFeeNoticeForm>(() => ({
    level: due,
    sentOn: trustToday(),
    delivery: 'FirstClassMail',
  }))
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordLeaseFeeNotice()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({ groundLeaseId: lease.id!, form })
      success('Notice recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the notice.'))
    }
  }

  return (
    <SlideOver
      title="Record Notice"
      subtitle={`${lease.homeownerFullName ?? 'Ground lease'} · ${formatBalance(lease.leaseFees?.statement?.balance ?? 0)}`}
      onClose={onClose}
      isDirty={form.level !== due || form.sentOn !== trustToday() || form.delivery !== 'FirstClassMail'}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="record-lease-fee-notice-form"
            disabled={mutation.isPending || !form.sentOn}
          >
            {mutation.isPending ? 'Recording…' : 'Record Notice'}
          </button>
        </>
      }
    >
      <form id="record-lease-fee-notice-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="notice-level">Level</label>
            <select
              id="notice-level"
              className="form-select"
              value={form.level}
              onChange={e => setForm(f => ({ ...f, level: e.target.value as NoticeLevel }))}
            >
              {NOTICE_LEVELS.map(l => <option key={l} value={l}>{NOTICE_LEVEL_LABELS[l]}</option>)}
            </select>
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="notice-sent-on">Date Sent</label>
            <DateInput
              id="notice-sent-on"
              className="form-input"
              value={form.sentOn}
              min={lease.leaseStartDate}
              max={trustToday()}
              onChange={v => setForm(f => ({ ...f, sentOn: v }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="notice-delivery">Delivery</label>
            <select
              id="notice-delivery"
              className="form-select"
              value={form.delivery}
              onChange={e => setForm(f => ({ ...f, delivery: e.target.value as NoticeDelivery }))}
            >
              {NOTICE_DELIVERIES.map(d => <option key={d} value={d}>{NOTICE_DELIVERY_LABELS[d]}</option>)}
            </select>
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
