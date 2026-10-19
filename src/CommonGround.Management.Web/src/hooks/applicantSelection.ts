import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  ApplicantSelectionApi,
  HouseholdApi,
  type ApproveEligibilityRequest,
  type DenyApplicationRequest,
  type HomeownerApplicationResponse,
  type HouseholdResponse,
  type PaginatedResponseOfHomeownerApplicationListItemResponse,
  type RecordCltOrientationRequest,
  type RecordHousingCounselingRequest,
  type RecordMortgagePrequalificationRequest,
  type SelectApplicantRequest,
  type RegisterHouseholdRequest,
  type RegisterHouseholdResponse,
  type SubmitApplicationResponse,
  type WithdrawApplicationRequest,
} from '../api'
import { ResponseError } from '../api/runtime'
import { apiConfig } from '../lib/apiConfig'

const api = new ApplicantSelectionApi(apiConfig)
const householdApi = new HouseholdApi(apiConfig)

export type HomeownerApplicationsQueryParams = {
  pageNumber: number
  pageSize?: number
  phase?: string
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
    queryKey: ['homeowner-applications', params.pageNumber, params.pageSize, params.phase],
    queryFn: () =>
      api.getHomeownerApplications({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize ?? 20,
        phase: params.phase,
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

export function useHousehold(householdId: string | null | undefined) {
  return useQuery<HouseholdResponse>({
    queryKey: ['household', householdId],
    queryFn: () => householdApi.getHousehold({ householdId: householdId! }),
    enabled: !!householdId,
    retry: shouldRetryQuery,
  })
}

/** Registers a household, then submits an application for it. */
export function useSubmitApplication() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (registerHouseholdRequest: RegisterHouseholdRequest) => {
      const household = await householdApi.registerHousehold({ registerHouseholdRequest }) as RegisterHouseholdResponse
      return await api.submitApplication({
        submitApplicationRequest: { householdId: household.householdId },
      }) as SubmitApplicationResponse
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['homeowner-applications'] })
    },
  })
}

export function useApproveApplication() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ applicationId, request }: { applicationId: string; request: ApproveEligibilityRequest }) =>
      api.approveApplication({ applicationId, approveEligibilityRequest: request }),
    onSuccess: (_data, variables) => {
      invalidateApplication(qc, variables.applicationId)
    },
  })
}

export function useRecordHousingCounseling() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ householdId, request }: { householdId: string; request: RecordHousingCounselingRequest }) =>
      householdApi.recordHousingCounseling({ householdId, recordHousingCounselingRequest: request }),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['household', variables.householdId] })
    },
  })
}

export function useRecordCltOrientation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ householdId, request }: { householdId: string; request: RecordCltOrientationRequest }) =>
      householdApi.recordCltOrientation({ householdId, recordCltOrientationRequest: request }),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['household', variables.householdId] })
    },
  })
}

export function useRecordMortgagePrequalification() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ householdId, request }: { householdId: string; request: RecordMortgagePrequalificationRequest }) =>
      householdApi.recordMortgagePrequalification({ householdId, recordMortgagePrequalificationRequest: request }),
    onSuccess: (_data, variables) => {
      qc.invalidateQueries({ queryKey: ['household', variables.householdId] })
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
