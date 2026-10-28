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

const OPEN_PHASES = ['Submitted', 'Approved']

/** The household's open (Submitted or Approved) application, if any. */
export const findOpenApplication = (household?: HouseholdResponse) =>
  household?.applications?.find(a => OPEN_PHASES.includes(a.phase ?? ''))

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

export type HouseholdMemberInput = { name: string; email: string; phone: string }

function useHouseholdMutation<TVars>(householdId: string, mutationFn: (vars: TVars) => Promise<unknown>) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['household', householdId] })
      qc.invalidateQueries({ queryKey: ['households'] })
      // Applications show the primary contact's current name and email.
      qc.invalidateQueries({ queryKey: ['homeowner-applications'] })
      qc.invalidateQueries({ queryKey: ['homeowner-application'] })
    },
  })
}

export function useAddHouseholdMember(householdId: string) {
  return useHouseholdMutation(householdId, (member: HouseholdMemberInput) =>
    api.addHouseholdMember({ householdId, addHouseholdMemberRequest: member }))
}

export function useUpdateHouseholdMember(householdId: string) {
  return useHouseholdMutation(householdId, ({ memberId, ...member }: HouseholdMemberInput & { memberId: string }) =>
    api.updateHouseholdMember({ householdId, memberId, updateHouseholdMemberRequest: member }))
}

export function useRemoveHouseholdMember(householdId: string) {
  return useHouseholdMutation(householdId, (memberId: string) =>
    api.removeHouseholdMember({ householdId, memberId }))
}

export function useUpdateHouseholdCircumstances(householdId: string) {
  return useHouseholdMutation(householdId, (req: { annualIncome: number; requiresAccessibility: boolean }) =>
    api.updateHouseholdCircumstances({ householdId, updateHouseholdCircumstancesRequest: req }))
}
