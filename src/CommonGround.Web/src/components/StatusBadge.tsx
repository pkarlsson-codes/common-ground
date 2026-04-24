import { statusLabel } from '../utils/statusLabel'

const CSS_CLASS: Record<string, string> = {
  // Property Registry
  Available:        'badge-available',
  Occupied:         'badge-occupied',
  UnderDevelopment: 'badge-underdevelopment',
  UnderRenovation:  'badge-underrenovation',
  // Ground Leases
  Active:           'badge-active',
  Inactive:         'badge-inactive',
  // Purchase Workflows
  Initiated:        'badge-initiated',
  InProgress:       'badge-inprogress',
  ClosingScheduled: 'badge-closingscheduled',
  Closed:           'badge-closed',
  Failed:           'badge-failed',
  Withdrawn:        'badge-withdrawn',
  Stalled:          'badge-stalled',
}

export default function StatusBadge({ status }: { status: string }) {
  return (
    <span className={`badge ${CSS_CLASS[status] ?? 'badge-default'}`}>
      {statusLabel(status)}
    </span>
  )
}
