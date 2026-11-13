import { Fragment, useEffect, useRef } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useLeaseFeeNoticesDue } from '../../hooks/groundLeases'
import { leaseFeeLetter, type LeaseFeeLetter } from '../ground-leases/leaseFeeLetter'

/** Who a lender copy goes to. */
interface LetterCopy {
  lender: string
  noticeAddress?: string | null
}

/** One printed letter; a lender copy starts with "Copy to {lender}" and the notice address, or a red warning. */
function NoticeLetter({ letter, copy, label }: { letter: LeaseFeeLetter; copy?: LetterCopy; label: string }) {
  return (
    <article className="notice-letter" aria-label={label}>
      {copy && (
        <div className="notice-letter__copy">
          <div className="notice-letter__heading">Copy to {copy.lender}</div>
          {copy.noticeAddress
            ? <div className="notice-letter__lines">{copy.noticeAddress.split('\n').map(line => <div key={line}>{line}</div>)}</div>
            : <p className="notice-letter__missing">Notice address not on file</p>}
        </div>
      )}
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
}

/**
 * Every late notice due, one letter per printed page, or one lease's with ?lease=. A notice of default is followed by
 * one copy per current permitted mortgagee (Exhibit: Permitted Mortgages, B.6). Opens the print dialog once the
 * letters have loaded. Outside the app's layout, so only the letters print.
 */
export default function LeaseFeeNoticesPrintPage() {
  const [params] = useSearchParams()
  const leaseId = params.get('lease') ?? undefined
  const { data, isLoading, isError } = useLeaseFeeNoticesDue(leaseId)
  const printed = useRef(false)
  const notices = data?.notices ?? []
  const today = data?.today
  const lenderCopies = notices.flatMap(n => (n.noticeDue === 'Default' ? n.mortgagees ?? [] : []))
  const copies = lenderCopies.length
  const noAddress = lenderCopies.filter(m => !m.noticeAddress).length

  useEffect(() => {
    if (printed.current || notices.length === 0) return
    printed.current = true
    window.print()
  }, [notices.length])

  return (
    <div>
      <div className="letters-toolbar">
        <h1 className="page-header__title" style={{ fontSize: 20 }}>Lease Fee Notices</h1>
        {data && (
          <span className="page-header__count">
            {notices.length === 1 ? '1 letter' : `${notices.length} letters`}
            {copies > 0 && (copies === 1 ? ', 1 lender copy' : `, ${copies} lender copies`)}
          </span>
        )}
        {noAddress > 0 && (
          <span style={{ color: 'var(--red)' }}>
            {noAddress === 1 ? '1 lender copy has' : `${noAddress} lender copies have`} no notice address on file
          </span>
        )}
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
          const letter = leaseFeeLetter(n, n.noticeDue ?? 'First', today, data?.letterSignOff ?? '', {
            cureDays: data?.cureDays ?? 30,
            extensionDays: data?.cureExtensionDays ?? 30,
          })
          const name = n.homeownerFullName ?? 'homeowner'
          const copiesOfThis = n.noticeDue === 'Default' ? n.mortgagees ?? [] : []
          return (
            <Fragment key={n.groundLeaseId}>
              <NoticeLetter letter={letter} label={`Notice to ${name}`} />
              {copiesOfThis.map((m, i) => (
                <NoticeLetter
                  key={`${m.lender}-${i}`}
                  letter={letter}
                  copy={{ lender: m.lender ?? 'Lender', noticeAddress: m.noticeAddress }}
                  label={`Copy to ${m.lender ?? 'lender'} of notice to ${name}`}
                />
              ))}
            </Fragment>
          )
        })}
      </div>
    </div>
  )
}
