/** A CSV cell: text, a number, or empty when null. */
export type CsvCell = string | number | null | undefined

const quote = (cell: CsvCell) => {
  if (cell == null) return ''
  const text = String(cell)
  return /[",\r\n]/.test(text) ? `"${text.replaceAll('"', '""')}"` : text
}

/** RFC 4180 CSV: a header row, then one line per row, CRLF line ends. */
export function toCsv(header: readonly string[], rows: readonly (readonly CsvCell[])[]): string {
  return [header, ...rows].map(row => row.map(quote).join(',')).join('\r\n') + '\r\n'
}

/** Has the browser save the text as a CSV file. */
export function downloadCsv(fileName: string, csv: string) {
  const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv;charset=utf-8' }))
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}
