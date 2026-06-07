import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  ApplicantSelectionApi,
  type ApproveEligibilityRequest,
  type DenyApplicationRequest,
  type DenyEligibilityRequest,
  type HomeownerApplicationResponse,
  type PaginatedResponseOfHomeownerApplicationListItemResponse,
  type RecordCltOrientationRequest,
  type RecordHousingCounselingRequest,
  type RecordMortgagePrequalificationRequest,
  type SelectApplicantRequest,
  type SubmitApplicationRequest,
  type SubmitApplicationResponse,
  type WithdrawApplicationRequest,
} from '../api'
import { ResponseError } from '../api/runtime'
import { apiConfig } from '../lib/apiConfig'

const api = new ApplicantSelectionApi(apiConfig)

export type HomeownerApplicationsQueryParams = {
  pageNumber: number
  pageSize?: number
  phase?: string
  search?: string
}

const invalidateApplication = (qc: ReturnType<typeof useQueryClient>, applicationId: string) => {
  qc.invalidateQueries({ queryKey: ['homeowner-application', applicationId] })
  qc.invalidateQueries({ queryKey: ['homeowner-applications'] })
}

const shouldRetryQuery = (failureCount: number, error: unknown): boolean => {
  if (error instanceof ResponseError) {
    const status = error.response.status
    // Avoid retry loops for request validation/conflict/not-found failures.
    if (status >= 400 && status < 500) {
      return false
    }
  }

  return failureCount < 2
}

export function useHomeownerApplications(params: HomeownerApplicationsQueryParams) {
  return useQuery<PaginatedResponseOfHomeownerApplicationListItemResponse>({
    queryKey: ['homeowner-applications', params.pageNumber, params.pageSize, params.phase, params.search],
    queryFn: () =>
      api.getHomeownerApplications({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize ?? 20,
        phase: params.phase,
        search: params.search,
      }),
    retry: shouldRetryQuery,
  })
}

export function useHomeownerApplication(applicationId: string | null) {
  return useQuery<HomeownerApplicationResponse>({
    queryKey: ['homeowner-application', applicationId],
    queryFn: () => api.getHomeownerApplication({ applicationId: applicationId! }),
    enabled: !!applicationId,
    retry: shouldRetryQuery,
  })
}

export function useSubmitApplication() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (submitApplicationRequest: SubmitApplicationRequest) =>
      api.submitApplication({ submitApplicationRequest }) as Promise<SubmitApplicationResponse>,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['homeowner-applications'] })
    },
  })
}

export function useApproveEligibility() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ applicationId, request }: { applicationId: string; request: ApproveEligibilityRequest }) =>
      api.approveEligibility({ applicationId, approveEligibilityRequest: request }),
    onSuccess: (_data, variables) => {
      invalidateApplication(qc, variables.applicationId)
    },
  })
}

export function useDenyEligibility() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ applicationId, request }: { applicationId: string; request: DenyEligibilityRequest }) =>
      api.denyEligibility({ applicationId, denyEligibilityRequest: request }),
    onSuccess: (_data, variables) => {
      invalidateApplication(qc, variables.applicationId)
    },
  })
}

export function useRecordHousingCounseling() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ applicationId, request }: { applicationId: string; request: RecordHousingCounselingRequest }) =>
      api.recordHousingCounseling({ applicationId, recordHousingCounselingRequest: request }),
    onSuccess: (_data, variables) => {
      invalidateApplication(qc, variables.applicationId)
    },
  })
}

export function useRecordCltOrientation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ applicationId, request }: { applicationId: string; request: RecordCltOrientationRequest }) =>
      api.recordCltOrientation({ applicationId, recordCltOrientationRequest: request }),
    onSuccess: (_data, variables) => {
      invalidateApplication(qc, variables.applicationId)
    },
  })
}

export function useRecordMortgagePrequalification() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ applicationId, request }: { applicationId: string; request: RecordMortgagePrequalificationRequest }) =>
      api.recordMortgagePrequalification({ applicationId, recordMortgagePrequalificationRequest: request }),
    onSuccess: (_data, variables) => {
      invalidateApplication(qc, variables.applicationId)
    },
  })
}

export function useSelectApplicant() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ applicationId, request }: { applicationId: string; request: SelectApplicantRequest }) =>
      api.selectApplicant({ applicationId, selectApplicantRequest: request }),
    onSuccess: (_data, variables) => {
      invalidateApplication(qc, variables.applicationId)
    },
  })
}

export function useDenyApplication() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ applicationId, request }: { applicationId: string; request: DenyApplicationRequest }) =>
      api.denyApplication({ applicationId, denyApplicationRequest: request }),
    onSuccess: (_data, variables) => {
      invalidateApplication(qc, variables.applicationId)
    },
  })
}

export function useWithdrawApplication() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ applicationId, request }: { applicationId: string; request: WithdrawApplicationRequest }) =>
      api.withdrawApplication({ applicationId, withdrawApplicationRequest: request }),
    onSuccess: (_data, variables) => {
      invalidateApplication(qc, variables.applicationId)
    },
  })
}
