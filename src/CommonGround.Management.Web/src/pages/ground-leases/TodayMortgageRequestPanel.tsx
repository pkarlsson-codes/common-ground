import { useState } from 'react'
import { useGroundLease, useMortgageRequests } from '../../hooks/groundLeases'
import { trustToday } from '../../lib/calendarDate'
import MortgageRequestPanel from './MortgageRequestPanel'
import RecordMortgageLoanClosedPanel from './RecordMortgageLoanClosedPanel'

interface Props {
  groundLeaseId: string
  /** Open: the lease's request waiting for a decision. Closed: its oldest approved loan past its expected closing. */
  open: 'pending' | 'closing'
  onClose: () => void
}

/**
 * A Today mortgage item names the lease, not the request. A lease has at most one request waiting for a decision,
 * so the item's request is that one, or for a closing to confirm, the approved loan whose closing passed longest ago.
 */
export default function TodayMortgageRequestPanel({ groundLeaseId, open, onClose }: Props) {
  const { data: requests } = useMortgageRequests(groundLeaseId)
  const { data: lease } = useGroundLease(open === 'closing' ? groundLeaseId : null)
  // Once found, the panel stays on that request: after a decision or closing it no longer matches, and its letter
  // still prints from it.
  const [requestId, setRequestId] = useState<string | null>(null)
  if (!requests) return null

  const today = trustToday()
  const found = open === 'pending'
    ? requests.find(r => r.status === 'Pending')
    : requests
      .filter(r => r.status === 'Approved' && (r.expectedClosingOn ?? today) < today)
      .sort((a, b) => (a.expectedClosingOn ?? '').localeCompare(b.expectedClosingOn ?? ''))[0]
  const id = requestId ?? found?.id ?? null
  if (id && id !== requestId) setRequestId(id)
  if (!id) return null

  if (open === 'pending') return <MortgageRequestPanel mortgageRequestId={id} onClose={onClose} />
  const request = requests.find(r => r.id === id)
  return request && lease ? <RecordMortgageLoanClosedPanel request={request} lease={lease} onClose={onClose} /> : null
}
