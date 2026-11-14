interface Props {
  id: string
  label: string
  value: number | null
  onChange: (value: number | null) => void
  hintId?: string
}

/** A dollar amount the user may leave empty; empty is null, so the app doesn't check against it. */
export default function OptionalAmountField({ id, label, value, onChange, hintId }: Props) {
  return (
    <div className="form-field">
      <label className="form-label" htmlFor={id}>{label}</label>
      <input
        id={id}
        className="form-input"
        type="number"
        min="0"
        step="0.01"
        value={value ?? ''}
        onChange={e => {
          const parsed = parseFloat(e.target.value)
          onChange(Number.isNaN(parsed) ? null : parsed)
        }}
        aria-describedby={hintId}
      />
    </div>
  )
}
