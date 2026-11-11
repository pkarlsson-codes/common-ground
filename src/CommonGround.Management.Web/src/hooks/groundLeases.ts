import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { GroundLeasesApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import { startOfMonth, trustToday, type CalendarDate } from '../lib/calendarDate'
import type {
  CreateGroundLeaseForm,
  RecordCheckInForm,
} from './types'

const api = new GroundLeasesApi(apiConfig)

export function useGroundLeases(pageNumber: number, status?: string, landParcelId?: string, homeId?: string, search?: string) {
  return useQuery({
    queryKey: ['ground-leases', pageNumber, status, landParcelId, homeId, search],
    queryFn: () => api.getGroundLeases({ pageNumber, pageSize: 20, statuses: status ? [status] : undefined, landParcelId, homeId, search }),
  })
}

/** The ground leases on a home or a land parcel, newest first. */
export function useConnectedGroundLeases(filter: { homeId?: string; landParcelId?: string }) {
  return useQuery({
    queryKey: ['ground-leases', 'connected', filter.homeId, filter.landParcelId],
    queryFn: () => api.getGroundLeases({ pageNumber: 1, pageSize: 50, homeId: filter.homeId, landParcelId: filter.landParcelId }),
    enabled: !!(filter.homeId || filter.landParcelId),
  })
}

export function useAllActiveGroundLeases() {
  return useQuery({
    queryKey: ['ground-leases', 'all-active'],
    queryFn: () => api.getGroundLeases({ pageSize: 1000, statuses: ['Active'] }),
  })
}

export function useGroundLease(id: string | null) {
  return useQuery({
    queryKey: ['ground-lease', id],
    queryFn: () => api.getGroundLease({ groundLeaseId: id! }),
    enabled: !!id,
  })
}

/** A lease that started before this month asks where fee tracking begins, so its past months aren't charged. */
export const tracksFeesFromLater = (form: Pick<CreateGroundLeaseForm, 'leaseStartDate'>) =>
  form.leaseStartDate < startOfMonth(trustToday())

export function useCreateGroundLease() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (form: CreateGroundLeaseForm) =>
      api.createGroundLease({
        createGroundLeaseRequest: {
          landParcelId: form.landParcelId,
          homeId: form.homeId,
          homeownerFullName: form.homeownerFullName,
          homeownerEmail: form.homeownerEmail,
          homeownerPhone: form.homeownerPhone || undefined,
          leaseStartDate: form.leaseStartDate,
          termYears: form.termYears,
          optionWindowDays: form.optionWindowDays,
          landUseFee: form.landUseFee,
          repairReserveFee: form.repairReserveFee,
          lateInterestRate: form.lateInterestRate,
          interestWaiverDays: form.interestWaiverDays,
          ...(tracksFeesFromLater(form) && { feesTrackedFrom: form.feesTrackedFrom, openingBalance: form.openingBalance }),
          basePrice: form.basePrice,
          originalAppraisedValue: form.originalAppraisedValue,
          covenantTerms: form.covenantTerms || undefined,
          formulaKind: form.formulaKind,
          appreciationShare: form.formulaKind === 'AppraisalBased' ? form.appreciationShare : undefined,
          indexType: form.formulaKind === 'IndexBased' ? form.indexType : undefined,
          baselineIndexValue: form.formulaKind === 'IndexBased' ? form.baselineIndexValue : undefined,
          annualPercentage: form.formulaKind === 'FixedPercentage' ? form.annualPercentage : undefined,
          improvementsCap: form.hasImprovementsCap ? form.improvementsCap : undefined,
        },
      }),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['ground-leases'] }),
  })
}

export function useRecordStewardshipCheckIn() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, form }: { groundLeaseId: string; form: RecordCheckInForm }) =>
      api.recordStewardshipCheckIn({
        groundLeaseId,
        recordStewardshipCheckInRequest: {
          checkInDate: form.checkInDate,
          notes: form.notes || undefined,
        },
      }),
    onSuccess: (_d, v) => qc.invalidateQueries({ queryKey: ['ground-lease', v.groundLeaseId] }),
  })
}

export const PAYMENT_METHODS = ['Check', 'BankTransfer', 'Cash', 'MoneyOrder', 'Card', 'Other'] as const
export type PaymentMethod = typeof PAYMENT_METHODS[number]

export const PAYMENT_METHOD_LABELS: Record<string, string> = {
  Check: 'Check',
  BankTransfer: 'Bank Transfer',
  Cash: 'Cash',
  MoneyOrder: 'Money Order',
  Card: 'Card',
  Other: 'Other',
}

export interface RecordLeaseFeePaymentForm {
  receivedDate: CalendarDate
  amount: number
  method: PaymentMethod
  reference: string
}

/** Leases with what they owe in lease fees, most days overdue first. */
/** Which leases the Lease Fees tab lists. */
export type LeaseFeeFilter = 'due' | 'notice' | 'all'

export function useLeaseFeeBalances(pageNumber: number, filter: LeaseFeeFilter, search?: string) {
  return useQuery({
    queryKey: ['lease-fee-balances', pageNumber, filter, search],
    queryFn: () => api.getLeaseFeeBalances({
      pageNumber,
      pageSize: 20,
      includeAll: filter === 'all',
      noticeDueOnly: filter === 'notice',
      search: search || undefined,
    }),
  })
}

export const NOTICE_LEVELS = ['First', 'Second', 'Default'] as const
export type NoticeLevel = typeof NOTICE_LEVELS[number]

export const NOTICE_LEVEL_LABELS: Record<string, string> = {
  First: 'First Notice',
  Second: 'Second Notice',
  Default: 'Notice of Default',
}

