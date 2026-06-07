import { statusLabel } from '../../utils/statusLabel'

const PHASE_CLASS: Record<string, string> = {
  Submitted: 'badge-initiated',
  Active: 'badge-inprogress',
  Waitlisted: 'badge-available',
  Selected: 'badge-closed',
  EligibilityDenied: 'badge-failed',
  Denied: 'badge-failed',
  Withdrawn: 'badge-withdrawn',
}

const PHASE_LABEL: Record<string, string> = {
  Submitted: 'Submitted',
  Active: 'Active',
  Waitlisted: 'Waitlisted',
  Selected: 'Selected',
  EligibilityDenied: 'Eligibility Denied',
  Denied: 'Denied',
  Withdrawn: 'Withdrawn',
}

type Props = {
  phase?: string | null
}

export default function ApplicationPhaseBadge({ phase }: Props) {
  const key = phase ?? 'Unknown'
  return (
    <span className={`badge ${PHASE_CLASS[key] ?? 'badge-default'}`}>
      {PHASE_LABEL[key] ?? statusLabel(key)}
    </span>
  )
}
