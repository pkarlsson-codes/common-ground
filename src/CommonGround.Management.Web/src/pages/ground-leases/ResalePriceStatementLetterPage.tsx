import { useEffect, useRef } from 'react'
import { useParams, useSearchParams } from 'react-router-dom'
import { useGroundLease, useLeaseFeeNoticePolicy, useResalePriceStatementPreview } from '../../hooks/groundLeases'
import { useHome } from '../../hooks/homes'
import { parseResalePriceStatementLetterParams, resalePriceStatementLetter } from './resalePriceStatementLetter'

/**
 * One lease's resale price statement letter, worked out from the statement date and figures in the address. Opens
 * the print dialog once it has loaded. Outside the app's layout, so only the letter prints.
 */
export default function ResalePriceStatementLetterPage() {
  const { groundLeaseId } = useParams()
  const [params] = useSearchParams()
  const { statementDate, figures } = parseResalePriceStatementLetterParams(params)
  const lease = useGroundLease(groundLeaseId ?? null)
  const home = useHome(lease.data?.homeId ?? null)
  const policy = useLeaseFeeNoticePolicy()
  const preview = useResalePriceStatementPreview(groundLeaseId ?? null, statementDate, figures, true)
  const printed = useRef(false)
  const ready = !!lease.data && !!preview.data && !home.isLoading && !policy.isLoading

  useEffect(() => {
    if (printed.current || !ready) return
    printed.current = true
    window.print()
  }, [ready])

  const letter = ready
    ? resalePriceStatementLetter(lease.data!, home.data?.label ?? null, preview.data!, policy.data?.letterSignOff ?? '')
    : null
  const failed = lease.isError || preview.isError || !statementDate

  return (
    <div>
      <div className="letters-toolbar">
        <h1 className="page-header__title" style={{ fontSize: 20 }}>Resale Price Statement</h1>
        <button type="button" className="btn btn-primary" onClick={() => window.print()} disabled={!letter}>
          Print
        </button>
      </div>

      {!letter && !failed && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {failed && (
        <div className="error-state">
          <div className="error-state__title">Could not work out the statement</div>
          <div className="error-state__sub">Close this tab and print the letter again from the statement panel.</div>
        </div>
      )}

      {letter && (
        <div className="letters">
          <article className="notice-letter" aria-label={letter.heading}>
            <p>{letter.date}</p>
            <div className="notice-letter__lines">
              {letter.recipient.map(line => <div key={line}>{line}</div>)}
            </div>
            <div className="notice-letter__heading">{letter.heading}</div>
            <p>{letter.greeting}</p>
            <p>{letter.priceSentence}</p>
            <ul>
              {letter.working.map(line => <li key={line}>{line}</li>)}
            </ul>
            <p>{letter.closing}</p>
            <p>Sincerely,</p>
            <div className="notice-letter__lines">
              {letter.signOff.map(line => <div key={line}>{line}</div>)}
            </div>
          </article>
        </div>
      )}
    </div>
  )
}
