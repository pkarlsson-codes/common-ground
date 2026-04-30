import SlideOver from '../SlideOver'
import OccupancyVerificationSection from './OccupancyVerificationSection'
import PropertyInspectionSection from './PropertyInspectionSection'
import LeaseViolationSection from './LeaseViolationSection'
import type { StewardshipQueueRow } from '../../pages/stewardship/stewardshipSortUtils'
import type { GroundLeaseListItemResponse } from '../../api'

interface Props {
  row: StewardshipQueueRow
  lease: GroundLeaseListItemResponse | undefined
  onClose: () => void
}

const URGENCY_LABEL: Record<string, string> = {
  red: 'Urgent',
  orange: 'High',
  yellow: 'Elevated',
  neutral: 'Normal',
}

export default function StewardshipCaseDetailPanel({ row, lease, onClose }: Props) {
  const title = lease?.homeownerFullName
    ?? (row.groundLeaseId.length > 12
      ? `${row.groundLeaseId.slice(0, 12)}…`
      : row.groundLeaseId)

  const subtitleParts = [
    row.address,
    URGENCY_LABEL[row.urgency],
    `Opened ${row.openedDate.toISOString().slice(0, 10)}`,
  ].filter(Boolean)

  const subtitle = subtitleParts.join(' · ')

  return (
    <SlideOver title={title} subtitle={subtitle} onClose={onClose}>
      <OccupancyVerificationSection groundLeaseId={row.groundLeaseId} caseId={row.caseId} />
      <PropertyInspectionSection groundLeaseId={row.groundLeaseId} caseId={row.caseId} />
      <LeaseViolationSection groundLeaseId={row.groundLeaseId} caseId={row.caseId} />
    </SlideOver>
  )
}
