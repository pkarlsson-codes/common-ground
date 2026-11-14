import { useGroundLease } from '../../hooks/groundLeases'
import RecordInsuranceCertificatePanel from './RecordInsuranceCertificatePanel'

/** A Today "Insurance certificate due" item names the lease; this loads it and opens the Record Certificate panel. */
export default function TodayInsuranceCertificatePanel({ groundLeaseId, onClose }: { groundLeaseId: string; onClose: () => void }) {
  const { data: lease } = useGroundLease(groundLeaseId)
  return lease ? <RecordInsuranceCertificatePanel lease={lease} onClose={onClose} /> : null
}
