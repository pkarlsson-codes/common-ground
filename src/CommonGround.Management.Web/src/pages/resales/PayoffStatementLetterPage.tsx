import { useEffect, useRef } from 'react'
import { useParams, useSearchParams } from 'react-router-dom'
import { useLeaseFeeNoticePolicy } from '../../hooks/groundLeases'
import { usePayoffStatement, useResale } from '../../hooks/resales'
import { trustToday } from '../../lib/calendarDate'
import { parsePayoffStatementLetterParams, payoffStatementLetter } from './payoffStatementLetter'

/**
 * A resale's payoff statement letter for the closing agent, worked out from the Good Through day in the address.
 * Opens the print dialog once it has loaded. Outside the app's layout, so only the letter prints.
 */
export default function PayoffStatementLetterPage() {
  const { resaleId } = useParams()
  const [params] = useSearchParams()
  const goodThrough = parsePayoffStatementLetterParams(params)
  const resale = useResale(resaleId ?? null)
  const statement = usePayoffStatement(resaleId ?? null, goodThrough, true)
  const policy = useLeaseFeeNoticePolicy()
  const printed = useRef(false)
  const ready = !!resale.data && !!statement.data && !policy.isLoading

  useEffect(() => {
    if (printed.current || !ready) return
    printed.current = true
    window.print()
  }, [ready])

  const letter = ready
    ? payoffStatementLetter(resale.data!, statement.data!, trustToday(), policy.data?.letterSignOff ?? '')
    : null
  const failed = resale.isError || statement.isError || !goodThrough

  return (
    <div>
      <div className="letters-toolbar">
        <h1 className="page-header__title" style={{ fontSize: 20 }}>Payoff Statement</h1>
        <button type="button" className="btn btn-primary" onClick={() => window.print()} disabled={!letter}>
          Print
        </button>
      </div>

      {!letter && !failed && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {failed && (
        <div className="error-state">
          <div className="error-state__title">Could not work out the statement</div>
          <div className="error-state__sub">Close this tab and print the letter again from the Payoff Statement panel.</div>
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
            {letter.paragraphs.map(text => <p key={text}>{text}</p>)}
            <ul>
              {letter.working.map(line => <li key={line}>{line}</li>)}
            </ul>
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
