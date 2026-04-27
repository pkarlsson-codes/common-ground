import SlideOver from '../SlideOver'
import { statusLabel } from '../../hooks/types'
import type { ForeclosurePreventionCaseResponse } from '../../api/models/ForeclosurePreventionCaseResponse'
import ForeclosurePreventionCaseCard from './ForeclosurePreventionCaseCard'

interface Props {
  fpcCase: ForeclosurePreventionCaseResponse
  homeownerName: string
  address: string
  onClose: () => void
}

export default function StewardshipFpcDetailPanel({
  fpcCase,
  homeownerName,
  address,
  onClose,
}: Props) {
  const subtitle = [address, statusLabel(fpcCase.status ?? '')].filter(Boolean).join(' · ')

  return (
    <SlideOver
      title={homeownerName}
      subtitle={subtitle}
      onClose={onClose}
    >
      <ForeclosurePreventionCaseCard
        fpc={fpcCase}
        groundLeaseId={fpcCase.groundLeaseId ?? ''}
      />
    </SlideOver>
  )
}
