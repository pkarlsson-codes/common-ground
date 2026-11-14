import { useGroundLease } from '../../hooks/groundLeases'
import PrepareResalePriceStatementPanel from './PrepareResalePriceStatementPanel'

/** A Today "Resale price statement due" item names the lease; this loads it and opens the statement panel. */
export default function TodayResalePriceStatementPanel({ groundLeaseId, onClose }: { groundLeaseId: string; onClose: () => void }) {
  const { data: lease } = useGroundLease(groundLeaseId)
  return lease ? <PrepareResalePriceStatementPanel lease={lease} onClose={onClose} /> : null
}
