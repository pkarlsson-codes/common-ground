import type { PurchaseWorkflowResponse } from '../../api'
import { formatCurrency as fmtCurrency } from '../../lib/currency'
import { incomeBandFor, incomeBandLabel, PRINTED_HOUSEHOLD_SIZES } from '../../lib/incomeBand'

/**
 * The limit for the household size typed into the eligibility form, read from the selection's printed row, and
 * the income band the typed income falls in. Above 8 people the API works both out when the verification is recorded.
 */
export function EligibilityIncomeCheck({ detail, form }: { detail: PurchaseWorkflowResponse; form: { verifiedHouseholdSize: string; verifiedAnnualIncome: string } }) {
  const limits = detail.incomeLimit?.limits
  const levels = detail.incomeLimitSchedule?.levels
  const size = Number(form.verifiedHouseholdSize)
  if ((!limits && !levels) || form.verifiedHouseholdSize === '' || !Number.isInteger(size) || size < 1) return null

  const people = `${size} ${size === 1 ? 'person' : 'people'}`
  if (size > PRINTED_HOUSEHOLD_SIZES) {
    return (
      <div className="form-hint" role="status">
        {limits ? `Limit for ${people}: worked out when recorded` : `Income band for ${people}: worked out when recorded`}
      </div>
    )
  }

  const income = form.verifiedAnnualIncome === '' ? null : Number(form.verifiedAnnualIncome)
  const limit = limits?.[size - 1]
  const over = income != null && limit != null && income > limit
  const band = income != null && levels ? incomeBandFor(levels, income, size) : null
  const bandText = band ? incomeBandLabel(band) : null
  return (
    <div className="form-hint" role="status">
      {limit != null && <>Limit for {people}: {fmtCurrency(limit)}</>}
      {over && <> <span className="badge badge-overincomelimit">Over income limit</span></>}
      {bandText && <>{limit != null ? ' · ' : ''}Income band: {bandText}</>}
    </div>
  )
}
