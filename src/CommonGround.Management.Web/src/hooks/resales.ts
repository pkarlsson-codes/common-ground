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

export interface RecordResaleAppraisalForm {
  appraisedValue: number
  appraisalDate: CalendarDate
  appraiserName: string
  approvedImprovements: number
  currentIndexValue: number
}

export interface ExercisePurchaseOptionForm {
  exercisedDate: CalendarDate
  notes: string
}

export interface RecordBuybackClosedForm {
  closingDate: CalendarDate
  pricePaid: number
  notes: string
  offerForSale: boolean
  holdReason: string
  feesPaidFromProceeds: number
}

/** The step a resale in progress is on, as the Resales page and the detail panel's tracker name it. */
export function resaleStep(resale: { status?: string; appraisedValue?: number | null; optionExercisedDate?: unknown }) {
  if (resale.status === 'Closed') return 'Closed'
  if (resale.optionExercisedDate) return 'Option Exercised'
  if (resale.appraisedValue != null) return 'Appraised'
  return 'Notice'
}

/** One resale, with its appraisal, decision and closing. */
export function useResale(id: string | null) {
  return useQuery({
    queryKey: ['resales', 'detail', id],
    queryFn: () => api.getResale({ resaleId: id! }),
    enabled: !!id,
  })
}

/** The prices an appraisal would set, worked out by the server from the resale's lease. */
export function usePreviewResalePrice(
  resaleId: string,
  figures: { appraisedValue: number; appraisalDate: CalendarDate; approvedImprovements?: number; currentIndexValue?: number },
  enabled: boolean,
) {
  return useQuery({
    queryKey: ['resales', 'price-preview', resaleId, figures],
    queryFn: () => api.previewResalePrice({ resaleId, previewResalePriceRequest: figures }),
    enabled,
    retry: false,
  })
}

/** What the seller owes the trust on a day, worked out by the server from the lease's fee ledger. */
export function usePayoffStatement(resaleId: string | null, goodThrough: CalendarDate | null, enabled: boolean) {
  return useQuery({
    queryKey: ['resales', 'payoff-statement', resaleId, goodThrough],
    queryFn: () => api.getPayoffStatement({ resaleId: resaleId!, goodThrough: goodThrough! }),
    enabled: enabled && !!resaleId && !!goodThrough,
    retry: false,
  })
}

/** The lease's closed resale, once the trust has bought the home back. */
export function useClosedResaleForLease(groundLeaseId: string, enabled: boolean) {
  return useQuery({
    queryKey: ['resales', 'closed-lease', groundLeaseId],
    queryFn: async () => (await api.getResales({ pageNumber: 1, pageSize: 1, status: 'Closed', groundLeaseId })).items?.[0] ?? null,
    enabled,
  })
}

const refreshResales = (qc: ReturnType<typeof useQueryClient>) => {
  void qc.invalidateQueries({ queryKey: ['resales'] })
  void qc.invalidateQueries({ queryKey: ['homes'] })
  void qc.invalidateQueries({ queryKey: ['home'] })
  void qc.invalidateQueries({ queryKey: ['ground-leases'] })
  void qc.invalidateQueries({ queryKey: ['ground-lease'] })
}

export function useRecordResaleAppraisal() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ resaleId, form, formulaKind, creditsImprovements }: {
      resaleId: string
      form: RecordResaleAppraisalForm
      formulaKind: string
      creditsImprovements: boolean
    }) =>
      api.recordResaleAppraisal({
        resaleId,
        recordResaleAppraisalRequest: {
          appraisedValue: form.appraisedValue,
          appraisalDate: form.appraisalDate,
          appraiserName: form.appraiserName,
          approvedImprovements: creditsImprovements ? form.approvedImprovements : undefined,
          currentIndexValue: formulaKind === 'IndexBased' ? form.currentIndexValue : undefined,
        },
      }),
    onSuccess: () => refreshResales(qc),
  })
}

export function useExercisePurchaseOption() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ resaleId, form }: { resaleId: string; form: ExercisePurchaseOptionForm }) =>
      api.exercisePurchaseOption({
        resaleId,
        exercisePurchaseOptionRequest: { exercisedDate: form.exercisedDate, notes: form.notes || undefined },
      }),
    onSuccess: () => refreshResales(qc),
  })
}

export function useRecordPayoffStatement() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ resaleId, goodThrough }: { resaleId: string; goodThrough: CalendarDate }) =>
      api.recordPayoffStatement({ resaleId, recordPayoffStatementRequest: { goodThrough } }),
    onSuccess: () => void qc.invalidateQueries({ queryKey: ['resales'] }),
  })
}

export function useRecordBuybackClosed() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ resaleId, form }: { resaleId: string; form: RecordBuybackClosedForm }) =>
      api.recordBuybackClosed({
        resaleId,
        recordBuybackClosedRequest: {
          closingDate: form.closingDate,
          pricePaid: form.pricePaid,
          notes: form.notes || undefined,
          offerForSale: form.offerForSale,
          holdReason: form.offerForSale ? undefined : form.holdReason,
          feesPaidFromProceeds: form.feesPaidFromProceeds > 0 ? form.feesPaidFromProceeds : undefined,
        },
      }),
    onSuccess: () => refreshResales(qc),
  })
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
    enabled: !!groundLeaseId,
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
