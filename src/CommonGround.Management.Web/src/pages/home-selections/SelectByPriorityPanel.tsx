import { useState } from 'react'
import type { WaitingListComparisonEntryDto } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { SELECTION_PRIORITIES, useSelectHouseholdByPriority } from '../../hooks/homeSelections'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import PurchaseTermsFields from './PurchaseTermsFields'
import { emptyPurchaseTerms, purchaseTermsComplete, type PurchaseTerms } from './purchaseTerms'

type Props = {
  homeId: string
  entry: WaitingListComparisonEntryDto
  assessedValue: number | null | undefined
  onClose: () => void
}

export default function SelectByPriorityPanel({ homeId, entry, assessedValue, onClose }: Props) {
  const [priorities, setPriorities] = useState<string[]>([])
  const [reason, setReason] = useState('')
  const [terms, setTerms] = useState<PurchaseTerms>(emptyPurchaseTerms)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useSelectHouseholdByPriority()
  const { success } = useToast()

  const togglePriority = (value: string, checked: boolean) =>
    setPriorities(p => (checked ? [...p, value] : p.filter(v => v !== value)))

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        homeId,
        request: {
          applicationId: entry.applicationId!,
          priorities: SELECTION_PRIORITIES.map(p => p.value).filter(v => priorities.includes(v)),
          reason,
          groundLeaseTemplateId: terms.groundLeaseTemplateId,
          purchasePrice: Number(terms.purchasePrice),
          selectedDate: terms.selectedDate,
        },
      })
      success('Household chosen')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to choose the household.'))
    }
  }

  return (
    <SlideOver
      title="Choose by Priority"
      subtitle={entry.primaryContact?.name ?? entry.applicationId}
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="select-by-priority-form"
            disabled={mutation.isPending || priorities.length === 0 || !reason.trim() || !purchaseTermsComplete(terms)}
          >
            {mutation.isPending ? 'Choosing…' : 'Choose Household'}
          </button>
        </>
      }
    >
      <form id="select-by-priority-form" onSubmit={handleSubmit}>
        <div className="detail-field__value" style={{ marginBottom: 16 }}>
          The waiting list is judged again on the date of the choice, and recorded with it.
        </div>
        <div className="form-grid">
          <fieldset className="form-field full" style={{ border: 0, padding: 0, margin: 0 }}>
            <legend className="form-label" style={{ marginBottom: 6 }}>Priorities That Justify This Choice</legend>
            {SELECTION_PRIORITIES.map(p => (
              <label key={p.value} style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 13 }}>
                <input
                  type="checkbox"
                  checked={priorities.includes(p.value)}
                  onChange={e => togglePriority(p.value, e.target.checked)}
                />
                {p.label}
              </label>
            ))}
          </fieldset>
          <div className="form-field full">
            <label className="form-label" htmlFor="selection-reason">Reason</label>
            <textarea
              id="selection-reason"
              className="form-textarea"
              value={reason}
              onChange={e => setReason(e.target.value)}
              placeholder="Why this household over the others on the waiting list…"
              required
            />
          </div>
          <PurchaseTermsFields
            value={terms}
            onChange={setTerms}
            assessedValue={assessedValue}
            prequalifiedAmount={entry.prequalifiedAmount ?? null}
          />
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
