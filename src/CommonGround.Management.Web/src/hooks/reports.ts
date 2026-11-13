import { useQuery } from '@tanstack/react-query'
import { ReportsApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { CalendarDate } from '../lib/calendarDate'

const api = new ReportsApi(apiConfig)

/** The funder report for a period. Doesn't load while `enabled` is false, such as when To is before From. */
export function useFunderReport(from: CalendarDate, to: CalendarDate, enabled: boolean) {
  return useQuery({
    queryKey: ['reports', 'funder', from, to],
    queryFn: () => api.getFunderReport({ from, to }),
    enabled,
  })
}
