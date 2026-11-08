const dollars = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 })
const cents = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 })

/** Whole dollars, e.g. "$205,000". */
export const formatCurrency = (n: number) => dollars.format(n)

/** Dollars and cents, e.g. "$205,000.50", for prices worked out by a formula. */
export const formatPrice = (n: number) => cents.format(n)
