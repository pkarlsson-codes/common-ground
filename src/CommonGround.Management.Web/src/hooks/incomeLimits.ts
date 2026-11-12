import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { IncomeLimitSchedulesApi, type IncomeLimitScheduleRequest, type IncomeLimitScheduleResponse } from '../api'
import { apiConfig } from '../lib/apiConfig'

const api = new IncomeLimitSchedulesApi(apiConfig)

/** The number of household sizes a published schedule prints a limit for. */
export const PUBLISHED_HOUSEHOLD_SIZES = 8

export function useIncomeLimitSchedules() {
  return useQuery({
    queryKey: ['income-limit-schedules'],
    queryFn: () => api.getIncomeLimitSchedules(),
  })
}

export function useEnterIncomeLimitSchedule() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (request: IncomeLimitScheduleRequest) =>
      api.enterIncomeLimitSchedule({ incomeLimitScheduleRequest: request }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['income-limit-schedules'] }),
  })
}

export function useCorrectIncomeLimitSchedule() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ scheduleId, request }: { scheduleId: string; request: IncomeLimitScheduleRequest }) =>
      api.correctIncomeLimitSchedule({ scheduleId, incomeLimitScheduleRequest: request }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['income-limit-schedules'] }),
  })
}

/** The schedule's levels as "50%, 80%, 120% of AMI". */
export const describeLevels = (schedule: IncomeLimitScheduleResponse) => {
  const percents = (schedule.levels ?? []).map(l => `${l.percentOfAmi}%`)
  return percents.length > 0 ? `${percents.join(', ')} of AMI` : '—'
}
