import { localToday } from '../../lib/dates'

/** A date as YYYY-MM-DD, or undefined. */
export const fmtD = (d: Date | null | undefined) => (d ? d.toISOString().slice(0, 10) : undefined)

/** Today as YYYY-MM-DD, for date input defaults. */
export const today = () => localToday()
