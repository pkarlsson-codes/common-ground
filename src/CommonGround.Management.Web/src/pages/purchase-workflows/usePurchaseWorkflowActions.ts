import type { FormEvent } from 'react'
import type { PurchaseWorkflowResponse } from '../../api'
import type {
  FailWorkflowForm,
  RecordClosedForm,
  RecordAppraisalForm,
  RecordDisclosureForm,
  RecordEligibilityForm,
  RecordFinancingForm,
  RecordInspectionForm,
  RecordLeaseExecutedForm,
  RecordTitleForm,
  ScheduleClosingForm,
  WithdrawWorkflowForm,
} from '../../hooks/types'
import { getAvailableActions } from '../../lib/transitionActions'
import { trackTransitionEvent } from '../../lib/telemetry'
import type { TrackKey } from './usePurchaseWorkflowDetailController'
import { getErrorMessage } from '../../lib/getErrorMessage'

type TrackMutation<T> = {
  mutateAsync: (args: { workflowId: string; form: T }) => Promise<unknown>
}

interface UsePurchaseWorkflowActionsParams {
  selectedId: string | null
  detail: PurchaseWorkflowResponse | undefined
  showToast: (msg: string, kind?: 'success' | 'error') => void
  eligMutation: TrackMutation<RecordEligibilityForm>
  appraisalMutation: TrackMutation<RecordAppraisalForm>
  finMutation: TrackMutation<RecordFinancingForm>
  inspMutation: TrackMutation<RecordInspectionForm>
  titleMutation: TrackMutation<RecordTitleForm>
  leaseExecMutation: TrackMutation<RecordLeaseExecutedForm>
  disclosureMutation: TrackMutation<RecordDisclosureForm>
  scheduleMutation: TrackMutation<ScheduleClosingForm>
  closeMutation: TrackMutation<RecordClosedForm>
  failMutation: TrackMutation<FailWorkflowForm>
  withdrawMutation: TrackMutation<WithdrawWorkflowForm>
  onTrackSuccess?: (key: TrackKey) => void
}

const isTerminal = (phase?: string) =>
  phase === 'Closed' || phase === 'Failed' || phase === 'Withdrawn'

const allTracksComplete = (wf: PurchaseWorkflowResponse) =>
  !!wf.eligibilityDetails &&
  !!wf.appraisalDetails &&
  !!wf.financingDetails &&
  !!wf.inspectionResult &&
  !!wf.titleDetails &&
  !!wf.leaseExecutionDetails &&
  !!wf.disclosureDetails

export function usePurchaseWorkflowActions({
  selectedId,
  detail,
  showToast,
  eligMutation,
  appraisalMutation,
  finMutation,
  inspMutation,
  titleMutation,
  leaseExecMutation,
  disclosureMutation,
  scheduleMutation,
  closeMutation,
  failMutation,
  withdrawMutation,
  onTrackSuccess,
}: UsePurchaseWorkflowActionsParams) {
  const makeTrackHandler = <T,>(
    mutate: (args: { workflowId: string; form: T }) => Promise<unknown>,
    actionId: string,
    successMsg: string,
    options?: { toState?: string; trackKey?: TrackKey },
  ) => async (e: FormEvent, form: T) => {
    e.preventDefault()
    if (!selectedId) return

    trackTransitionEvent({
      event: 'ui.transition',
      aggregate: 'workflow',
      actionId,
      entityId: selectedId,
      fromState: detail?.phase ?? null,
      toState: options?.toState ?? null,
      outcome: 'attempt',
    })

    try {
      await mutate({ workflowId: selectedId, form })
      showToast(successMsg)
      trackTransitionEvent({
        event: 'ui.transition',
        aggregate: 'workflow',
        actionId,
        entityId: selectedId,
        fromState: detail?.phase ?? null,
        toState: options?.toState ?? null,
        outcome: 'success',
      })
      if (options?.trackKey && onTrackSuccess) {
        onTrackSuccess(options.trackKey)
      }
    } catch (error) {
      showToast(await getErrorMessage(error, 'Action failed'), 'error')
      trackTransitionEvent({
        event: 'ui.transition',
        aggregate: 'workflow',
        actionId,
        entityId: selectedId,
        fromState: detail?.phase ?? null,
        toState: options?.toState ?? null,
        outcome: 'failure',
        error: error instanceof Error ? error.message : 'Unknown error',
      })
    }
  }

  const handleElig = makeTrackHandler(eligMutation.mutateAsync, 'workflow:record-eligibility', 'Eligibility recorded', { toState: 'InProgress', trackKey: 'eligibility' })
  const handleAppraisal = makeTrackHandler(appraisalMutation.mutateAsync, 'workflow:record-appraisal', 'Appraisal recorded', { toState: 'InProgress', trackKey: 'appraisal' })
  const handleFin = makeTrackHandler(finMutation.mutateAsync, 'workflow:record-financing', 'Financing committed', { toState: 'InProgress', trackKey: 'financing' })
  const handleInsp = makeTrackHandler(inspMutation.mutateAsync, 'workflow:record-inspection', 'Inspection recorded', { toState: 'InProgress', trackKey: 'inspection' })
  const handleTitle = makeTrackHandler(titleMutation.mutateAsync, 'workflow:record-title', 'Title cleared', { toState: 'InProgress', trackKey: 'title' })
  const handleLeaseEx = makeTrackHandler(leaseExecMutation.mutateAsync, 'workflow:record-lease-executed', 'Lease executed', { toState: 'InProgress', trackKey: 'leaseExecution' })
  const handleDisclosure = makeTrackHandler(disclosureMutation.mutateAsync, 'workflow:record-disclosure', 'Disclosure recorded', { toState: 'InProgress', trackKey: 'disclosure' })
  const handleSchedule = makeTrackHandler(scheduleMutation.mutateAsync, 'workflow:schedule-closing', 'Closing scheduled', { toState: 'ClosingScheduled', trackKey: 'closing' })
  const handleClose = makeTrackHandler(closeMutation.mutateAsync, 'workflow:record-closed', 'Purchase closed', { toState: 'Closed', trackKey: 'closing' })
  const handleFail = makeTrackHandler(failMutation.mutateAsync, 'workflow:fail', 'Workflow marked failed', { toState: 'Failed' })
  const handleWithdraw = makeTrackHandler(withdrawMutation.mutateAsync, 'workflow:withdraw', 'Workflow withdrawn', { toState: 'Withdrawn' })

  const workflowActions = detail ? getAvailableActions('workflow', detail.phase ?? '') : []
  const hasWorkflowAction = (id: string) => workflowActions.some(a => a.id === id)
  const canSchedule = !!detail && hasWorkflowAction('workflow:schedule-closing') && allTracksComplete(detail)
  const canClose = !!detail && hasWorkflowAction('workflow:record-closed')
  const canAct = !!detail && !isTerminal(detail.phase)

  return {
    handleElig,
    handleAppraisal,
    handleFin,
    handleInsp,
    handleTitle,
    handleLeaseEx,
    handleDisclosure,
    handleSchedule,
    handleClose,
    handleFail,
    handleWithdraw,
    hasWorkflowAction,
    canSchedule,
    canClose,
    canAct,
  }
}
