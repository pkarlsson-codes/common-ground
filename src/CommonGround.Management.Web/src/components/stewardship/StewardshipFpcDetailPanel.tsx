import SlideOver from '../SlideOver'
import NavIdLink from '../NavIdLink'
import { statusLabel } from '../../hooks/types'
import { useForeclosurePreventionCase } from '../../hooks/stewardship'
import ForeclosurePreventionCaseCard from './ForeclosurePreventionCaseCard'

interface Props {
  caseId: string
  homeownerName: string
  address: string
  onClose: () => void
}

export default function StewardshipFpcDetailPanel({
  caseId,
  homeownerName,
  address,
  onClose,
}: Props) {
  const { data: fpc } = useForeclosurePreventionCase(caseId)
  const subtitle = [address, fpc ? statusLabel(fpc.status ?? '') : null].filter(Boolean).join(' · ')

  return (
    <SlideOver
      title={homeownerName}
      subtitle={subtitle}
      onClose={onClose}
    >
      {fpc && (
        <>
          <div className="detail-section">
            <NavIdLink id={fpc.groundLeaseId} to="/ground-leases" label="Open Ground Lease" />
          </div>
          <ForeclosurePreventionCaseCard key={fpc.id} fpc={fpc} />
        </>
      )}
    </SlideOver>
  )
}
