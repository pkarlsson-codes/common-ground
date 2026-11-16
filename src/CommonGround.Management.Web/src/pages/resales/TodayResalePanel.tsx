import { useGroundLease } from '../../hooks/groundLeases'
import { useResale } from '../../hooks/resales'
import ExercisePurchaseOptionPanel from './ExercisePurchaseOptionPanel'
import RecordBuybackClosedPanel from './RecordBuybackClosedPanel'
import RecordResaleAppraisalPanel from './RecordResaleAppraisalPanel'

export type TodayResaleAction = 'appraisal' | 'exercise' | 'buyback-closing'

/** A Today resale item names the resale; this loads it (and its lease, for the appraisal) and opens the panel for the action. */
export default function TodayResalePanel({ resaleId, action, onClose }: { resaleId: string; action: TodayResaleAction; onClose: () => void }) {
  const { data: resale } = useResale(resaleId)
  const { data: lease } = useGroundLease(resale?.groundLeaseId ?? null)
  if (!resale) return null
  if (action === 'appraisal') return <RecordResaleAppraisalPanel resale={resale} lease={lease} onClose={onClose} />
  if (action === 'exercise') return <ExercisePurchaseOptionPanel resale={resale} onClose={onClose} />
  return <RecordBuybackClosedPanel resale={resale} onClose={onClose} />
}
