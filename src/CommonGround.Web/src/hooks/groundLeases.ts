import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { GroundLeasesApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type {
  CreateGroundLeaseForm,
  OnboardHomeownerForm,
  RecordCheckInForm,
  ResaleEstimateForm,
} from './types'

const api = new GroundLeasesApi(apiConfig)

export function useGroundLeases(page: number, status?: string, landParcelId?: string, buildingId?: string, search?: string) {
  return useQuery({
    queryKey: ['ground-leases', page, status, landParcelId, buildingId, search],
    queryFn: () => api.getGroundLeases({ page, pageSize: 20, status, landParcelId, buildingId, search }),
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
          buildingId: form.buildingId,
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

export function useOnboardHomeowner() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, form }: { groundLeaseId: string; form: OnboardHomeownerForm }) =>
      api.onboardHomeowner({
        groundLeaseId,
        onboardHomeownerRequest: {
          fullName: form.fullName,
          email: form.email,
          phone: form.phone || undefined,
        },
      }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: ['ground-lease', v.groundLeaseId] })
      qc.invalidateQueries({ queryKey: ['ground-leases'] })
    },
  })
}

export function useChangeHomeowner() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, form }: { groundLeaseId: string; form: OnboardHomeownerForm }) =>
      api.changeHomeowner({
        groundLeaseId,
        changeHomeownerRequest: {
          fullName: form.fullName,
          email: form.email,
          phone: form.phone || undefined,
        },
      }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: ['ground-lease', v.groundLeaseId] })
      qc.invalidateQueries({ queryKey: ['ground-leases'] })
    },
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