export const NOTICE_DELIVERIES = ['FirstClassMail', 'CertifiedMail', 'RegisteredMail', 'HandDelivered', 'Email'] as const
export type NoticeDelivery = typeof NOTICE_DELIVERIES[number]

export const NOTICE_DELIVERY_LABELS: Record<string, string> = {
  FirstClassMail: 'First-Class Mail',
  CertifiedMail: 'Certified Mail',
  RegisteredMail: 'Registered Mail',
  HandDelivered: 'Hand Delivered',
  Email: 'Email',
}

/** The deliveries a notice of default may use: the lease's notice clause (Art 14.2) asks for proof it arrived. */
export const DEFAULT_NOTICE_DELIVERIES: readonly NoticeDelivery[] = ['CertifiedMail', 'RegisteredMail', 'HandDelivered']

/** The deliveries a notice of the given level may use. */
export const deliveriesFor = (level: string): readonly NoticeDelivery[] =>
  level === 'Default' ? DEFAULT_NOTICE_DELIVERIES : NOTICE_DELIVERIES

/** What a notice of default's cure status means, for a lease's Lease Fees section. */
export const DEFAULT_CURE_STATUS_LABELS: Record<string, string> = {
  Curing: 'Cure period running',
  Cured: 'Cured',
  PaidAfterDeadline: 'Paid after the deadline',
  Uncured: 'Not cured',
}

export interface RecordLeaseFeeNoticeForm {
  level: NoticeLevel
  sentOn: CalendarDate
  delivery: NoticeDelivery
}

/** Notices due, for printing: every lease's, or one lease's. */
export function useLeaseFeeNoticesDue(groundLeaseId?: string) {
  return useQuery({
    queryKey: ['lease-fee-notices-due', groundLeaseId ?? null],
    queryFn: () => api.getLeaseFeeNoticesDue({ groundLeaseId }),
  })
}

/** Invalidates everything a lease fee change can move: the lease, the balances list, the notices due and Today. */
function invalidateLeaseFees(qc: ReturnType<typeof useQueryClient>, groundLeaseId: string) {
  qc.invalidateQueries({ queryKey: ['ground-lease', groundLeaseId] })
  qc.invalidateQueries({ queryKey: ['lease-fee-balances'] })
  qc.invalidateQueries({ queryKey: ['lease-fee-notices-due'] })
  qc.invalidateQueries({ queryKey: ['today'] })
}

export function useRecordLeaseFeeNotice() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, form }: { groundLeaseId: string; form: RecordLeaseFeeNoticeForm }) =>
      api.recordLeaseFeeNotice({
        groundLeaseId,
        recordLeaseFeeNoticeRequest: { level: form.level, sentOn: form.sentOn, delivery: form.delivery },
      }),
    onSuccess: (_d, v) => invalidateLeaseFees(qc, v.groundLeaseId),
  })
}

export function useWaiveLeaseFeeInterest() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, amount, reason }: { groundLeaseId: string; amount: number; reason: string }) =>
      api.waiveLeaseFeeInterest({ groundLeaseId, waiveLeaseFeeInterestRequest: { amount, reason } }),
    onSuccess: (_d, v) => invalidateLeaseFees(qc, v.groundLeaseId),
  })
}

export function useCorrectLeaseFeeTracking() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, feesTrackedFrom, openingBalance }: { groundLeaseId: string; feesTrackedFrom: CalendarDate; openingBalance: number }) =>
      api.correctLeaseFeeTracking({ groundLeaseId, correctLeaseFeeTrackingRequest: { feesTrackedFrom, openingBalance } }),
    onSuccess: (_d, v) => invalidateLeaseFees(qc, v.groundLeaseId),
  })
}

export function useLeaseFeeNoticePolicy() {
  return useQuery({
    queryKey: ['lease-fee-notice-policy'],
    queryFn: () => api.getLeaseFeeNoticePolicy(),
  })
}

export interface LeaseFeeNoticePolicyForm {
  firstNoticeDays: number
  secondNoticeDays: number
  defaultNoticeDays: number
  cureDays: number
  cureExtensionDays: number
  letterSignOff: string
}

export function useSetLeaseFeeNoticePolicy() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (form: LeaseFeeNoticePolicyForm) =>
      api.setLeaseFeeNoticePolicy({ setLeaseFeeNoticePolicyRequest: form }),
    onSuccess: data => {
      qc.setQueryData(['lease-fee-notice-policy'], data)
      qc.invalidateQueries({ queryKey: ['lease-fee-balances'] })
      qc.invalidateQueries({ queryKey: ['lease-fee-notices-due'] })
      qc.invalidateQueries({ queryKey: ['ground-lease'] })
      qc.invalidateQueries({ queryKey: ['today'] })
    },
  })
}

export function useRecordLeaseFeePayment() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, form }: { groundLeaseId: string; form: RecordLeaseFeePaymentForm }) =>
      api.recordLeaseFeePayment({
        groundLeaseId,
        recordLeaseFeePaymentRequest: {
          receivedDate: form.receivedDate,
          amount: form.amount,
          method: form.method,
          reference: form.reference.trim() || undefined,
        },
      }),
    onSuccess: (_d, v) => invalidateLeaseFees(qc, v.groundLeaseId),
  })
}

export function useVoidLeaseFeePayment() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, paymentId, reason }: { groundLeaseId: string; paymentId: string; reason: string }) =>
      api.voidLeaseFeePayment({ groundLeaseId, paymentId, voidLeaseFeePaymentRequest: { reason } }),
    onSuccess: (_d, v) => invalidateLeaseFees(qc, v.groundLeaseId),
  })
}
