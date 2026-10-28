// Expiry is a calendar date stored as midnight UTC, so it is read and compared in UTC; other timestamps are local.

export const fmtSize = (bytes: number): string => {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

const toDate = (d: Date | string) => (d instanceof Date ? d : new Date(d))

export const fmtDate = (d: Date | string | null | undefined) =>
  d ? toDate(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' }) : '—'

export const fmtExpiry = (d: Date | string) =>
  toDate(d).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric', timeZone: 'UTC' })

/** The expiry as YYYY-MM-DD, for date inputs and comparisons. */
export const expiryIso = (d: Date | string) => toDate(d).toISOString().slice(0, 10)

const localIso = (offsetDays = 0) => {
  const d = new Date()
  d.setDate(d.getDate() + offsetDays)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

export const isExpired = (d: Date | string) => expiryIso(d) < localIso()

/** Expired, or expiring within 30 days. */
export const isExpiringSoon = (d: Date | string) => expiryIso(d) <= localIso(30)
