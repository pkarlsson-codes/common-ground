import { useState } from 'react'
import type { ResaleResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import { usePayoffStatement, useRecordPayoffStatement } from '../../hooks/resales'
import { useToast } from '../../hooks/useToast'
import { addDays, trustToday, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { PAYOFF_MAX_DAYS_AHEAD, goodThroughValid, payoffStatementLetterPath, payoffWorking } from './payoffStatementLetter'

interface Props {
  resale: ResaleResponse
  onClose: () => void
}

/** What the seller owes the trust on a day, for the closing agent to pay from proceeds. Printing the letter records it. */
export default function PayoffStatementPanel({ resale, onClose }: Props) {
  const today = trustToday()
  const [goodThrough, setGoodThrough] = useState<CalendarDate | null>(today)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const valid = goodThroughValid(goodThrough, today)
  const statement = usePayoffStatement(resale.id!, goodThrough, valid)
  const data = valid ? statement.data : undefined
  const mutation = useRecordPayoffStatement()
  const { success } = useToast()

  const handlePrint = async () => {
    if (!valid) return
    setProblem(null)
    // Open the tab before the request finishes, or the browser takes it for a pop-up.
    window.open(payoffStatementLetterPath(resale.id!, goodThrough), '_blank')
    try {
      await mutation.mutateAsync({ resaleId: resale.id!, goodThrough })
      success('Payoff statement recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the payoff statement.'))
    }
  }

  return (
    <SlideOver
      title="Payoff Statement"
      subtitle={resale.homeLabel ?? undefined}
      onClose={onClose}
      footer={
        <>
          <button type="button" className="btn btn-ghost" onClick={onClose}>Close</button>
          <button type="button" className="btn btn-primary" onClick={handlePrint} disabled={mutation.isPending || !data}>
            {mutation.isPending ? 'Recording…' : 'Print Letter'}
          </button>
        </>
      }
    >
      <div className="form-grid">
        <div className="form-field">
          <label className="form-label" htmlFor="payoff-good-through">Good Through</label>
          <DateInput
            id="payoff-good-through"
            className="form-input"
            value={goodThrough}
            min={today}
            max={addDays(today, PAYOFF_MAX_DAYS_AHEAD)}
            onChange={setGoodThrough}
            required
          />
        </div>
      </div>

      <div className="detail-section" aria-live="polite">
        <div className="detail-section__title">Total Owed</div>
        {data ? (
          <>
            <div className="mono" style={{ fontSize: 20, marginBottom: 8 }}>
              {(data.totalOwed ?? 0) > 0 ? formatPrice(data.totalOwed ?? 0) : 'Nothing owed'}
            </div>
            {payoffWorking(data).map(line => (
              <div key={line} style={{ fontSize: 13, color: 'var(--text-secondary)' }}>{line}</div>
            ))}
          </>
        ) : (
          <p style={{ fontSize: 13, color: 'var(--text-dim)' }}>
            {statement.isError && valid ? "Couldn't work out the amount owed." : 'Working out the amount owed…'}
          </p>
        )}
      </div>

      <ApiProblemAlert problem={problem} />
    </SlideOver>
  )
}
