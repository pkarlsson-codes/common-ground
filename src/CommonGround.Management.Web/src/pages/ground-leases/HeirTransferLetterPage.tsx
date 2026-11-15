import { useEffect, useRef } from 'react'
import { useParams } from 'react-router-dom'
import { useHeirTransfer, useLeaseFeeNoticePolicy } from '../../hooks/groundLeases'
import { useHome } from '../../hooks/homes'
import { trustToday } from '../../lib/calendarDate'
import { heirLetterApplies, heirTransferLetter } from './heirTransferLetter'

/**
 * A completed heir transfer's letter: the trust's consent under Art 10.3.
 * Opens the print dialog once it has loaded. Outside the app's layout, so only the letter prints.
 */
export default function HeirTransferLetterPage() {
  const { heirTransferId } = useParams()
  const transfer = useHeirTransfer(heirTransferId ?? null)
  const home = useHome(transfer.data?.homeId ?? null)
  const policy = useLeaseFeeNoticePolicy()
  const printed = useRef(false)
  const ready = !!transfer.data && !home.isLoading && !policy.isLoading
  const decided = heirLetterApplies(transfer.data?.status)

  useEffect(() => {
    if (printed.current || !ready || !decided) return
    printed.current = true
    window.print()
  }, [ready, decided])

  const letter = ready && decided
    ? heirTransferLetter(transfer.data!, home.data?.label ?? null, trustToday(), policy.data?.letterSignOff ?? '')
    : null

  return (
    <div>
      <div className="letters-toolbar">
        <h1 className="page-header__title" style={{ fontSize: 20 }}>Heir Transfer Letter</h1>
        <button type="button" className="btn btn-primary" onClick={() => window.print()} disabled={!letter}>
          Print
        </button>
      </div>

      {transfer.isLoading && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {transfer.isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load the heir transfer</div>
          <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
        </div>
      )}
      {transfer.data && !decided && (
        <div className="empty-state">
          <div className="empty-state__title">The transfer has no letter: it isn't completed.</div>
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
