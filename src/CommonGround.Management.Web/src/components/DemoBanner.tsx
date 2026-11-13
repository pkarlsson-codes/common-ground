import { useDemo } from '../hooks/demo'
import { formatCalendarDate, longDate } from '../lib/calendarDate'

/** On a demo server, says the trust holds sample data and when it is deleted, with a feedback link. */
export default function DemoBanner() {
  const { data: demo } = useDemo()
  if (!demo?.deletesOn || !demo.feedbackUrl) return null

  // A web form opens in a new tab so the visitor keeps their place in the app; mailto opens their mail program.
  const external = !demo.feedbackUrl.startsWith('mailto:')

  return (
    <div className="demo-banner" role="note" aria-label="Demo">
      <span>
        Sample data for trying CommonGround. Don't enter real personal information.
        This trust is deleted on {formatCalendarDate(demo.deletesOn, longDate)}.
      </span>
      <a
        className="demo-banner__feedback"
        href={demo.feedbackUrl}
        {...(external ? { target: '_blank', rel: 'noreferrer' } : {})}
      >
        Send feedback
      </a>
    </div>
  )
}
