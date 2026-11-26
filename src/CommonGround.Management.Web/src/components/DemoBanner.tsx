import { useState } from 'react'
import { useDemo } from '../hooks/demo'
import { formatCalendarDate, longDate } from '../lib/calendarDate'
import DemoFeedbackPanel from './DemoFeedbackPanel'

/** On a demo server, says the trust holds sample data and when it is deleted, with a button to send feedback. */
export default function DemoBanner() {
  const { data: demo } = useDemo()
  const [sendingFeedback, setSendingFeedback] = useState(false)
  if (!demo?.deletesOn) return null

  // The panel sits outside the banner so it doesn't take on the banner's colour and size.
  return (
    <>
      <div className="demo-banner" role="note" aria-label="Demo">
        <span>
          Sample data for trying CommonGround. Don't enter real personal information.
          This trust is deleted on {formatCalendarDate(demo.deletesOn, longDate)}.
        </span>
        <button type="button" className="demo-banner__feedback" onClick={() => setSendingFeedback(true)}>
          Send feedback
        </button>
      </div>
      {sendingFeedback && <DemoFeedbackPanel onClose={() => setSendingFeedback(false)} />}
    </>
  )
}
