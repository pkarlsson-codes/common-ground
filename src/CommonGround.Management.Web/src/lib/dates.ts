/**
 * The user's local calendar date as YYYY-MM-DD, offset from today, for date input defaults and
 * comparisons. Not toISOString(), which gives the UTC date and runs a day ahead in the evening.
 */
export const localToday = (offsetDays = 0) => {
  const d = new Date()
  d.setDate(d.getDate() + offsetDays)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}
