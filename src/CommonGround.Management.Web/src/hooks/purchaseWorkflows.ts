import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { PurchaseWorkflowsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
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
} from './types'
import type { PurchaseWorkflowResponse } from '../api/models/PurchaseWorkflowResponse'
import type { PurchaseWorkflowListItemResponse } from '../api/models/PurchaseWorkflowListItemResponse'
import { trustToday } from '../lib/calendarDate'

const api = new PurchaseWorkflowsApi(apiConfig)

export function usePurchaseWorkflows(pageNumber: number, groundLeaseTemplateId?: string, phase?: string, search?: string, enabled = true) {
  return useQuery({
    queryKey: ['purchase-workflows', pageNumber, groundLeaseTemplateId, phase, search],
    queryFn: () => api.getPurchaseWorkflows({ pageNumber, pageSize: 20, groundLeaseTemplateId, phase, search }),
    enabled,
  })
}

/** Every open purchase, in one column per step it waits on. */
export function usePurchaseBoard() {
  return useQuery({
    queryKey: ['purchase-workflows', 'board'],
    queryFn: () => api.getPurchaseBoard(),
  })
}

/** The purchases of a home, or of a household, newest first. */
export function useConnectedPurchaseWorkflows(filter: { homeId?: string; householdId?: string }) {
  return useQuery({
    queryKey: ['purchase-workflows', 'connected', filter.homeId, filter.householdId],
    queryFn: () => api.getPurchaseWorkflows({ pageNumber: 1, pageSize: 50, homeId: filter.homeId, householdId: filter.householdId }),
    enabled: !!(filter.homeId || filter.householdId),
  })
}

/** What staff call each step a purchase can wait on, by the API's step name. */
export const PURCHASE_STEP_LABELS: Record<string, string> = {
  Eligibility: 'Eligibility',
  Financing: 'Financing',
  Inspection: 'Inspection',
  Title: 'Title',
  LeaseSigning: 'Lease signing',
  Closing: 'Closing',
}

const TERMINAL_PHASES = ['Closed', 'Failed', 'Withdrawn']

/** Whether a purchase in <paramref name="phase"/> is still under way. */
export const isOpenPurchasePhase = (phase?: string | null) => !!phase && !TERMINAL_PHASES.includes(phase)

/** The step an open purchase is waiting on, in the order the tracks are usually worked. */
export function purchaseWaitingOn(workflow: PurchaseWorkflowResponse): string {
  if (workflow.phase === 'Stalled') return 'resolving the stall'
  if (!workflow.eligibilityDetails) return 'eligibility verification'
  if (!workflow.financingDetails) return 'the financing commitment'
  if (!workflow.inspectionResult) return 'the inspection'
  if (!workflow.titleDetails) return 'title clearance'
  if (!workflow.leaseExecutionDetails) return 'the lease signing'
  if (!workflow.scheduledClosingDate) return 'scheduling the closing'
  return 'the closing'
}

export function usePurchaseWorkflow(id: string | null) {
  return useQuery({
    queryKey: ['purchase-workflow', id],
    queryFn: () => api.getPurchaseWorkflow({ workflowId: id! }),
    enabled: !!id,
  })
}

const invalidateWorkflow = (qc: ReturnType<typeof useQueryClient>, workflowId: string) => {
  qc.invalidateQueries({ queryKey: ['purchase-workflow', workflowId] })
  qc.invalidateQueries({ queryKey: ['purchase-workflows'] })
}

const applyOptimisticWorkflowPhase = (
  qc: ReturnType<typeof useQueryClient>,
  workflowId: string,
  phase: string,
) => {
  const previousDetail = qc.getQueryData<PurchaseWorkflowResponse | undefined>(['purchase-workflow', workflowId])
  const previousLists = qc.getQueriesData({ queryKey: ['purchase-workflows'] })

  qc.setQueryData<PurchaseWorkflowResponse | undefined>(['purchase-workflow', workflowId], (current) =>
    current ? { ...current, phase } : current,
  )

  previousLists.forEach(([queryKey]) => {
    qc.setQueryData(queryKey, (current: { items?: PurchaseWorkflowListItemResponse[] } | undefined) => {
      if (!current?.items) return current

      return {
        ...current,
        items: current.items.map((item) =>
          item.id === workflowId ? { ...item, phase } : item,
        ),
      }
    })
  })

  return { previousDetail, previousLists }
}

const rollbackOptimisticWorkflowPhase = (
  qc: ReturnType<typeof useQueryClient>,
  workflowId: string,
  context?: { previousDetail: unknown; previousLists: [readonly unknown[], unknown][] },
) => {
  if (!context) return

  qc.setQueryData(['purchase-workflow', workflowId], context.previousDetail)
  context.previousLists.forEach(([queryKey, data]) => {
    qc.setQueryData(queryKey, data)
  })
}

