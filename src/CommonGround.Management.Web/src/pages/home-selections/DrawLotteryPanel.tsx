import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useDrawSelectionLottery } from '../../hooks/homeSelections'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import PurchaseTermsFields from './PurchaseTermsFields'
import { emptyPurchaseTerms, purchaseTermsComplete, type PurchaseTerms } from './purchaseTerms'

type Props = {
  homeId: string
  /** The households staff marked as tied, or null to draw from the whole waiting list. */
  tiedApplicationIds: string[] | null
  waitingCount: number
  assessedValue: number | null | undefined
  onClose: () => void
}

export default function DrawLotteryPanel({ homeId, tiedApplicationIds, waitingCount, assessedValue, onClose }: Props) {
  const [terms, setTerms] = useState<PurchaseTerms>(emptyPurchaseTerms)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useDrawSelectionLottery()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        homeId,
        request: {
          tiedApplicationIds,
          groundLeaseTemplateId: terms.groundLeaseTemplateId,
          purchasePrice: Number(terms.purchasePrice),
          selectedDate: terms.selectedDate,
        },
      })
      success('Lottery drawn')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to draw the lottery.'))
    }
  }

  const poolDescription = tiedApplicationIds
    ? `Draws between the ${tiedApplicationIds.length} tied households you marked.`
    : `Draws from all ${waitingCount} households on the waiting list.`

  return (
    <SlideOver
      title="Draw Lottery"
      subtitle={poolDescription}
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="draw-lottery-form"
            disabled={mutation.isPending || !purchaseTermsComplete(terms)}
          >
            {mutation.isPending ? 'Drawing…' : 'Draw Lottery'}
          </button>
        </>
      }
    >
      <form id="draw-lottery-form" onSubmit={handleSubmit}>
        <div className="detail-field__value" style={{ marginBottom: 16 }}>
          Households that can't be chosen on the date of the draw are left out of it. The whole waiting list and
          the seed are recorded, so the draw can be checked.
        </div>
        <div className="form-grid">
          <PurchaseTermsFields value={terms} onChange={setTerms} assessedValue={assessedValue} />
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
