import { formatCurrency } from '../../lib/currency'

type PriceCheckInput = {
  price: number
  prequalifiedAmount?: number | null
}

/** Warnings for a purchase price. They never block saving. Empty when no price is entered. */
export function purchasePriceWarnings({ price, prequalifiedAmount }: PriceCheckInput): string[] {
  if (!price) return []
  const warnings: string[] = []

  if (prequalifiedAmount != null && price > prequalifiedAmount) {
    warnings.push(`The price is above the household's prequalified amount (${formatCurrency(price)} against ${formatCurrency(prequalifiedAmount)}).`)
  }

  return warnings
}
