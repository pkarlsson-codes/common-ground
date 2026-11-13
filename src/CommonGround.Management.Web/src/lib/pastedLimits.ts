/** One line of a pasted income limit table. */
export type PastedLine = {
  /** A number written with a % sign, e.g. "(50%)" in HUD's "Very Low (50%) Income Limits". */
  percentOfAmi: number | null
  /** Every other number on the line, in order. */
  amounts: number[]
}

/** "$50,050" → 50050. NaN when the text isn't a number. */
export const parseAmount = (text: string) => {
  const cleaned = text.replace(/[$,\s]/g, '')
  return cleaned === '' ? NaN : Number(cleaned)
}

const isHouseholdSizeHeader = (amounts: number[]) => amounts.length > 1 && amounts.every((n, i) => n === i + 1)

/**
 * Reads the numbers in text copied from a spreadsheet, a web page or a PDF. Cells are split on tabs and spaces;
 * words such as "Very Low" and "Income Limits" are dropped. Lines with no numbers are left out, and so is a
 * "1 2 3 … 8" household size header.
 */
export const parsePastedLimits = (text: string): PastedLine[] =>
  text
    .split(/\r?\n/)
    .map(line => {
      let percentOfAmi: number | null = null
      const amounts: number[] = []
      for (const cell of line.split(/[\t ]+/)) {
        const token = cell.replace(/^[(]+|[)*]+$/g, '')
        if (token.endsWith('%')) {
          const percent = parseAmount(token.slice(0, -1))
          if (percentOfAmi === null && Number.isFinite(percent)) percentOfAmi = percent
          continue
        }
        const amount = parseAmount(token)
        if (Number.isFinite(amount)) amounts.push(amount)
      }
      return { percentOfAmi, amounts }
    })
    .filter(l => (l.percentOfAmi !== null || l.amounts.length > 0) && !isHouseholdSizeHeader(l.amounts))
