import type { FormEvent } from 'react'
import type { PurchaseWorkflowResponse } from '../../api'
import type {
  FailWorkflowForm,
  RecordClosedForm,
  RecordEligibilityForm,
  RecordFinancingForm,
  RecordInspectionForm,
  RecordLeaseExecutedForm,
  RecordTitleForm,
  ScheduleClosingForm,
  StallWorkflowForm,
  WithdrawWorkflowForm,
} from '../../hooks/types'
import { getAvailableActions } from '../../lib/transitionActions'
import { trackTransitionEvent } from '../../lib/telemetry'

type TrackMutation<T> = {
  mutateAsync: (args: { workflowId: string; form: T }) => Promise<unknown>
}

interface UsePurchaseWorkflowActionsParams {
  selectedId: string | null
  detail: PurchaseWorkflowResponse | undefined
  showToast: (msg: string, kind?: 'success' | 'error') => void
  eligMutation: TrackMutation<RecordEligibilityForm>
  finMutation: TrackMutation<RecordFinancingForm>
  inspMutation: TrackMutation<RecordInspectionForm>
  titleMutation: TrackMutation<RecordTitleForm>
  leaseExecMutation: TrackMutation<RecordLeaseExecutedForm>
  scheduleMutation: TrackMutation<ScheduleClosingForm>
  closeMutation: TrackMutation<RecordClosedForm>
  failMutation: TrackMutation<FailWorkflowForm>
  withdrawMutation: TrackMutation<WithdrawWorkflowForm>
  stallMutation: TrackMutation<StallWorkflowForm>
}

const isTerminal = (phase?: string) =>
  phase === 'Closed' || phase === 'Failed' || phase === 'Withdrawn'

const allTracksComplete = (wf: PurchaseWorkflowResponse) =>
  !!wf.eligibilityDetails &&
  !!wf.financingDetails &&
  !!wf.inspectionResult &&
  !!wf.titleDetails &&
  !!wf.leaseExecutionDetails

export function usePurchaseWorkflowActions({
  selectedId,
  detail,
  showToast,
  eligMutation,
  finMutation,
  inspMutation,
  titleMutation,
  leaseExecMutation,
  scheduleMutation,
  closeMutation,
  failMutation,
  withdrawMutation,
  stallMutation,
}: UsePurchaseWorkflowActionsParams) {
  const makeTrackHandler = <T,>(
    mutate: (args: { workflowId: string; form: T }) => Promise<unknown>,
    actionId: string,
    successMsg: string,
    options?: { toState?: string; requiresConfirmation?: boolean; confirmationMessage?: string },
  ) => async (e: FormEvent, form: T) => {
    e.preventDefault()
    if (!selectedId) return

    if (options?.requiresConfirmation && !window.confirm(options.confirmationMessage ?? 'Are you sure?')) {
      return
    }

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
    } catch (error) {
      showToast('Action failed', 'error')
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

  const handleElig = makeTrackHandler(eligMutation.mutateAsync, 'workflow:record-eligibility', 'Eligibility recorded', { toState: 'InProgress' })
  const handleFin = makeTrackHandler(finMutation.mutateAsync, 'workflow:record-financing', 'Financing committed', { toState: 'InProgress' })
  const handleInsp = makeTrackHandler(inspMutation.mutateAsync, 'workflow:record-inspection', 'Inspection recorded', { toState: 'InProgress' })
  const handleTitle = makeTrackHandler(titleMutation.mutateAsync, 'workflow:record-title', 'Title cleared', { toState: 'InProgress' })
  const handleLeaseEx = makeTrackHandler(leaseExecMutation.mutateAsync, 'workflow:record-lease-executed', 'Lease executed', { toState: 'InProgress' })
  const handleSchedule = makeTrackHandler(scheduleMutation.mutateAsync, 'workflow:schedule-closing', 'Closing scheduled', { toState: 'ClosingScheduled' })
  const handleClose = makeTrackHandler(closeMutation.mutateAsync, 'workflow:record-closed', 'Purchase closed', { toState: 'Closed' })
  const handleFail = makeTrackHandler(failMutation.mutateAsync, 'workflow:fail', 'Workflow marked failed', {
    toState: 'Failed',
    requiresConfirmation: true,
    confirmationMessage: 'Mark this workflow as failed? This cannot be undone.',
  })
  const handleWithdraw = makeTrackHandler(withdrawMutation.mutateAsync, 'workflow:withdraw', 'Workflow withdrawn', {
    toState: 'Withdrawn',
    requiresConfirmation: true,
    confirmationMessage: 'Withdraw this workflow? This cannot be undone.',
  })
  const handleStall = makeTrackHandler(stallMutation.mutateAsync, 'workflow:stall', 'Workflow stalled', { toState: 'Stalled' })

  const workflowActions = detail ? getAvailableActions('workflow', detail.phase ?? '') : []
  const hasWorkflowAction = (id: string) => workflowActions.some(a => a.id === id)
  const canSchedule = !!detail && hasWorkflowAction('workflow:schedule-closing') && allTracksComplete(detail)
  const canClose = !!detail && hasWorkflowAction('workflow:record-closed')
  const canAct = !!detail && !isTerminal(detail.phase)

  return {
    handleElig,
    handleFin,
    handleInsp,
    handleTitle,
    handleLeaseEx,
    handleSchedule,
    handleClose,
    handleFail,
    handleWithdraw,
    handleStall,
    hasWorkflowAction,
    canSchedule,
    canClose,
    canAct,
  }
}
