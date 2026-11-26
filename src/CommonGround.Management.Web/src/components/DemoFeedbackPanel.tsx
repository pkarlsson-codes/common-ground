import { useState } from 'react'
import { useLocation } from 'react-router-dom'
import ApiProblemAlert from './ApiProblemAlert'
import SlideOver from './SlideOver'
import SlideOverCancelButton from './SlideOverCancelButton'
import { useCurrentUser } from '../hooks/auth'
import { useSendDemoFeedback } from '../hooks/demo'
import { useToast } from '../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../lib/getErrorMessage'

/** The most characters a message holds, as the API allows. */
const MESSAGE_MAX_LENGTH = 4000

type Props = {
  onClose: () => void
}

/** Sends feedback from a demo server to whoever runs it, with the page the visitor was on. */
export default function DemoFeedbackPanel({ onClose }: Props) {
  const location = useLocation()
  const { data: user } = useCurrentUser()
  const [message, setMessage] = useState('')
  const [mayReply, setMayReply] = useState(true)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useSendDemoFeedback()
  const { success } = useToast()
  const email = user?.email || null

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({
        message: message.trim(),
        page: location.pathname + location.search,
        mayReply: mayReply && email !== null,
      })
      success('Thanks, your feedback was sent')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Could not send your feedback.'))
    }
  }

  return (
    <SlideOver
      title="Send feedback"
      subtitle="What works, what doesn't, and what doesn't match how your trust works. The page you're on is sent with it."
      onClose={onClose}
      isDirty={message.trim().length > 0}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="demo-feedback-form"
            disabled={mutation.isPending || !message.trim()}
          >
            {mutation.isPending ? 'Sending…' : 'Send'}
          </button>
        </>
      }
    >
      <form id="demo-feedback-form" onSubmit={e => { void handleSubmit(e) }}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label" htmlFor="demo-feedback-message">Feedback</label>
            <textarea
              id="demo-feedback-message"
              className="form-textarea"
              rows={8}
              value={message}
              maxLength={MESSAGE_MAX_LENGTH}
              onChange={e => setMessage(e.target.value)}
              required
            />
          </div>
          {email && (
            <div className="form-field full">
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <input type="checkbox" checked={mayReply} onChange={e => setMayReply(e.target.checked)} />
                You can reply to me at {email}
              </label>
            </div>
          )}
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
