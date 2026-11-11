import { useEffect, useRef } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useLeaseFeeNoticesDue } from '../../hooks/groundLeases'
import { leaseFeeLetter } from '../ground-leases/leaseFeeLetter'

/**
 * Every late notice due, one letter per printed page, or one lease's with ?lease=. Opens the print dialog once the
 * letters have loaded. Outside the app's layout, so only the letters print.
 */
export default function LeaseFeeNoticesPrintPage() {
  const [params] = useSearchParams()
  const leaseId = params.get('lease') ?? undefined
  const { data, isLoading, isError } = useLeaseFeeNoticesDue(leaseId)
  const printed = useRef(false)
  const notices = data?.notices ?? []
  const today = data?.today

  useEffect(() => {
    if (printed.current || notices.length === 0) return
    printed.current = true
    window.print()
  }, [notices.length])

  return (
    <div>
      <div className="letters-toolbar">
        <h1 className="page-header__title" style={{ fontSize: 20 }}>Lease Fee Notices</h1>
        {data && <span className="page-header__count">{notices.length === 1 ? '1 letter' : `${notices.length} letters`}</span>}
        <button type="button" className="btn btn-primary" onClick={() => window.print()} disabled={notices.length === 0}>
          Print
        </button>
      </div>

      {isLoading && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
      {isError && (
        <div className="error-state">
          <div className="error-state__title">Could not load the notices</div>
          <div className="error-state__sub">Try again in a moment. If it keeps failing, contact support.</div>
        </div>
      )}
      {data && notices.length === 0 && (
        <div className="empty-state">
          <div className="empty-state__title">No notices are due.</div>
        </div>
      )}

      <div className="letters">
        {today && notices.map(n => {
          const letter = leaseFeeLetter(n, n.noticeDue ?? 'First', today, data?.letterSignOff ?? '')
          return (
            <article key={n.groundLeaseId} className="notice-letter" aria-label={`Notice to ${n.homeownerFullName ?? 'homeowner'}`}>
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
          )
        })}
      </div>
    </div>
  )
}
