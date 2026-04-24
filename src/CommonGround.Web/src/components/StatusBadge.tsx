import { statusLabel } from '../utils/statusLabel'

const CSS_CLASS: Record<string, string> = {
  Available:        'badge-available',
  Occupied:         'badge-occupied',
  UnderDevelopment: 'badge-underdevelopment',
  UnderRenovation:  'badge-underrenovation',
}

export default function StatusBadge({ status }: { status: string }) {
  return (
    <span className={`badge ${CSS_CLASS[status] ?? 'badge-default'}`}>
      {statusLabel(status)}
    </span>
  )
}
