import { useQuery } from '@tanstack/react-query'
import { TodayApi } from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new TodayApi(apiConfig)

/** Everything waiting on staff today, across every area; the page and the sidebar counts share it. */
export function useToday() {
  return useQuery({
    queryKey: ['today'],
    queryFn: () => api.getToday(),
  })
}
