import { useState } from 'react'
import type { MortgageRequestResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useApproveMortgageRequest } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import MortgageDebtCheck from './MortgageDebtCheck'
import { loanTermsText, mortgageRequestTitle } from './mortgageRequestLetter'
import { formatPrice } from '../../lib/currency'

interface Props {
  request: MortgageRequestResponse
  onClose: () => void
}

/** Gives the trust's written permission for the loan (Art 8.1). Above the cap it needs a reason. */
export default function ApproveMortgageRequestPanel({ request, onClose }: Props) {
  const [reason, setReason] = useState('')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useApproveMortgageRequest()
  const { success } = useToast()
  const overCap = !!request.overCap

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (overCap && !reason.trim()) return
    setProblem(null)
    try {
      await mutation.mutateAsync({ request, reason })
      success('Request approved')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to approve the request.'))
    }
  }

  return (
    <SlideOver
      title="Approve Mortgage Request"
      subtitle={mortgageRequestTitle(request)}
      onClose={onClose}
      isDirty={reason !== ''}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="approve-mortgage-request-form"
            disabled={mutation.isPending || (overCap && !reason.trim())}
          >
            {mutation.isPending ? 'Approving…' : overCap ? 'Approve Above Cap' : 'Approve'}
          </button>
        </>
      }
    >
      <form id="approve-mortgage-request-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          {formatPrice(request.principal ?? 0)} at {loanTermsText(request)}. The approval is the trust's written
          permission for these terms only.
        </p>
        <MortgageDebtCheck request={request} />
        {overCap && (
          <div className="form-grid" style={{ marginTop: 14 }}>
            <p className="form-field full" style={{ fontSize: 13, margin: 0 }}>
              Total debt after closing is above the lease's cap of {request.maxMortgageDebtPercent}%.
            </p>
            <div className="form-field full">
              <label className="form-label" htmlFor="approve-mortgage-reason">Reason</label>
              <textarea
                id="approve-mortgage-reason"
                className="form-textarea"
                value={reason}
                onChange={e => setReason(e.target.value)}
                placeholder="Refinance of the first loan that lowers the monthly payment."
                required
              />
            </div>
          </div>
        )}
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