const applyOptimisticTrackDetails = (
  qc: ReturnType<typeof useQueryClient>,
  workflowId: string,
  patch: Record<string, unknown>,
) => {
  const previousDetail = qc.getQueryData<PurchaseWorkflowResponse | undefined>(['purchase-workflow', workflowId])

  qc.setQueryData<PurchaseWorkflowResponse | undefined>(['purchase-workflow', workflowId], (current) =>
    current ? { ...current, ...patch } : current,
  )

  return { previousDetail }
}

const rollbackOptimisticTrackDetails = (
  qc: ReturnType<typeof useQueryClient>,
  workflowId: string,
  context?: { previousDetail: unknown },
) => {
  if (!context) return

  qc.setQueryData(['purchase-workflow', workflowId], context.previousDetail)
}

export function useRecordEligibilityVerified() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: RecordEligibilityForm }) =>
      api.recordEligibilityVerified({
        workflowId,
        recordEligibilityVerifiedRequest: {
          verifiedDate: form.verifiedDate,
          notes: form.notes || undefined,
        },
      }),
    onMutate: async ({ workflowId, form }) => {
      await qc.cancelQueries({ queryKey: ['purchase-workflow', workflowId] })
      await qc.cancelQueries({ queryKey: ['purchase-workflows'] })

      const context = applyOptimisticTrackDetails(qc, workflowId, {
        eligibilityDetails: {
          verifiedDate: form.verifiedDate,
          notes: form.notes || undefined,
        },
      })
      return { workflowId, ...context }
    },
    onError: (_e, vars, context) => {
      rollbackOptimisticTrackDetails(qc, vars.workflowId, context)
    },
    onSettled: (_d, _e, vars) => invalidateWorkflow(qc, vars.workflowId),
  })
}

export function useRecordFinancingCommitted() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: RecordFinancingForm }) =>
      api.recordFinancingCommitted({
        workflowId,
        recordFinancingCommittedRequest: {
          lenderName: form.lenderName,
          committedAmount: form.committedAmount,
          preapprovalLetterDate: form.preapprovalLetterDate,
        },
      }),
    onMutate: async ({ workflowId, form }) => {
      await qc.cancelQueries({ queryKey: ['purchase-workflow', workflowId] })
      await qc.cancelQueries({ queryKey: ['purchase-workflows'] })

      const context = applyOptimisticTrackDetails(qc, workflowId, {
        financingDetails: {
          lenderName: form.lenderName,
          committedAmount: form.committedAmount,
          preapprovalLetterDate: form.preapprovalLetterDate,
        },
      })
      return { workflowId, ...context }
    },
    onError: (_e, vars, context) => {
      rollbackOptimisticTrackDetails(qc, vars.workflowId, context)
    },
    onSettled: (_d, _e, vars) => invalidateWorkflow(qc, vars.workflowId),
  })
}

export function useRecordInspectionCompleted() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: RecordInspectionForm }) =>
      api.recordInspectionCompleted({
        workflowId,
        recordInspectionCompletedRequest: {
          inspectionDate: form.inspectionDate,
          inspectorName: form.inspectorName,
          notes: form.notes || undefined,
        },
      }),
    onMutate: async ({ workflowId, form }) => {
      await qc.cancelQueries({ queryKey: ['purchase-workflow', workflowId] })
      await qc.cancelQueries({ queryKey: ['purchase-workflows'] })

      const context = applyOptimisticTrackDetails(qc, workflowId, {
        inspectionResult: {
          inspectionDate: form.inspectionDate,
          inspectorName: form.inspectorName,
          notes: form.notes || undefined,
        },
      })
      return { workflowId, ...context }
    },
    onError: (_e, vars, context) => {
      rollbackOptimisticTrackDetails(qc, vars.workflowId, context)
    },
    onSettled: (_d, _e, vars) => invalidateWorkflow(qc, vars.workflowId),
  })
}

export function useRecordTitleCleared() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: RecordTitleForm }) =>
      api.recordTitleCleared({
        workflowId,
        recordTitleClearedRequest: {
          clearedDate: form.clearedDate,
          titleCompanyName: form.titleCompanyName || undefined,
        },
      }),
    onMutate: async ({ workflowId, form }) => {
      await qc.cancelQueries({ queryKey: ['purchase-workflow', workflowId] })
      await qc.cancelQueries({ queryKey: ['purchase-workflows'] })

      const context = applyOptimisticTrackDetails(qc, workflowId, {
        titleDetails: {
          clearedDate: form.clearedDate,
          titleCompanyName: form.titleCompanyName || undefined,
        },
      })
      return { workflowId, ...context }
    },
    onError: (_e, vars, context) => {
      rollbackOptimisticTrackDetails(qc, vars.workflowId, context)
    },
    onSettled: (_d, _e, vars) => invalidateWorkflow(qc, vars.workflowId),
  })
}

