import { useEffect, useRef, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useResalePriceStatementPreviews, useResalePriceStatementsDue } from '../../hooks/groundLeases'
import { ResalePriceStatementLetterArticle } from '../ground-leases/ResalePriceStatementLetterPage'
import { resalePriceStatementLetter } from '../ground-leases/resalePriceStatementLetter'
import { loadPrintedRound } from './resalePriceRound'

/**
 * A round's resale price statement letters, one per printed page, from the figures the Prepare Statements panel saved
 * for this tab. Opens the print dialog once they have loaded. Outside the app's layout, so only the letters print.
 */
export default function ResalePriceStatementsPrintPage() {
  const [params] = useSearchParams()
  const [round] = useState(() => loadPrintedRound(params.get('round')))
  const due = useResalePriceStatementsDue(true, '')
  const preview = useResalePriceStatementPreviews(round?.statementDate ?? null, round?.leases ?? [], !!round)
  const printed = useRef(false)

  const leases = new Map((due.data?.leases ?? []).map(l => [l.groundLeaseId!, l]))
  const letters = (preview.data?.leases ?? []).flatMap(r => {
    const lease = leases.get(r.groundLeaseId!)
    if (!lease || !r.price) return []
    return [{ id: r.groundLeaseId!, letter: resalePriceStatementLetter(lease, lease.homeLabel ?? null, r.price, due.data?.letterSignOff ?? '') }]
  })
  const refused = (preview.data?.leases ?? []).filter(r => !r.price)
  const ready = !!due.data && !!preview.data && letters.length > 0
  const failed = !round || due.isError || preview.isError || (!!due.data && !!preview.data && letters.length === 0)

  useEffect(() => {
    if (printed.current || !ready) return
    printed.current = true
    window.print()
  }, [ready])

  return (
    <div>
      <div className="letters-toolbar">
        <h1 className="page-header__title" style={{ fontSize: 20 }}>
          Resale Price Statements{ready ? ` (${letters.length})` : ''}
        </h1>
        <button type="button" className="btn btn-primary" onClick={() => window.print()} disabled={!ready}>
          Print
        </button>
      </div>

      {!ready && !failed && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {failed && (
        <div className="error-state">
          <div className="error-state__title">Could not work out the statements</div>
          <div className="error-state__sub">Close this tab and print the statements again from the Prepare Statements panel.</div>
        </div>
      )}
      {ready && refused.length > 0 && (
        <p className="form-error letters-toolbar" role="alert">
          {refused.length === 1 ? '1 statement' : `${refused.length} statements`} could not be worked out and are not
          printed. Close this tab and check the figures in the Prepare Statements panel.
        </p>
      )}

      {ready && (
        <div className="letters">
          {letters.map(({ id, letter }) => (
            <ResalePriceStatementLetterArticle key={id} letter={letter} label={`${letter.heading}: ${letter.recipient[0]}`} />
          ))}
        </div>
      )}
    </div>
  )
}
