import { useQuery } from '@tanstack/react-query'
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
