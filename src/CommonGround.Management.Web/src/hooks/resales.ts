import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ResalesApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import { daysBetween, trustToday, type CalendarDate } from '../lib/calendarDate'

const api = new ResalesApi(apiConfig)

export const NOTICE_CHANNELS = ['Phone', 'Email', 'Letter', 'InPerson'] as const
export type NoticeChannel = typeof NOTICE_CHANNELS[number]

export const NOTICE_CHANNEL_LABELS: Record<string, string> = {
  Phone: 'Phone',
  Email: 'Email',
  Letter: 'Letter',
  InPerson: 'In Person',
}

/** "N days left", or "Window passed" once the option window's last day is behind. */
export function optionWindowLeft(endsOn: CalendarDate) {
  const left = daysBetween(trustToday(), endsOn)
  if (left < 0) return 'Window passed'
  return left === 1 ? '1 day left' : `${left} days left`
}

export interface RecordIntentToSellForm {
  noticeDate: CalendarDate
  channel: NoticeChannel
  notes: string
}

export interface WithdrawIntentToSellForm {
  withdrawnDate: CalendarDate
  reason: string
}

/** Resales in progress, most recent notice first. */
export function useOpenResales(pageNumber: number) {
  return useQuery({
    queryKey: ['resales', 'open', pageNumber],
    queryFn: () => api.getResales({ pageNumber, pageSize: 20, status: 'Open' }),
  })
}

/** The resale in progress on a lease, or null when there is none. */
export function useOpenResaleForLease(groundLeaseId: string) {
  return useQuery({
    queryKey: ['resales', 'lease', groundLeaseId],
    queryFn: async () => (await api.getResales({ pageNumber: 1, pageSize: 1, status: 'Open', groundLeaseId })).items?.[0] ?? null,
  })
}

export function useRecordIntentToSell() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, form }: { groundLeaseId: string; form: RecordIntentToSellForm }) =>
      api.recordIntentToSell({
        groundLeaseId,
        recordIntentToSellRequest: {
          noticeDate: form.noticeDate,
          channel: form.channel,
          notes: form.notes || undefined,
        },
      }),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['resales'] })
      void qc.invalidateQueries({ queryKey: ['homes'] })
      void qc.invalidateQueries({ queryKey: ['home'] })
    },
  })
}

export function useWithdrawIntentToSell() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ resaleId, form }: { resaleId: string; form: WithdrawIntentToSellForm }) =>
      api.withdrawIntentToSell({
        resaleId,
        withdrawIntentToSellRequest: { withdrawnDate: form.withdrawnDate, reason: form.reason },
      }),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['resales'] })
      void qc.invalidateQueries({ queryKey: ['homes'] })
      void qc.invalidateQueries({ queryKey: ['home'] })
    },
  })
}
