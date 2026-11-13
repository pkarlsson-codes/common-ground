import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import {
  MORTGAGE_PURPOSE_LABELS,
  MORTGAGE_REQUEST_NOTICE_BUSINESS_DAYS,
  useReceiveMortgageRequest,
  type ReceiveMortgageRequestForm,
} from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import LoanAppraisalFields from './LoanAppraisalFields'
import { appraisalValid, businessDaysBetween } from './mortgageRequestLetter'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
  /** Called with the new request's ID once it is recorded. */
  onRecorded: (mortgageRequestId: string) => void
}

const emptyForm = (): ReceiveMortgageRequestForm => ({
  receivedOn: trustToday(),
  expectedClosingOn: null,
  lender: '',
  purpose: 'Refinance',
  principal: 0,
  totalDebtAfterClosing: 0,
  interestRatePercent: 0,
  rateType: 'Fixed',
  termYears: 0,
  closingCosts: 0,
  appraisal: { appraisedValue: 0, appraisalDate: null, currentIndexValue: 0 },
  notes: '',
})

/** Records a homeowner's request for a new loan on the home (Art 8.3). */
export default function RecordMortgageRequestPanel({ lease, onClose, onRecorded }: Props) {
  const [form, setForm] = useState(emptyForm)
  // Until the user types a total, it follows the principal and purpose.
  const [totalEdited, setTotalEdited] = useState(false)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useReceiveMortgageRequest()
  const { success } = useToast()
  const indexBased = lease.formulaKind === 'IndexBased'

  const otherDebt = (lease.mortgages?.permitted ?? [])
    .filter(m => !m.paidOffOn)
    .reduce((sum, m) => sum + (m.originalPrincipal ?? 0), 0)
  const suggestedTotal = (principal: number, purpose: string) =>
    principal > 0 ? (purpose === 'Refinance' ? principal : principal + otherDebt) : 0
  const totalDebt = totalEdited ? form.totalDebtAfterClosing : suggestedTotal(form.principal, form.purpose)

  const set = (patch: Partial<ReceiveMortgageRequestForm>) => setForm(f => ({ ...f, ...patch }))

  const businessDays = form.expectedClosingOn && form.expectedClosingOn >= form.receivedOn
    ? businessDaysBetween(form.receivedOn, form.expectedClosingOn)
    : null
  const shortNotice = businessDays !== null && businessDays < MORTGAGE_REQUEST_NOTICE_BUSINESS_DAYS

  const valid = !!form.expectedClosingOn
    && form.expectedClosingOn >= form.receivedOn
    && form.lender.trim() !== ''
    && form.principal > 0
    && totalDebt >= form.principal
    && form.interestRatePercent > 0
    && form.termYears > 0
    && appraisalValid(form.appraisal, indexBased, false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!valid) return
    setProblem(null)
    try {
      const result = await mutation.mutateAsync({
        groundLeaseId: lease.id!,
        form: { ...form, totalDebtAfterClosing: totalDebt },
        indexBased,
      })
      success('Request recorded')
      onRecorded(result.mortgageRequestId!)
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the request.'))
    }
  }

  return (
    <SlideOver
      title="Record Mortgage Request"
      subtitle={lease.homeownerFullName ?? 'Ground lease'}
      onClose={onClose}
      isDirty={form.lender !== '' || form.principal !== 0 || form.expectedClosingOn !== null}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="record-mortgage-request-form"
            disabled={mutation.isPending || !valid}
          >
            {mutation.isPending ? 'Recording…' : 'Record Request'}
          </button>
        </>
      }
    >
      <form id="record-mortgage-request-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          The lease asks the homeowner to send: the lender, the reason for the loan, the principal and total debt,
          closing costs, the interest rate, the repayment schedule, and the loan's appraisal.
        </p>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-request-received">Received On</label>
            <DateInput
              id="mortgage-request-received"
              className="form-input"
              value={form.receivedOn}
              min={lease.leaseStartDate}
              max={trustToday()}
              onChange={v => v && set({ receivedOn: v })}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-request-closing">Expected Closing</label>
            <DateInput
              id="mortgage-request-closing"
              className="form-input"
              value={form.expectedClosingOn}
              min={form.receivedOn}
              onChange={v => set({ expectedClosingOn: v })}
              required
            />
          </div>
          {businessDays !== null && (
            <p className="form-field full" style={{ fontSize: 13, color: shortNotice ? 'var(--warning)' : 'var(--text-secondary)' }}>
              {businessDays} business {businessDays === 1 ? 'day' : 'days'} from receipt to closing.
              {shortNotice && ` The lease asks for at least ${MORTGAGE_REQUEST_NOTICE_BUSINESS_DAYS}.`}
            </p>
          )}

          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-request-lender">Lender</label>
            <input
              id="mortgage-request-lender"
              className="form-input"
              value={form.lender}
              onChange={e => set({ lender: e.target.value })}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-request-purpose">Purpose</label>
            <select
              id="mortgage-request-purpose"
              className="form-select"
              value={form.purpose}
              onChange={e => set({ purpose: e.target.value })}
            >
              {Object.entries(MORTGAGE_PURPOSE_LABELS).map(([value, label]) => (
                <option key={value} value={value}>{label}</option>
              ))}
            </select>
          </div>

          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-request-principal">Principal ($)</label>
            <input
              id="mortgage-request-principal"
              className="form-input"
              type="number"
              min="0.01"
              step="0.01"
              value={form.principal || ''}
              onChange={e => set({ principal: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-request-total-debt">Total Mortgage Debt After Closing ($)</label>
            <input
              id="mortgage-request-total-debt"
              className="form-input"
              type="number"
              min={form.principal || 0.01}
              step="0.01"
              value={totalDebt || ''}
              onChange={e => {
                setTotalEdited(true)
                set({ totalDebtAfterClosing: parseFloat(e.target.value) || 0 })
              }}
              required
              aria-describedby="mortgage-request-total-debt-hint"
            />
            <p id="mortgage-request-total-debt-hint" className="form-hint">
              All mortgage debt on the home once this loan closes, including loans it doesn't pay off.
              {!totalEdited && form.principal > 0 && (form.purpose === 'Refinance'
                ? ' Filled in with the principal, as a refinance usually pays off the current loans.'
                : otherDebt > 0
                  ? ` Filled in with the principal plus ${formatPrice(otherDebt)} of current permitted mortgages.`
                  : '')}
            </p>
          </div>

          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-request-rate">Interest Rate (% a year)</label>
            <input
              id="mortgage-request-rate"
              className="form-input"
              type="number"
              min="0.001"
              step="0.001"
              value={form.interestRatePercent || ''}
              onChange={e => set({ interestRatePercent: parseFloat(e.target.value) || 0 })}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-request-rate-type">Rate Type</label>
            <select
              id="mortgage-request-rate-type"
              className="form-select"
              value={form.rateType}
              onChange={e => set({ rateType: e.target.value })}
            >
              <option value="Fixed">Fixed</option>
              <option value="Adjustable">Adjustable</option>
            </select>
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-request-term">Term (years)</label>
            <input
              id="mortgage-request-term"
              className="form-input"
              type="number"
              min="1"
              max="50"
              step="1"
              value={form.termYears || ''}
              onChange={e => set({ termYears: parseInt(e.target.value, 10) || 0 })}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-request-closing-costs">Closing Costs ($)</label>
            <input
              id="mortgage-request-closing-costs"
              className="form-input"
              type="number"
              min="0"
              step="0.01"
              value={form.closingCosts || ''}
              onChange={e => set({ closingCosts: parseFloat(e.target.value) || 0 })}
              aria-describedby="mortgage-request-closing-costs-hint"
            />
            <p id="mortgage-request-closing-costs-hint" className="form-hint">Optional.</p>
          </div>

          <p className="form-field full" style={{ fontSize: 13, color: 'var(--text-secondary)', margin: 0 }}>
            The loan's appraisal. Optional now; the debt check and approval need it.
          </p>
          <LoanAppraisalFields
            appraisal={form.appraisal}
            indexBased={indexBased}
            required={false}
            onChange={appraisal => set({ appraisal })}
          />
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
