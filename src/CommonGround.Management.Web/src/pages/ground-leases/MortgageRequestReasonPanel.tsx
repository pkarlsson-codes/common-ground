import { useState } from 'react'
import type { MortgageRequestResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useDenyMortgageRequest, useWithdrawMortgageRequest } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { mortgageRequestTitle } from './mortgageRequestLetter'

interface Props {
  request: MortgageRequestResponse
  /** Deny: the trust refuses the loan. Withdraw: the homeowner dropped it. */
  action: 'deny' | 'withdraw'
  onClose: () => void
}

const TEXT = {
  deny: {
    title: 'Deny Mortgage Request',
    intro: 'The reason is printed in the letter to the homeowner.',
    placeholder: "Total debt after closing would be 112% of the purchase option price; the lease allows 90%.",
    button: 'Deny',
    pending: 'Denying…',
    toast: 'Request denied',
    failed: 'Failed to deny the request.',
  },
  withdraw: {
    title: 'Withdraw Mortgage Request',
    intro: 'For when the homeowner drops the loan. An approval no longer counts: a loan on the same terms needs a new request.',
    placeholder: 'Homeowner chose a different lender.',
    button: 'Withdraw',
    pending: 'Withdrawing…',
    toast: 'Request withdrawn',
    failed: 'Failed to withdraw the request.',
  },
}

/** Denies a request waiting for a decision, or withdraws an open one; both need a reason. */
export default function MortgageRequestReasonPanel({ request, action, onClose }: Props) {
  const [reason, setReason] = useState('')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const deny = useDenyMortgageRequest()
  const withdraw = useWithdrawMortgageRequest()
  const mutation = action === 'deny' ? deny : withdraw
  const { success } = useToast()
  const text = TEXT[action]

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!reason.trim()) return
    setProblem(null)
    try {
      if (action === 'deny') await deny.mutateAsync({ request, reason })
      else await withdraw.mutateAsync({ request, withdrawnOn: trustToday(), reason })
      success(text.toast)
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, text.failed))
    }
  }

  return (
    <SlideOver
      title={text.title}
      subtitle={mortgageRequestTitle(request)}
      onClose={onClose}
      isDirty={reason !== ''}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="mortgage-request-reason-form"
            disabled={mutation.isPending || !reason.trim()}
          >
            {mutation.isPending ? text.pending : text.button}
          </button>
        </>
      }
    >
      <form id="mortgage-request-reason-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>{text.intro}</p>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label" htmlFor="mortgage-request-reason">Reason</label>
            <textarea
              id="mortgage-request-reason"
              className="form-textarea"
              value={reason}
              onChange={e => setReason(e.target.value)}
              placeholder={text.placeholder}
              required
            />
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
