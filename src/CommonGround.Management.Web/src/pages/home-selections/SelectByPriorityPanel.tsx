import { useState } from 'react'
import type { HomeSelectionCandidateDto } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { SELECTION_PRIORITIES, useSelectHouseholdByPriority } from '../../hooks/homeSelections'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import PurchaseTermsFields from './PurchaseTermsFields'
import { emptyPurchaseTerms, purchaseTermsComplete, type PurchaseTerms } from './purchaseTerms'

type Props = {
  selectionId: string
  candidate: HomeSelectionCandidateDto
  onClose: () => void
}

export default function SelectByPriorityPanel({ selectionId, candidate, onClose }: Props) {
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
        selectionId,
        request: {
          applicationId: candidate.applicationId!,
          priorities: SELECTION_PRIORITIES.map(p => p.value).filter(v => priorities.includes(v)),
          reason,
          groundLeaseTemplateId: terms.groundLeaseTemplateId,
          purchasePrice: Number(terms.purchasePrice),
          selectedDate: terms.selectedDate,
        },
      })
      success('Household selected')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to select household.'))
    }
  }

  return (
    <SlideOver
      title="Select by Priority"
      subtitle={candidate.primaryContact?.name ?? candidate.applicationId}
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
            {mutation.isPending ? 'Selecting…' : 'Select Household'}
          </button>
        </>
      }
    >
      <form id="select-by-priority-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Priorities That Justify This Choice</label>
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
          </div>
          <div className="form-field full">
            <label className="form-label">Reason</label>
            <textarea
              className="form-textarea"
              value={reason}
              onChange={e => setReason(e.target.value)}
              placeholder="Why this household over the other candidates…"
              required
            />
          </div>
          <PurchaseTermsFields value={terms} onChange={setTerms} />
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
