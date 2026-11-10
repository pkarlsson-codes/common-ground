import { formatPrice } from '../../lib/currency'

interface Props {
  landUseFee: number
  repairReserveFee: number
  onChange: (fees: { landUseFee: number; repairReserveFee: number }) => void
}

const toFee = (value: string) => Math.max(0, parseFloat(value) || 0)

/** The two monthly fees a lease charges (Model Lease Art 5.1), and their sum. */
export default function LeaseFeeFields({ landUseFee, repairReserveFee, onChange }: Props) {
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
          onChange={e => onChange({ landUseFee: toFee(e.target.value), repairReserveFee })}
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
          onChange={e => onChange({ landUseFee, repairReserveFee: toFee(e.target.value) })}
          required
        />
        <div className="form-help">Monthly Lease Fee: {formatPrice(landUseFee + repairReserveFee)}, due on the 1st</div>
      </div>
    </>
  )
}
