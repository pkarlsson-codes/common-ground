import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { DemoApi } from '../api'
import { ResponseError } from '../api/runtime'
import { apiConfig } from '../lib/apiConfig'

const api = new DemoApi(apiConfig)

/** The demo banner for the current trust, or null when the server is not in demo mode. */
export function useDemo() {
  return useQuery({
    queryKey: ['demo'],
    queryFn: async () => {
      try {
        return await api.getDemo()
      } catch (error) {
        if (error instanceof ResponseError && error.response.status === 404) return null
        throw error
      }
    },
    staleTime: Infinity,
    retry: false,
  })
}

/**
 * The signed-in user's latest sample trust, polled while it is being filled.
 * Data is undefined while loading, null when the server is not in demo mode,
 * and otherwise the current response (its sandbox is null when the user has none).
 */
export function useDemoSandbox() {
  return useQuery({
    queryKey: ['demo', 'sandbox'],
    queryFn: async () => {
      try {
        return await api.getCurrentSandbox()
      } catch (error) {
        if (error instanceof ResponseError && error.response.status === 404) return null
        throw error
      }
    },
    refetchInterval: (query) => (query.state.data?.sandbox?.status === 'Seeding' ? 2000 : false),
    retry: false,
  })
}

/** Starts filling a sample trust for the signed-in user. */
export function useStartDemoSandbox() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => api.startSandbox(),
    onSuccess: (sandbox) => queryClient.setQueryData(['demo', 'sandbox'], { sandbox }),
  })
}

/**
 * Whether this server is a demo server, asked without a session for the signed-out Welcome page.
 * Data is null when demo mode is off. Only fetched while `enabled`.
 */
export function useDemoMode(enabled: boolean) {
  return useQuery({
    queryKey: ['demo', 'mode'],
    queryFn: async () => {
      try {
        return await api.getMode()
      } catch (error) {
        if (error instanceof ResponseError && error.response.status === 404) return null
        throw error
      }
    },
    enabled,
    staleTime: Infinity,
    retry: false,
  })
}
