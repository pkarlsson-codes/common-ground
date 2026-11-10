import { formatCalendarDate, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'

/** "$120.00 due", "$40.00 credit" or "Paid up". */
export const formatBalance = (balance: number) =>
  balance > 0 ? `${formatPrice(balance)} due` : balance < 0 ? `${formatPrice(-balance)} credit` : 'Paid up'

/** The month a charge fell due in, e.g. "February 2026". */
export const formatDueMonth = (due: CalendarDate) => formatCalendarDate(due, { year: 'numeric', month: 'long' })

export const formatDaysOverdue = (days: number) => (days === 1 ? '1 day' : `${days} days`)
