import type { InputHTMLAttributes } from 'react'
import { calendarDateOrNull, type CalendarDate } from '../lib/calendarDate'

type Props = Omit<InputHTMLAttributes<HTMLInputElement>, 'type' | 'value' | 'onChange' | 'required'> & (
  | { required: true; value: CalendarDate | null; onChange: (value: CalendarDate) => void }
  | { required?: false; value: CalendarDate | null; onChange: (value: CalendarDate | null) => void }
)

/**
 * A date input that reads and writes CalendarDate. Optional inputs report null when cleared. Required
 * inputs only report complete dates; the browser's required check stops a cleared one being submitted.
 */
export function DateInput({ value, onChange, required, ...props }: Props) {
  return (
    <input
      {...props}
      type="date"
      required={required}
      value={value ?? ''}
      onChange={e => {
        const date = calendarDateOrNull(e.target.value)
        if (date || !required) (onChange as (value: CalendarDate | null) => void)(date)
      }}
    />
  )
}
