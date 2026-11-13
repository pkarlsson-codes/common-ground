import { useState } from 'react'
import type { PermittedMortgageDto } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useEditPermittedMortgage } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { LenderFields } from './PermittedMortgageFields'

interface Props {
  groundLeaseId: string
  mortgage: PermittedMortgageDto
  onClose: () => void
}

/** Changes a permitted mortgage's lender and notice address, for when servicing moves or the address turns up. */
export default function EditPermittedMortgagePanel({ groundLeaseId, mortgage, onClose }: Props) {
  const [lender, setLender] = useState(mortgage.lender ?? '')
  const [noticeAddress, setNoticeAddress] = useState(mortgage.noticeAddress ?? '')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useEditPermittedMortgage()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({ groundLeaseId, mortgageId: mortgage.mortgageId!, form: { lender, noticeAddress } })
      success('Mortgage updated')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to update the mortgage.'))
    }
  }

  return (
    <SlideOver
      title="Edit Permitted Mortgage"
      subtitle={`${mortgage.lender} · ${formatPrice(mortgage.originalPrincipal ?? 0)}`}
      onClose={onClose}
      isDirty={lender !== (mortgage.lender ?? '') || noticeAddress !== (mortgage.noticeAddress ?? '')}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="edit-permitted-mortgage-form"
            disabled={mutation.isPending || !lender.trim()}
          >
            {mutation.isPending ? 'Saving…' : 'Save'}
          </button>
        </>
      }
    >
      <form id="edit-permitted-mortgage-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <LenderFields
            lender={lender}
            noticeAddress={noticeAddress}
            onLenderChange={setLender}
            onNoticeAddressChange={setNoticeAddress}
          />
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
