import { AsYouType, parsePhoneNumberFromString } from 'libphonenumber-js'

const DEFAULT_COUNTRY = 'US'

/**
 * Formats a phone number as the user types, e.g. "2025551" -> "(202) 555-1".
 * Deletions pass through unformatted so backspacing over punctuation is not undone.
 */
export function formatPhoneInput(value: string, previous = ''): string {
  if (value.length < previous.length) return value
  return new AsYouType(DEFAULT_COUNTRY).input(value)
}

/** True when the value parses to a valid phone number (local numbers default to US). */
export function isValidPhone(value: string): boolean {
  return parsePhoneNumberFromString(value, DEFAULT_COUNTRY)?.isValid() ?? false
}

/** Converts input to canonical E.164 (e.g. "+12025551234"), or undefined when invalid. */
export function toE164(value: string): string | undefined {
  const parsed = parsePhoneNumberFromString(value, DEFAULT_COUNTRY)
  return parsed?.isValid() ? parsed.number : undefined
}

/**
 * Formats a stored phone number for display: national format for US numbers
 * ("(202) 555-1234"), international format otherwise. Falls back to the raw value.
 */
export function formatPhoneDisplay(value?: string | null): string | undefined {
  if (!value) return undefined
  const parsed = parsePhoneNumberFromString(value, DEFAULT_COUNTRY)
  if (!parsed) return value
  return parsed.country === DEFAULT_COUNTRY ? parsed.formatNational() : parsed.formatInternational()
}