export function useRecordLeaseExecuted() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: RecordLeaseExecutedForm }) =>
      api.recordLeaseExecuted({
        workflowId,
        recordLeaseExecutedRequest: { signedDate: form.signedDate },
      }),
    onMutate: async ({ workflowId, form }) => {
      await qc.cancelQueries({ queryKey: ['purchase-workflow', workflowId] })
      await qc.cancelQueries({ queryKey: ['purchase-workflows'] })

      const context = applyOptimisticTrackDetails(qc, workflowId, {
        leaseExecutionDetails: { signedDate: form.signedDate },
      })
      return { workflowId, ...context }
    },
    onError: (_e, vars, context) => {
      rollbackOptimisticTrackDetails(qc, vars.workflowId, context)
    },
    onSettled: (_d, _e, vars) => invalidateWorkflow(qc, vars.workflowId),
  })
}

export function useScheduleClosing() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: ScheduleClosingForm }) =>
      api.scheduleClosing({
        workflowId,
        scheduleClosingRequest: { scheduledClosingDate: form.scheduledDate },
      }),
    onMutate: async ({ workflowId }) => {
      await qc.cancelQueries({ queryKey: ['purchase-workflow', workflowId] })
      await qc.cancelQueries({ queryKey: ['purchase-workflows'] })

      const context = applyOptimisticWorkflowPhase(qc, workflowId, 'ClosingScheduled')
      return { workflowId, ...context }
    },
    onError: (_e, vars, context) => {
      rollbackOptimisticWorkflowPhase(qc, vars.workflowId, context)
    },
    onSettled: (_d, _e, vars) => invalidateWorkflow(qc, vars.workflowId),
  })
}

export function useRecordPurchaseClosed() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: RecordClosedForm }) =>
      api.recordPurchaseClosed({
        workflowId,
        recordPurchaseClosedRequest: {
          actualClosingDate: form.actualClosingDate,
          notes: form.closingNotes || undefined,
        },
      }),
    onMutate: async ({ workflowId }) => {
      await qc.cancelQueries({ queryKey: ['purchase-workflow', workflowId] })
      await qc.cancelQueries({ queryKey: ['purchase-workflows'] })

      const context = applyOptimisticWorkflowPhase(qc, workflowId, 'Closed')
      return { workflowId, ...context }
    },
    onError: (_e, vars, context) => {
      rollbackOptimisticWorkflowPhase(qc, vars.workflowId, context)
    },
    onSettled: (_d, _e, vars) => invalidateWorkflow(qc, vars.workflowId),
  })
}

export function useFailPurchase() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: FailWorkflowForm }) =>
      api.failPurchase({
        workflowId,
        failPurchaseRequest: {
          failureReason: form.reason,
          failedDate: trustToday(),
        },
      }),
    onMutate: async ({ workflowId }) => {
      await qc.cancelQueries({ queryKey: ['purchase-workflow', workflowId] })
      await qc.cancelQueries({ queryKey: ['purchase-workflows'] })

      const context = applyOptimisticWorkflowPhase(qc, workflowId, 'Failed')
      return { workflowId, ...context }
    },
    onError: (_e, vars, context) => {
      rollbackOptimisticWorkflowPhase(qc, vars.workflowId, context)
    },
    onSettled: (_d, _e, vars) => invalidateWorkflow(qc, vars.workflowId),
  })
}

export function useWithdrawPurchase() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: WithdrawWorkflowForm }) =>
      api.withdrawPurchase({
        workflowId,
        withdrawPurchaseRequest: {
          reason: form.reason,
          withdrawnDate: trustToday(),
        },
      }),
    onMutate: async ({ workflowId }) => {
      await qc.cancelQueries({ queryKey: ['purchase-workflow', workflowId] })
      await qc.cancelQueries({ queryKey: ['purchase-workflows'] })

      const context = applyOptimisticWorkflowPhase(qc, workflowId, 'Withdrawn')
      return { workflowId, ...context }
    },
    onError: (_e, vars, context) => {
      rollbackOptimisticWorkflowPhase(qc, vars.workflowId, context)
    },
    onSettled: (_d, _e, vars) => invalidateWorkflow(qc, vars.workflowId),
  })
}

export function useStallWorkflow() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: StallWorkflowForm }) =>
      api.stallWorkflow({
        workflowId,
        stallWorkflowRequest: {
          stalledReason: form.reason,
          stalledDate: trustToday(),
          resolutionTargetDate: form.resolutionTargetDate || undefined,
        },
      }),
    onMutate: async ({ workflowId }) => {
      await qc.cancelQueries({ queryKey: ['purchase-workflow', workflowId] })
      await qc.cancelQueries({ queryKey: ['purchase-workflows'] })

      const context = applyOptimisticWorkflowPhase(qc, workflowId, 'Stalled')
      return { workflowId, ...context }
    },
    onError: (_e, vars, context) => {
      rollbackOptimisticWorkflowPhase(qc, vars.workflowId, context)
    },
    onSettled: (_d, _e, vars) => invalidateWorkflow(qc, vars.workflowId),
  })
}
