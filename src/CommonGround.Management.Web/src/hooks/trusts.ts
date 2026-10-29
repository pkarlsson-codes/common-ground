import { useQuery } from '@tanstack/react-query'
import { TrustsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new TrustsApi(apiConfig)

export function useCurrentTrust() {
  return useQuery({
    queryKey: ['trusts', 'current'],
    queryFn: () => api.getCurrentTrust(),
    staleTime: Infinity,
  })
}
