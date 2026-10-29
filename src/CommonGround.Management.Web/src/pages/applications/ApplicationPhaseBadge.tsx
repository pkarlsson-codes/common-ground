import { statusLabel } from '../../utils/statusLabel'

const PHASE_CLASS: Record<string, string> = {
  Submitted: 'badge-initiated',
  Approved: 'badge-available',
  Selected: 'badge-closed',
  Denied: 'badge-failed',
  Withdrawn: 'badge-withdrawn',
}

const PHASE_LABEL: Record<string, string> = {
  Submitted: 'Submitted',
  Approved: 'Approved · on waiting list',
  Selected: 'Selected',
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
