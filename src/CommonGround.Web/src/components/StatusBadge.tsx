import { statusLabel } from '../utils/statusLabel'

const CSS_CLASS: Record<string, string> = {
  // Property Registry
  Available:        'badge-available',
  PendingOccupancy: 'badge-initiated',
  Resale:           'badge-stalled',
  Abandoned:        'badge-failed',
  Predevelopment:   'badge-underdevelopment',
  UnderConstruction:'badge-underdevelopment',
  ReadyForOccupancy:'badge-available',
  Occupied:         'badge-occupied',
  Transitioning:    'badge-stalled',
  Decommissioned:   'badge-failed',
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
  // Applicant Selection phases
  EligibilityReview:'badge-inprogress',
  Eligible:         'badge-available',
  Selected:         'badge-closed',
  Denied:           'badge-failed',
  // Occupancy Verification
  Pending:          'badge-initiated',
  Submitted:        'badge-inprogress',
  Verified:         'badge-closed',
  Overdue:          'badge-stalled',
  // Property Inspection
  Scheduled:        'badge-initiated',
  Completed:        'badge-closed',
  Cancelled:        'badge-withdrawn',
  // Lease Violations
  Identified:       'badge-initiated',
  NoticeIssued:     'badge-inprogress',
  Cured:            'badge-closed',
  Escalated:        'badge-failed',
  // Foreclosure Prevention
  Opened:           'badge-initiated',
  Resolved:         'badge-closed',
}

export default function StatusBadge({ status }: { status: string }) {
  return (
    <span className={`badge ${CSS_CLASS[status] ?? 'badge-default'}`}>
      {statusLabel(status)}
    </span>
  )
}
