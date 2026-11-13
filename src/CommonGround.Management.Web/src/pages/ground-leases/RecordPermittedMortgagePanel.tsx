import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useRecordPermittedMortgage } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { LenderFields } from './PermittedMortgageFields'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
}

/** Records a mortgage the trust already permitted that isn't on the lease yet (Art 8.1). */
export default function RecordPermittedMortgagePanel({ lease, onClose }: Props) {
  const [lender, setLender] = useState('')
  const [noticeAddress, setNoticeAddress] = useState('')
  const [originalPrincipal, setOriginalPrincipal] = useState(0)
  const [permittedOn, setPermittedOn] = useState<CalendarDate | null>(null)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordPermittedMortgage()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!permittedOn) return
    setProblem(null)
    try {
      await mutation.mutateAsync({
        groundLeaseId: lease.id!,
        form: { lender, noticeAddress, originalPrincipal, permittedOn },
      })
      success('Mortgage recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the mortgage.'))
    }
  }

  return (
    <SlideOver
      title="Record Permitted Mortgage"
      subtitle={lease.homeownerFullName ?? 'Ground lease'}
      onClose={onClose}
      isDirty={lender !== '' || noticeAddress !== '' || originalPrincipal !== 0 || permittedOn !== null}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="record-permitted-mortgage-form"
            disabled={mutation.isPending || !lender.trim() || originalPrincipal <= 0 || !permittedOn}
          >
            {mutation.isPending ? 'Recording…' : 'Record Mortgage'}
          </button>
        </>
      }
    >
      <form id="record-permitted-mortgage-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          For a loan the trust already permitted in writing. A new loan goes through a mortgage request.
        </p>
        <div className="form-grid">
          <LenderFields
            lender={lender}
            noticeAddress={noticeAddress}
            onLenderChange={setLender}
            onNoticeAddressChange={setNoticeAddress}
          />
          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-principal">Original Principal ($)</label>
            <input
              id="mortgage-principal"
              className="form-input"
              type="number"
              min="0.01"
              step="0.01"
              value={originalPrincipal || ''}
              onChange={e => setOriginalPrincipal(parseFloat(e.target.value) || 0)}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-permitted-on">Permitted On</label>
            <DateInput
              id="mortgage-permitted-on"
              className="form-input"
              value={permittedOn}
              min={lease.leaseStartDate}
              max={trustToday()}
              onChange={setPermittedOn}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
