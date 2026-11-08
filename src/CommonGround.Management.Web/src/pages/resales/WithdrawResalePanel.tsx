import { useState } from 'react'
import type { ResaleResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import { useWithdrawIntentToSell, type WithdrawIntentToSellForm } from '../../hooks/resales'
import { useToast } from '../../hooks/useToast'
import { trustToday } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  resale: ResaleResponse
  onClose: () => void
}

/** Records that the homeowner decided not to sell after all. */
export default function WithdrawResalePanel({ resale, onClose }: Props) {
  const [form, setForm] = useState<WithdrawIntentToSellForm>(() => ({ withdrawnDate: trustToday(), reason: '' }))
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useWithdrawIntentToSell()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({ resaleId: resale.id!, form })
      success('Intent to sell withdrawn')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to withdraw the intent to sell.'))
    }
  }

  return (
    <SlideOver
      title="Withdraw Intent to Sell"
      subtitle={resale.homeLabel ?? undefined}
      onClose={onClose}
      footer={
        <>
          <button type="button" className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button type="submit" form="withdraw-resale-form" className="btn btn-primary" disabled={mutation.isPending || !form.reason.trim()}>
            {mutation.isPending ? 'Withdrawing…' : 'Withdraw Intent'}
          </button>
        </>
      }
    >
      <form id="withdraw-resale-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="resale-withdrawn-date">Withdrawn Date</label>
            <DateInput
              id="resale-withdrawn-date"
              className="form-input"
              value={form.withdrawnDate}
              min={resale.noticeDate}
              max={trustToday()}
              onChange={v => setForm(f => ({ ...f, withdrawnDate: v }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="resale-withdraw-reason">Reason</label>
            <textarea
              id="resale-withdraw-reason"
              className="form-textarea"
              value={form.reason}
              onChange={e => setForm(f => ({ ...f, reason: e.target.value }))}
              placeholder="Why the homeowner decided not to sell…"
              required
            />
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
