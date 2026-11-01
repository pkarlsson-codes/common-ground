import type { LifecycleStep } from '../../lib/lifecycle'

/** A record's lifecycle: done steps with their date, the current step highlighted, later steps dimmed. */
export default function LifecycleStepper({ steps, label }: { steps: LifecycleStep[]; label: string }) {
  return (
    <ol className="lifecycle-stepper" aria-label={label}>
      {steps.map(step => (
        <li
          key={step.label}
          className={`lifecycle-step lifecycle-step--${step.state}`}
          aria-current={step.state === 'current' ? 'step' : undefined}
        >
          <span className="lifecycle-step__dot" aria-hidden="true">{step.state === 'done' ? '✓' : ''}</span>
          <span className="lifecycle-step__label">{step.label}</span>
          {step.date && <span className="lifecycle-step__date">{step.date}</span>}
        </li>
      ))}
    </ol>
  )
}

