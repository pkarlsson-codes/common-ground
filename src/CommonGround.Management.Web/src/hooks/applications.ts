import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  HomeownerApplicationsApi,
  HouseholdApi,
  type ApproveApplicationRequest,
  type DenyApplicationRequest,
  type HomeownerApplicationResponse,
  type PaginatedResponseOfHomeownerApplicationListItemResponse,
  type PaginatedResponseOfWaitingListEntryResponse,
  type SelectApplicantRequest,
  type RegisterHouseholdRequest,
  type RegisterHouseholdResponse,
  type SubmitApplicationResponse,
  type WithdrawApplicationRequest,
} from '../api'
import { ResponseError } from '../api/runtime'
import { apiConfig } from '../lib/apiConfig'

const api = new HomeownerApplicationsApi(apiConfig)
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

/**
 * The waiting list of approved applications, longest waiting first. Keyed under
 * 'homeowner-applications' so application and household changes refresh it.
 */
export function useWaitingList(params: { pageNumber: number; pageSize?: number; search?: string }) {
  return useQuery<PaginatedResponseOfWaitingListEntryResponse>({
    queryKey: ['homeowner-applications', 'waiting-list', params.pageNumber, params.pageSize, params.search],
    queryFn: () =>
      api.getWaitingList({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize ?? 20,
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
      qc.invalidateQueries({ queryKey: ['households'] })
    },
  })
}

export function useApproveApplication() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ applicationId, request }: { applicationId: string; request: ApproveApplicationRequest }) =>
      api.approveApplication({ applicationId, approveApplicationRequest: request }),
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
