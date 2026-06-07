import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { GroundLeasesApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type {
  CreateGroundLeaseForm,
  RecordCheckInForm,
  ResaleEstimateForm,
} from './types'

const api = new GroundLeasesApi(apiConfig)

export function useGroundLeases(pageNumber: number, status?: string, landParcelId?: string, homeId?: string, search?: string) {
  return useQuery({
    queryKey: ['ground-leases', pageNumber, status, landParcelId, homeId, search],
    queryFn: () => api.getGroundLeases({ pageNumber, pageSize: 20, statuses: status ? [status] : undefined, landParcelId, homeId, search }),
  })
}

export function useAllActiveGroundLeases() {
  return useQuery({
    queryKey: ['ground-leases', 'all-active'],
    queryFn: () => api.getGroundLeases({ pageSize: 1000, statuses: ['Active'] }),
  })
}

export function useGroundLease(id: string | null) {
  return useQuery({
    queryKey: ['ground-lease', id],
    queryFn: () => api.getGroundLease({ groundLeaseId: id! }),
    enabled: !!id,
  })
}

export function useCreateGroundLease() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (form: CreateGroundLeaseForm) =>
      api.createGroundLease({
        createGroundLeaseRequest: {
          landParcelId: form.landParcelId,
          homeId: form.homeId,
          leaseStartDate: form.leaseStartDate,
          termYears: form.termYears,
          covenantTerms: form.covenantTerms || undefined,
          formulaKind: form.formulaKind,
          appreciationShare: form.formulaKind === 'AppraisalBased' ? form.appreciationShare : undefined,
          indexType: form.formulaKind === 'IndexBased' ? form.indexType : undefined,
          baselineIndexValue: form.formulaKind === 'IndexBased' ? form.baselineIndexValue : undefined,
          annualPercentage: form.formulaKind === 'FixedPercentage' ? form.annualPercentage : undefined,
          improvementsCap: form.hasImprovementsCap ? form.improvementsCap : undefined,
        },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['ground-leases'] }),
  })
}

export function useRecordStewardshipCheckIn() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, form }: { groundLeaseId: string; form: RecordCheckInForm }) =>
      api.recordStewardshipCheckIn({
        groundLeaseId,
        recordStewardshipCheckInRequest: {
          checkInDate: form.checkInDate,
          notes: form.notes || undefined,
        },
      }),
    onSuccess: (_d, v) => qc.invalidateQueries({ queryKey: ['ground-lease', v.groundLeaseId] }),
  })
}

export function useCalculateResaleEstimate() {
  return useMutation({
    mutationFn: ({ groundLeaseId, form }: { groundLeaseId: string; form: ResaleEstimateForm }) =>
      api.calculateResaleEstimate({
        groundLeaseId,
        calculateResaleEstimateRequest: {
          originalPurchasePrice: form.originalPurchasePrice,
          currentAppraisalValue: form.currentAppraisalValue,
          originalAppraisalValue: form.originalAppraisalValue,
          ownershipYears: form.ownershipYears,
          approvedImprovements: form.approvedImprovements || undefined,
          currentIndexValue: form.currentIndexValue || undefined,
        },
      }),
  })
}
