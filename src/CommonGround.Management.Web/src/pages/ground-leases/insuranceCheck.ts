import type { InsuranceRequirementsChangeDto } from '../../api'
import type { CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'

export interface InsuranceRequirementsInEffect {
  replacementValue: number | null
  liabilityPerOccurrence: number | null
}

/**
 * The requirements in effect on a day: the latest change effective on or before it, or the first one for a day
 * before the lease start. The history comes latest effective first, as the lease view gives it.
 */
export function requirementsOn(
  history: readonly InsuranceRequirementsChangeDto[],
  date: CalendarDate,
): InsuranceRequirementsInEffect {
  const change = history.find(c => !!c.effectiveFrom && c.effectiveFrom <= date) ?? history[history.length - 1]
  return {
    replacementValue: change?.replacementValue ?? null,
    liabilityPerOccurrence: change?.liabilityPerOccurrence ?? null,
  }
}

export interface CertificateCover {
  dwellingCoverage: number
  liabilityPerOccurrence: number
  trustNamedAdditionalInsured: boolean
}

export interface CertificateCheck {
  /** Each way the certificate falls short, as a sentence. */
  shortfalls: string[]
  /** Each requirement the lease doesn't set, so the app doesn't check it. */
  notChecked: string[]
}

/** Compares a certificate with the requirements, the way the lease does. */
export function checkCertificate(requirements: InsuranceRequirementsInEffect, cover: CertificateCover): CertificateCheck {
  const shortfalls: string[] = []
  const notChecked: string[] = []
  const { replacementValue, liabilityPerOccurrence } = requirements

  if (replacementValue == null) notChecked.push('Replacement value not checked: none set on the lease')
  else if (cover.dwellingCoverage < replacementValue)
    shortfalls.push(`Dwelling coverage ${formatPrice(cover.dwellingCoverage)} is below the replacement value ${formatPrice(replacementValue)}`)

  if (liabilityPerOccurrence == null) notChecked.push('Liability not checked: none set on the lease')
  else if (cover.liabilityPerOccurrence < liabilityPerOccurrence)
    shortfalls.push(`Liability ${formatPrice(cover.liabilityPerOccurrence)} is below the ${formatPrice(liabilityPerOccurrence)} required`)

  if (!cover.trustNamedAdditionalInsured) shortfalls.push('The trust is not named as additional insured')

  return { shortfalls, notChecked }
}

/** The sentence for each shortfall code the lease view gives for its current certificate. */
export const SHORTFALL_LABELS: Record<string, string> = {
  DwellingBelowReplacementValue: 'Dwelling coverage below the replacement value',
  LiabilityBelowRequired: 'Liability below the required amount',
  TrustNotAdditionalInsured: 'Trust not named as additional insured',
}
