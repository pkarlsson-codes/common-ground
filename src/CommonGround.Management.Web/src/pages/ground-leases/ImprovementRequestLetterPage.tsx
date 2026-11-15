import { useEffect, useRef } from 'react'
import { useParams } from 'react-router-dom'
import { useImprovementRequest, useLeaseFeeNoticePolicy } from '../../hooks/groundLeases'
import { useHome } from '../../hooks/homes'
import { trustToday } from '../../lib/calendarDate'
import { improvementLetterApplies, improvementRequestLetter } from './improvementRequestLetter'

/**
 * A decided improvement request's letter: the trust's approval with its pre-approved credit, or its refusal.
 * Opens the print dialog once it has loaded. Outside the app's layout, so only the letter prints.
 */
export default function ImprovementRequestLetterPage() {
  const { improvementRequestId } = useParams()
  const request = useImprovementRequest(improvementRequestId ?? null)
  const home = useHome(request.data?.homeId ?? null)
  const policy = useLeaseFeeNoticePolicy()
  const printed = useRef(false)
  const ready = !!request.data && !home.isLoading && !policy.isLoading
  const decided = improvementLetterApplies(request.data?.status)

  useEffect(() => {
    if (printed.current || !ready || !decided) return
    printed.current = true
    window.print()
  }, [ready, decided])

  const letter = ready && decided
    ? improvementRequestLetter(request.data!, home.data?.label ?? null, trustToday(), policy.data?.letterSignOff ?? '')
    : null

  return (
    <div>
      <div className="letters-toolbar">
        <h1 className="page-header__title" style={{ fontSize: 20 }}>Improvement Request Letter</h1>
        <button type="button" className="btn btn-primary" onClick={() => window.print()} disabled={!letter}>
          Print
        </button>
      </div>

      {request.isLoading && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {request.isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load the request</div>
          <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
        </div>
      )}
      {request.data && !decided && (
        <div className="empty-state">
          <div className="empty-state__title">The request has no letter: it isn't approved or denied.</div>
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
