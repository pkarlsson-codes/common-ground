import type { HomeownerApplicationResponse } from '../../api'

const fmtCurrency = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(n)

/**
 * What changed since staff last checked the household still qualifies, e.g. "Income $52,000 → $61,000". Empty when
 * the application has no qualification to confirm.
 */
export function qualificationChanges(
  application: Pick<
    HomeownerApplicationResponse,
    'annualIncome' | 'householdSize' | 'qualifiedAnnualIncome' | 'qualifiedHouseholdSize' | 'qualificationToConfirmSince'
  >,
): string[] {
  if (!application.qualificationToConfirmSince) return []
  const changes: string[] = []
  if (application.qualifiedAnnualIncome != null && application.qualifiedAnnualIncome !== application.annualIncome) {
    changes.push(`Income ${fmtCurrency(application.qualifiedAnnualIncome)} → ${fmtCurrency(application.annualIncome ?? 0)}`)
  }
  if (application.qualifiedHouseholdSize != null && application.qualifiedHouseholdSize !== application.householdSize) {
    changes.push(`Household size ${application.qualifiedHouseholdSize} → ${application.householdSize}`)
  }
  return changes
}
