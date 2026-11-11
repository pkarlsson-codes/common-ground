/** How a lease's late-fee interest term reads (Model Lease Art 5.7): "10% a year, waived if paid within 30 days", or "None". */
export function lateInterestText(rate: number | null | undefined, waiverDays: number | null | undefined): string {
  if (!rate) return 'None'
  const days = waiverDays ?? 0
  const pct = `${rate.toLocaleString('en-US', { maximumFractionDigits: 3 })}% a year`
  return days > 0 ? `${pct}, waived if paid within ${days} day${days === 1 ? '' : 's'}` : pct
}
