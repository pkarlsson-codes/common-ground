interface Props {
  value: number | null
  onChange: (value: number | null) => void
}

/**
 * The lease's debt cap (Model Lease Art 8.3), a whole percent of the purchase option price. Starts empty: the manual
 * leaves the figure to each trust, so the form doesn't pick one.
 */
export default function DebtCapField({ value, onChange }: Props) {
  return (
    <div className="form-field">
      <label className="form-label" htmlFor="lease-debt-cap">Debt Cap (% of purchase option price)</label>
      <input
        id="lease-debt-cap"
        className="form-input"
        type="number"
        min="1"
        max="200"
        step="1"
        value={value ?? ''}
        onChange={e => {
          const parsed = parseInt(e.target.value, 10)
          onChange(Number.isNaN(parsed) ? null : parsed)
        }}
        aria-describedby="lease-debt-cap-hint"
        required
      />
      <p id="lease-debt-cap-hint" className="form-hint">
        The most total mortgage debt may be when the trust permits a new loan (Art 8.3). Commonly 80–100%.
      </p>
    </div>
  )
}
