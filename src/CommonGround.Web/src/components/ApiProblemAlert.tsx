import type { ApiProblemDetails } from '../lib/getErrorMessage'

type Props = {
  problem: ApiProblemDetails | null
  onRetry?: () => void
}

export default function ApiProblemAlert({ problem, onRetry }: Props) {
  if (!problem) {
    return null
  }

  const fieldErrors = Object.entries(problem.errors ?? {})

  return (
    <div className="form-error" role="alert" aria-live="polite">
      <div>{problem.message}</div>

      {fieldErrors.length > 0 && (
        <ul className="form-error__list">
          {fieldErrors.map(([field, messages]) => {
            const normalizedField = field.replace(/^\$\./, '')
            return (
              <li key={field}>
                <span className="form-error__field">{normalizedField}:</span> {messages.join(', ')}
              </li>
            )
          })}
        </ul>
      )}

      {onRetry && (
        <div style={{ marginTop: 10 }}>
          <button type="button" className="btn btn-ghost btn-sm" onClick={onRetry}>Retry</button>
        </div>
      )}
    </div>
  )
}
