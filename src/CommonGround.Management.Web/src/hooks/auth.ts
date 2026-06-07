import { useQuery, useQueryClient } from '@tanstack/react-query'
import { AuthApi } from '../api'
import { ResponseError } from '../api/runtime'
import { apiConfig } from '../lib/apiConfig'

const api = new AuthApi(apiConfig)

export const currentUserQueryKey = ['auth', 'current-user']

export function useCurrentUser() {
  return useQuery({
    queryKey: currentUserQueryKey,
    queryFn: async () => {
      try {
        const data = await api.getCurrentUser()
        if (data && data.id) {
          return {
            id: data.id,
            username: data.username ?? data.id,
            email: data.email ?? '',
            trustId: data.trustId ?? null,
            hasTrust: data.hasTrust ?? !!data.trustId,
          }
        }
        return null
      } catch (error) {
        if (error instanceof ResponseError && error.response.status === 401) {
          return null
        }
        throw error
      }
    },
    retry: false,
    staleTime: 5 * 60 * 1000,
  })
}

export function useRefreshCurrentUser() {
  const qc = useQueryClient()
  return () => qc.refetchQueries({ queryKey: currentUserQueryKey })
}
