import { useState } from 'react'
import type { MortgageRequestResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useRecordMortgageRequestAppraisal, type LoanAppraisalForm } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import LoanAppraisalFields from './LoanAppraisalFields'
import { appraisalValid, mortgageRequestTitle } from './mortgageRequestLetter'

interface Props {
  request: MortgageRequestResponse
  indexBased: boolean
  onClose: () => void
}

/** Adds the loan's appraisal, and closing costs if they came with it, to a request waiting for a decision. */
export default function AddMortgageAppraisalPanel({ request, indexBased, onClose }: Props) {
  const [appraisal, setAppraisal] = useState<LoanAppraisalForm>({ appraisedValue: 0, appraisalDate: null, currentIndexValue: 0 })
  const [closingCosts, setClosingCosts] = useState(request.closingCosts ?? 0)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordMortgageRequestAppraisal()
  const { success } = useToast()
  const valid = appraisalValid(appraisal, indexBased, true)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!valid) return
    setProblem(null)
    try {
      await mutation.mutateAsync({ request, appraisal, closingCosts, indexBased })
      success('Appraisal added')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to add the appraisal.'))
    }
  }

  return (
    <SlideOver
      title="Add Appraisal"
      subtitle={mortgageRequestTitle(request)}
      onClose={onClose}
      isDirty={appraisal.appraisedValue !== 0 || appraisal.appraisalDate !== null}
      footer={
        <>
          <SlideOverCancelButton />
          <button className="btn btn-primary" type="submit" form="add-mortgage-appraisal-form" disabled={mutation.isPending || !valid}>
            {mutation.isPending ? 'Saving…' : 'Add Appraisal'}
          </button>
        </>
      }
    >
      <form id="add-mortgage-appraisal-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          The appraisal the lender ordered for this loan. The debt check works out the purchase option price from it.
        </p>
        <div className="form-grid">
          <LoanAppraisalFields appraisal={appraisal} indexBased={indexBased} required onChange={setAppraisal} />
          <div className="form-field">
            <label className="form-label" htmlFor="appraisal-closing-costs">Closing Costs ($)</label>
            <input
              id="appraisal-closing-costs"
              className="form-input"
              type="number"
              min="0"
              step="0.01"
              value={closingCosts || ''}
              onChange={e => setClosingCosts(parseFloat(e.target.value) || 0)}
              aria-describedby="appraisal-closing-costs-hint"
            />
            <p id="appraisal-closing-costs-hint" className="form-hint">Optional.</p>
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
