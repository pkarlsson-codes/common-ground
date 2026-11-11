import { formatPrice } from '../../lib/currency'
import { lateInterestText } from './lateInterest'

export interface LeaseFeeTermsForm {
  landUseFee: number
  repairReserveFee: number
  lateInterestRate: number
  interestWaiverDays: number
}

interface Props {
  fees: LeaseFeeTermsForm
  onChange: (fees: Partial<LeaseFeeTermsForm>) => void
}

const toAmount = (value: string) => Math.max(0, parseFloat(value) || 0)
const toDays = (value: string) => Math.max(0, parseInt(value, 10) || 0)

/** The two monthly fees a lease charges (Model Lease Art 5.1), their sum, and the interest on a late fee (Art 5.7). */
export default function LeaseFeeFields({ fees, onChange }: Props) {
  const { landUseFee, repairReserveFee, lateInterestRate, interestWaiverDays } = fees
  return (
    <>
      <div className="form-field">
        <label className="form-label" htmlFor="lease-land-use-fee">Land Use Fee ($/month)</label>
        <input
          id="lease-land-use-fee"
          className="form-input"
          type="number"
          min="0"
          step="0.01"
          value={landUseFee}
          onChange={e => onChange({ landUseFee: toAmount(e.target.value) })}
          required
        />
      </div>
      <div className="form-field">
        <label className="form-label" htmlFor="lease-repair-reserve-fee">Repair Reserve Fee ($/month)</label>
        <input
          id="lease-repair-reserve-fee"
          className="form-input"
          type="number"
          min="0"
          step="0.01"
          value={repairReserveFee}
          onChange={e => onChange({ repairReserveFee: toAmount(e.target.value) })}
          required
        />
        <div className="form-help">Monthly Lease Fee: {formatPrice(landUseFee + repairReserveFee)}, due on the 1st</div>
      </div>
      <div className="form-field">
        <label className="form-label" htmlFor="lease-late-interest-rate">Late Interest Rate (% a year)</label>
        <input
          id="lease-late-interest-rate"
          className="form-input"
          type="number"
          min="0"
          max="100"
          step="0.01"
          value={lateInterestRate}
          onChange={e => onChange({ lateInterestRate: Math.min(100, toAmount(e.target.value)) })}
          required
        />
        <div className="form-help">The rate written into the lease. 0 charges no interest.</div>
      </div>
      <div className="form-field">
        <label className="form-label" htmlFor="lease-interest-waiver-days">Interest Waived If Paid Within (days)</label>
        <input
          id="lease-interest-waiver-days"
          className="form-input"
          type="number"
          min="0"
          step="1"
          value={interestWaiverDays}
          onChange={e => onChange({ interestWaiverDays: toDays(e.target.value) })}
          disabled={lateInterestRate === 0}
          required
        />
        <div className="form-help">Late Interest: {lateInterestText(lateInterestRate, interestWaiverDays)}</div>
      </div>
    </>
  )
}
