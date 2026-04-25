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

const api = new PurchaseWorkflowsApi(apiConfig)

export function usePurchaseWorkflows(page: number, groundLeaseId?: string, phase?: string, search?: string) {
  return useQuery({
    queryKey: ['purchase-workflows', page, groundLeaseId, phase, search],
    queryFn: () => api.getPurchaseWorkflows({ page, pageSize: 20, groundLeaseId, phase, search }),
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
          groundLeaseId: form.groundLeaseId,
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
  const previousDetail = qc.getQueryData(['purchase-workflow', workflowId])
  const previousLists = qc.getQueriesData({ queryKey: ['purchase-workflows'] })

  qc.setQueryData(['purchase-workflow', workflowId], (current: any) =>
    current ? { ...current, phase } : current,
  )

  previousLists.forEach(([queryKey]) => {
    qc.setQueryData(queryKey, (current: any) => {
      if (!current?.items) return current

      return {
        ...current,
        items: current.items.map((item: any) =>
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
    onSuccess: (_d, v) => invalidateWorkflow(qc, v.workflowId),
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
    onSuccess: (_d, v) => invalidateWorkflow(qc, v.workflowId),
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
    onSuccess: (_d, v) => invalidateWorkflow(qc, v.workflowId),
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
    onSuccess: (_d, v) => invalidateWorkflow(qc, v.workflowId),
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
    onSuccess: (_d, v) => invalidateWorkflow(qc, v.workflowId),
  })
}

export function useScheduleClosing() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ workflowId, form }: { workflowId: string; form: ScheduleClosingForm }) =>
      api.scheduleClosing({
        workflowId,
        scheduleClosingRequest: { scheduledDate: form.scheduledDate },
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
          closingNotes: form.closingNotes || undefined,
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
        failPurchaseRequest: { reason: form.reason },
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
        withdrawPurchaseRequest: { reason: form.reason },
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
          reason: form.reason,
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
