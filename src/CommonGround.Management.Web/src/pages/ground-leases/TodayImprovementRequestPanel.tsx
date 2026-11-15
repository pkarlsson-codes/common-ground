import { useState } from 'react'
import { useImprovementRequests } from '../../hooks/groundLeases'
import ImprovementRequestPanel from './ImprovementRequestPanel'

interface Props {
  groundLeaseId: string
  onClose: () => void
}

/**
 * A Today improvement item names the lease, not the request. The panel opens the lease's oldest request waiting
 * for a decision. Once found it stays on that request: after a decision it no longer waits, and its letter still
 * prints from it.
 */
export default function TodayImprovementRequestPanel({ groundLeaseId, onClose }: Props) {
  const { data: requests } = useImprovementRequests(groundLeaseId)
  const [requestId, setRequestId] = useState<string | null>(null)
  if (!requests) return null

  const found = requests
    .filter(r => r.status === 'Waiting for decision')
    .sort((a, b) => (a.receivedOn ?? '').localeCompare(b.receivedOn ?? ''))[0]
  const id = requestId ?? found?.id ?? null
  if (id && id !== requestId) setRequestId(id)
  if (!id) return null

  return <ImprovementRequestPanel improvementRequestId={id} onClose={onClose} />
}
