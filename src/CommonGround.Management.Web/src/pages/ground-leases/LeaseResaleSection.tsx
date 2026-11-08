import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { ResaleResponse } from '../../api'
import {
  NOTICE_CHANNEL_LABELS,
  NOTICE_CHANNELS,
  optionWindowLeft,
  resaleStep,
  useClosedResaleForLease,
  useOpenResaleForLease,
  useRecordIntentToSell,
  type NoticeChannel,
  type RecordIntentToSellForm,
} from '../../hooks/resales'
import { useToast } from '../../hooks/useToast'
import { DateInput } from '../../components/DateInput'
import { trustToday } from '../../lib/calendarDate'
import { getErrorMessage } from '../../lib/getErrorMessage'
import { formatStepDate } from '../../lib/lifecycle'

interface Props {
  groundLeaseId: string
  leaseActive: boolean
}

/** The lease's resale in progress, or the way to record the homeowner's notice to sell. */
export default function LeaseResaleSection({ groundLeaseId, leaseActive }: Props) {
  const { data: resale, isLoading } = useOpenResaleForLease(groundLeaseId)
  const { data: closed } = useClosedResaleForLease(groundLeaseId, !leaseActive)

  if (!leaseActive) {
    return closed ? (
      <div className="detail-section">
        <div className="detail-section__title">Resale</div>
        <ClosedResale resale={closed} />
      </div>
    ) : null
  }

  return (
    <div className="detail-section">
      <div className="detail-section__title">Resale</div>
      {isLoading ? null : resale ? <OpenResale resale={resale} /> : <NoResale groundLeaseId={groundLeaseId} />}
    </div>
  )
}

const emptyIntent = (): RecordIntentToSellForm => ({ noticeDate: trustToday(), channel: 'Phone', notes: '' })

function NoResale({ groundLeaseId }: { groundLeaseId: string }) {
  const mutation = useRecordIntentToSell()
  const { success } = useToast()
  const [form, setForm] = useState<RecordIntentToSellForm | null>(null)
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!form) return
    setError(null)
    try {
      await mutation.mutateAsync({ groundLeaseId, form })
      setForm(null)
      success('Intent to sell recorded')
    } catch (err) {
      setError(await getErrorMessage(err, 'Failed to record the intent to sell.'))
    }
  }

  if (!form) {
    return (
      <>
        <p style={{ fontSize: 13, color: 'var(--text-dim)', marginBottom: 12 }}>No resale in progress.</p>
        <button type="button" className="btn btn-secondary" onClick={() => setForm(emptyIntent())}>
          Record Intent to Sell
        </button>
      </>
    )
  }

  return (
    <form onSubmit={handleSubmit}>
      <div className="form-grid" style={{ marginBottom: 12 }}>
        <div className="form-field">
          <label className="form-label" htmlFor="resale-notice-date">Notice Date</label>
          <DateInput
            id="resale-notice-date"
            className="form-input"
            value={form.noticeDate}
            max={trustToday()}
            onChange={v => setForm(f => f && { ...f, noticeDate: v })}
            required
          />
        </div>
        <div className="form-field">
          <label className="form-label" htmlFor="resale-channel">Notice Came By</label>
          <select
            id="resale-channel"
            className="form-select"
            value={form.channel}
            onChange={e => setForm(f => f && { ...f, channel: e.target.value as NoticeChannel })}
          >
            {NOTICE_CHANNELS.map(c => <option key={c} value={c}>{NOTICE_CHANNEL_LABELS[c]}</option>)}
          </select>
        </div>
        <div className="form-field full">
          <label className="form-label" htmlFor="resale-notes">Notes</label>
          <textarea
            id="resale-notes"
            className="form-textarea"
            value={form.notes}
            onChange={e => setForm(f => f && { ...f, notes: e.target.value })}
            placeholder="Reason for selling, timing, anything the homeowner mentioned…"
          />
        </div>
      </div>
      <div style={{ display: 'flex', gap: 8 }}>
        <button type="submit" className="btn btn-primary" disabled={mutation.isPending}>
          {mutation.isPending ? 'Recording…' : 'Record Intent'}
        </button>
        <button type="button" className="btn btn-ghost" onClick={() => { setForm(null); setError(null) }}>Cancel</button>
      </div>
      {error && <div className="form-error">{error}</div>}
    </form>
  )
}

function OpenResale({ resale }: { resale: ResaleResponse }) {
  const navigate = useNavigate()

  return (
    <>
      <div className="detail-grid" style={{ marginBottom: 12 }}>
        <div className="detail-field">
          <div className="detail-field__label">Notice Date</div>
          <div className="detail-field__value mono">{resale.noticeDate}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Notice Came By</div>
          <div className="detail-field__value">{NOTICE_CHANNEL_LABELS[resale.noticeChannel ?? ''] ?? resale.noticeChannel}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Option Window End</div>
          <div className="detail-field__value mono">{resale.optionWindowEndsOn}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Option Window</div>
          <div className="detail-field__value">{resale.optionExercisedDate ? '' : optionWindowLeft(resale.optionWindowEndsOn!)}</div>
        </div>
        <div className="detail-field">
          <div className="detail-field__label">Step</div>
          <div className="detail-field__value">{resaleStep(resale)}</div>
        </div>
      </div>
      <button type="button" className="btn btn-secondary" onClick={() => navigate('/resales', { state: { openId: resale.id } })}>
        Open Resale
      </button>
    </>
  )
}

/** The resale that ended the lease, once the trust has bought the home back. */
function ClosedResale({ resale }: { resale: ResaleResponse }) {
  const navigate = useNavigate()

  return (
    <>
      <p style={{ fontSize: 13, color: 'var(--text-dim)', marginBottom: 12 }}>
        The trust bought this home on {formatStepDate(resale.closingDate)}.
      </p>
      <button type="button" className="btn btn-secondary" onClick={() => navigate('/resales', { state: { openId: resale.id } })}>
        Open Resale
      </button>
    </>
  )
}
