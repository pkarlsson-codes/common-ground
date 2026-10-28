import SlideOver from '../SlideOver'
import NavIdLink from '../NavIdLink'
import OccupancyVerificationSection from './OccupancyVerificationSection'
import PropertyInspectionSection from './PropertyInspectionSection'
import LeaseViolationSection from './LeaseViolationSection'
import { useStewardshipCase } from '../../hooks/stewardship'
import { URGENCY_LABEL, type StewardshipQueueRow } from '../../pages/stewardship/stewardshipSortUtils'
import { fmtD } from './format'

interface Props {
  row: StewardshipQueueRow
  onClose: () => void
}

export default function StewardshipCaseDetailPanel({ row, onClose }: Props) {
  const { data: sc } = useStewardshipCase(row.caseId)

  const title = row.homeownerName !== '—'
    ? row.homeownerName
    : `${row.groundLeaseId.slice(0, 12)}…`

  const subtitle = [
    row.address !== '—' ? row.address : null,
    URGENCY_LABEL[row.urgency],
    `Opened ${fmtD(row.openedDate)}`,
  ].filter(Boolean).join(' · ')

  return (
    <SlideOver title={title} subtitle={subtitle} onClose={onClose}>
      <div className="detail-section">
        <NavIdLink id={row.groundLeaseId} to="/ground-leases" label="Open Ground Lease" />
      </div>
      {sc && (
        <div key={sc.id}>
          <OccupancyVerificationSection sc={sc} />
          <PropertyInspectionSection sc={sc} />
          <LeaseViolationSection sc={sc} />
        </div>
      )}
    </SlideOver>
  )
}
