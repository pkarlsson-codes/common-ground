import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useSetInsuranceRequirements } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { formatCalendarDate, trustToday, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import OptionalAmountField from './OptionalAmountField'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
}

/**
 * Changes the insurance the lease requires (Art 9.4) from a day on. The first allowed day is the lease start or the
 * latest change, whichever is later; a change can't be dated before an earlier one.
 */
export default function SetInsuranceRequirementsPanel({ lease, onClose }: Props) {
  const insurance = lease.insurance
  const initialReplacement = insurance?.requiredReplacementValue ?? null
  const initialLiability = insurance?.requiredLiabilityPerOccurrence ?? null
  const earliest = earliestEffectiveFrom(lease)
  const today = trustToday()
  const initialEffectiveFrom = earliest && earliest > today ? earliest : today

  const [replacementValue, setReplacementValue] = useState<number | null>(initialReplacement)
  const [liabilityPerOccurrence, setLiabilityPerOccurrence] = useState<number | null>(initialLiability)
  const [effectiveFrom, setEffectiveFrom] = useState<CalendarDate | null>(initialEffectiveFrom)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useSetInsuranceRequirements()
  const { success } = useToast()

  // The lease ignores a change that keeps the same amounts, so Save waits for one.
  const changed = replacementValue !== initialReplacement || liabilityPerOccurrence !== initialLiability
  const isDirty = changed || effectiveFrom !== initialEffectiveFrom

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!effectiveFrom) return
    setProblem(null)
    try {
      await mutation.mutateAsync({ groundLeaseId: lease.id!, replacementValue, liabilityPerOccurrence, effectiveFrom })
      success('Insurance requirements saved')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to save the insurance requirements.'))
    }
  }

  return (
    <SlideOver
      title="Insurance Requirements"
      subtitle={lease.homeownerFullName ?? undefined}
      onClose={onClose}
      isDirty={isDirty}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="set-insurance-requirements-form"
            disabled={mutation.isPending || !effectiveFrom || !changed}
          >
            {mutation.isPending ? 'Saving…' : 'Save Requirements'}
          </button>
        </>
      }
    >
      <form id="set-insurance-requirements-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          Clear an amount to stop checking it. The lease asks the trust to tell the homeowner of a raise at least 30 days
          before the policy renews; write that letter yourself.
        </p>
        <div className="form-grid">
          <OptionalAmountField
            id="insurance-replacement-value"
            label="Replacement Value ($)"
            value={replacementValue}
            onChange={setReplacementValue}
          />
          <OptionalAmountField
            id="insurance-liability"
            label="Liability per Occurrence ($)"
            value={liabilityPerOccurrence}
            onChange={setLiabilityPerOccurrence}
          />
          <div className="form-field">
            <label className="form-label" htmlFor="insurance-effective-from">Effective From</label>
            <DateInput
              id="insurance-effective-from"
              className="form-input"
              value={effectiveFrom}
              min={earliest ?? undefined}
              onChange={setEffectiveFrom}
              aria-describedby="insurance-effective-from-hint"
              required
            />
            <p id="insurance-effective-from-hint" className="form-hint">
              {earliest && `On or after ${formatCalendarDate(earliest)}. `}
              The current certificate is checked against the amounts in effect on its policy start date.
            </p>
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}

/** The lease start or the latest change to the requirements, whichever is later. */
function earliestEffectiveFrom(lease: GroundLeaseResponse): CalendarDate | null {
  const dates = [lease.leaseStartDate, ...(lease.insurance?.requirementsHistory ?? []).map(c => c.effectiveFrom)]
    .filter((d): d is CalendarDate => !!d)
  return dates.length ? dates.reduce((a, b) => (a > b ? a : b)) : null
}
