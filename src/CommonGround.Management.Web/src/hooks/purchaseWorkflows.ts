import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { PurchaseWorkflowsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type {
  FailWorkflowForm,
  InitiateWorkflowForm,
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
import { localToday } from '../lib/dates'

const api = new PurchaseWorkflowsApi(apiConfig)

export function usePurchaseWorkflows(pageNumber: number, groundLeaseTemplateId?: string, phase?: string, search?: string) {
  return useQuery({
    queryKey: ['purchase-workflows', pageNumber, groundLeaseTemplateId, phase, search],
    queryFn: () => api.getPurchaseWorkflows({ pageNumber, pageSize: 20, groundLeaseTemplateId, phase, search }),
  })
}

export function usePurchaseWorkflow(id: string | null) {
  return useQuery({
    queryKey: ['purchase-workflow', id],
    queryFn: () => api.getPurchaseWorkflow({ workflowId: id! }),
    enabled: !!id,
  })
}

export function useInitiatePurchaseWorkflow() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (form: InitiateWorkflowForm) =>
      api.initiatePurchaseWorkflow({
        initiatePurchaseWorkflowRequest: {
          groundLeaseTemplateId: form.groundLeaseTemplateId,
          landParcelId: form.landParcelId,
          homeId: form.homeId,
          buyerFullName: form.buyerFullName,
          buyerEmail: form.buyerEmail,
          buyerPhone: form.buyerPhone || undefined,
          purchasePrice: form.purchasePrice,
        },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['purchase-workflows'] }),
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
          failedDate: localToday(),
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
          withdrawnDate: localToday(),
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
          stalledDate: localToday(),
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
