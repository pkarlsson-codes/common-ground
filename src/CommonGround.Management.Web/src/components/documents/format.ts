import { formatCalendarDate, trustToday, type CalendarDate } from '../../lib/calendarDate'

export const fmtSize = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

const toDate = (d: Date | string) => (d instanceof Date ? d : new Date(d))

export const fmtDate = (d: Date | string | null | undefined) =>
  d ? toDate(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' }) : '—'

export const fmtExpiry = (d: CalendarDate) => formatCalendarDate(d)

export const isExpired = (d: CalendarDate) => d < trustToday()

/** Expired, or expiring within 30 days. */
export const isExpiringSoon = (d: CalendarDate) => d <= trustToday(30)
