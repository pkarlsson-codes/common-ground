import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  HomeownerApplicationsApi,
  HouseholdApi,
  type HouseholdResponse,
  type PaginatedResponseOfHouseholdListItemResponse,
  type SubmitApplicationResponse,
} from '../api'
import { ResponseError } from '../api/runtime'
import { apiConfig } from '../lib/apiConfig'

const api = new HouseholdApi(apiConfig)
const applicationsApi = new HomeownerApplicationsApi(apiConfig)

export type HouseholdsQueryParams = {
  pageNumber: number
  pageSize?: number
  search?: string
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

export function useHouseholds(params: HouseholdsQueryParams) {
  return useQuery<PaginatedResponseOfHouseholdListItemResponse>({
    queryKey: ['households', params.pageNumber, params.pageSize, params.search],
    queryFn: () =>
      api.getHouseholds({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize ?? 20,
        search: params.search,
      }),
    retry: shouldRetryQuery,
  })
}

export function useHousehold(householdId: string | null | undefined) {
  return useQuery<HouseholdResponse>({
    queryKey: ['household', householdId],
    queryFn: () => api.getHousehold({ householdId: householdId! }),
    enabled: !!householdId,
    retry: shouldRetryQuery,
  })
}

/** Submits a new application for an existing household. */
export function useStartApplication() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (householdId: string) =>
      applicationsApi.submitApplication({
        submitApplicationRequest: { householdId },
      }) as Promise<SubmitApplicationResponse>,
    onSuccess: (_data, householdId) => {
      qc.invalidateQueries({ queryKey: ['household', householdId] })
      qc.invalidateQueries({ queryKey: ['homeowner-applications'] })
    },
  })
}
