import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { GroundLeasesApi, type MortgageRequestResponse, type ReceiveMortgageRequestResponse } from '../api'
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
          // The input is required, so the form can't submit without it.
          maxMortgageDebtPercent: form.maxMortgageDebtPercent!,
          insuranceReplacementValue: form.insuranceReplacementValue,
          insuranceLiabilityPerOccurrence: form.insuranceLiabilityPerOccurrence,
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

/** How a permitted mortgage was permitted (Art 8), as the lease's Mortgages section shows it. */
export const MORTGAGE_SOURCE_LABELS: Record<string, string> = {
  LeaseSigning: 'Lease signing',
  Request: 'Request',
  Recorded: 'Recorded',
}

export interface RecordPermittedMortgageForm {
  lender: string
  noticeAddress: string
  originalPrincipal: number
  permittedOn: CalendarDate
}

export interface EditPermittedMortgageForm {
  lender: string
  noticeAddress: string
}

function invalidateLease(qc: ReturnType<typeof useQueryClient>, groundLeaseId: string) {
  qc.invalidateQueries({ queryKey: ['ground-lease', groundLeaseId] })
}

export function useRecordPermittedMortgage() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, form }: { groundLeaseId: string; form: RecordPermittedMortgageForm }) =>
      api.recordPermittedMortgage({
        groundLeaseId,
        recordPermittedMortgageRequest: {
          lender: form.lender.trim(),
          // The API requires the field; null means no address on file.
          noticeAddress: form.noticeAddress.trim() || null,
          originalPrincipal: form.originalPrincipal,
          permittedOn: form.permittedOn,
        },
      }),
    onSuccess: (_d, v) => invalidateLease(qc, v.groundLeaseId),
  })
}

export function useEditPermittedMortgage() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, mortgageId, form }: { groundLeaseId: string; mortgageId: string; form: EditPermittedMortgageForm }) =>
      api.editPermittedMortgage({
        groundLeaseId,
        mortgageId,
        editPermittedMortgageRequest: {
          lender: form.lender.trim(),
          noticeAddress: form.noticeAddress.trim() || null,
        },
      }),
    onSuccess: (_d, v) => invalidateLease(qc, v.groundLeaseId),
  })
}

export function useMarkPermittedMortgagePaidOff() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, mortgageId, paidOffOn }: { groundLeaseId: string; mortgageId: string; paidOffOn: CalendarDate }) =>
      api.markPermittedMortgagePaidOff({ groundLeaseId, mortgageId, markPermittedMortgagePaidOffRequest: { paidOffOn } }),
    onSuccess: (_d, v) => invalidateLease(qc, v.groundLeaseId),
  })
}

/** What a homeowner's loan is for (Art 8.3), as the API names it and the app shows it. */
export const MORTGAGE_PURPOSE_LABELS: Record<string, string> = {
  Refinance: 'Refinance',
  SecondMortgage: 'Second Mortgage',
  HomeRepair: 'Home Repair',
  Other: 'Other',
}

/** A mortgage request's status, as the app shows it. */
export const MORTGAGE_REQUEST_STATUS_LABELS: Record<string, string> = {
  Pending: 'Waiting for decision',
  Approved: 'Approved',
  Denied: 'Denied',
  Closed: 'Closed',
  Withdrawn: 'Withdrawn',
}

/** The fewest business days the lease asks for between a request and the loan's closing (Art 8.3). */
export const MORTGAGE_REQUEST_NOTICE_BUSINESS_DAYS = 15

export interface LoanAppraisalForm {
  appraisedValue: number
  appraisalDate: CalendarDate | null
  /** Only sent on an index-based lease. */
  currentIndexValue: number
}

export interface ReceiveMortgageRequestForm {
  receivedOn: CalendarDate
  expectedClosingOn: CalendarDate | null
  lender: string
  purpose: string
  principal: number
  totalDebtAfterClosing: number
  interestRatePercent: number
  rateType: string
  termYears: number
  closingCosts: number
  appraisal: LoanAppraisalForm
  notes: string
}

/** The appraisal fields as the API takes them: all null when no value was entered. */
function appraisalFields(appraisal: LoanAppraisalForm, indexBased: boolean) {
  const has = appraisal.appraisedValue > 0
  return {
    appraisedValue: has ? appraisal.appraisedValue : null,
    appraisalDate: has ? appraisal.appraisalDate : null,
    currentIndexValue: has && indexBased ? appraisal.currentIndexValue : null,
  }
}

function invalidateMortgageRequests(qc: ReturnType<typeof useQueryClient>, groundLeaseId: string) {
  qc.invalidateQueries({ queryKey: ['mortgage-requests', groundLeaseId] })
  qc.invalidateQueries({ queryKey: ['mortgage-request'] })
  qc.invalidateQueries({ queryKey: ['ground-lease', groundLeaseId] })
  qc.invalidateQueries({ queryKey: ['today'] })
}

export function useMortgageRequests(groundLeaseId: string | null) {
  return useQuery({
    queryKey: ['mortgage-requests', groundLeaseId],
    queryFn: () => api.getMortgageRequests({ groundLeaseId: groundLeaseId! }),
    enabled: !!groundLeaseId,
  })
}

export function useMortgageRequest(mortgageRequestId: string | null) {
  return useQuery({
    queryKey: ['mortgage-request', mortgageRequestId],
    queryFn: () => api.getMortgageRequest({ mortgageRequestId: mortgageRequestId! }),
    enabled: !!mortgageRequestId,
  })
}

export function useReceiveMortgageRequest() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async ({ groundLeaseId, form, indexBased }: { groundLeaseId: string; form: ReceiveMortgageRequestForm; indexBased: boolean }) => {
      const result = await api.receiveMortgageRequest({
        groundLeaseId,
        receiveMortgageRequestRequest: {
          receivedOn: form.receivedOn,
          expectedClosingOn: form.expectedClosingOn!,
          lender: form.lender.trim(),
          purpose: form.purpose,
          principal: form.principal,
          totalDebtAfterClosing: form.totalDebtAfterClosing,
          interestRatePercent: form.interestRatePercent,
          rateType: form.rateType,
          termYears: form.termYears,
          closingCosts: form.closingCosts > 0 ? form.closingCosts : null,
          ...appraisalFields(form.appraisal, indexBased),
          notes: form.notes.trim() || null,
        },
      })
      return result as ReceiveMortgageRequestResponse
    },
    onSuccess: (_d, v) => invalidateMortgageRequests(qc, v.groundLeaseId),
  })
}

export function useRecordMortgageRequestAppraisal() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ request, appraisal, closingCosts, indexBased }: {
      request: MortgageRequestResponse
      appraisal: LoanAppraisalForm
      closingCosts: number
      indexBased: boolean
    }) =>
      api.recordMortgageRequestAppraisal({
        mortgageRequestId: request.id!,
        recordMortgageRequestAppraisalRequest: {
          appraisedValue: appraisal.appraisedValue,
          appraisalDate: appraisal.appraisalDate!,
          currentIndexValue: indexBased ? appraisal.currentIndexValue : null,
          closingCosts: closingCosts > 0 ? closingCosts : null,
        },
      }),
    onSuccess: (_d, v) => invalidateMortgageRequests(qc, v.request.groundLeaseId!),
  })
}

export function useApproveMortgageRequest() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ request, reason }: { request: MortgageRequestResponse; reason: string }) =>
      api.approveMortgageRequest({
        mortgageRequestId: request.id!,
        approveMortgageRequestRequest: { approvedOn: trustToday(), reason: reason.trim() || null },
      }),
    onSuccess: (_d, v) => invalidateMortgageRequests(qc, v.request.groundLeaseId!),
  })
}

export function useDenyMortgageRequest() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ request, reason }: { request: MortgageRequestResponse; reason: string }) =>
      api.denyMortgageRequest({
        mortgageRequestId: request.id!,
        denyMortgageRequestRequest: { deniedOn: trustToday(), reason: reason.trim() },
      }),
    onSuccess: (_d, v) => invalidateMortgageRequests(qc, v.request.groundLeaseId!),
  })
}

export function useRecordMortgageLoanClosed() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ request, closedOn, paidOffMortgageIds, noticeAddress }: {
      request: MortgageRequestResponse
      closedOn: CalendarDate
      paidOffMortgageIds: string[]
      noticeAddress: string
    }) =>
      api.recordMortgageLoanClosed({
        mortgageRequestId: request.id!,
        recordMortgageLoanClosedRequest: { closedOn, paidOffMortgageIds, noticeAddress: noticeAddress.trim() || null },
      }),
    onSuccess: (_d, v) => invalidateMortgageRequests(qc, v.request.groundLeaseId!),
  })
}

export function useWithdrawMortgageRequest() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ request, withdrawnOn, reason }: { request: MortgageRequestResponse; withdrawnOn: CalendarDate; reason: string }) =>
      api.withdrawMortgageRequest({
        mortgageRequestId: request.id!,
        withdrawMortgageRequestRequest: { withdrawnOn, reason: reason.trim() },
      }),
    onSuccess: (_d, v) => invalidateMortgageRequests(qc, v.request.groundLeaseId!),
  })
}

/** The figures a resale price statement needs beyond what the lease holds; which count depends on the formula. */
export interface ResalePriceStatementFigures {
  marketValue: number | null
  marketValueSource: string
  indexValue: number | null
  improvementsCredit: number | null
}

const statementFiguresRequest = (statementDate: CalendarDate, figures: ResalePriceStatementFigures) => ({
  statementDate,
  marketValue: figures.marketValue,
  marketValueSource: figures.marketValueSource.trim() || null,
  indexValue: figures.indexValue,
  improvementsCredit: figures.improvementsCredit,
})

/** A lease's resale price on a statement date, worked out by the API; off until the figures are complete. */
export function useResalePriceStatementPreview(
  groundLeaseId: string | null,
  statementDate: CalendarDate | null,
  figures: ResalePriceStatementFigures,
  enabled: boolean,
) {
  return useQuery({
    queryKey: ['resale-price-statement-preview', groundLeaseId, statementDate, figures],
    queryFn: () => api.previewResalePriceStatement({
      groundLeaseId: groundLeaseId!,
      previewResalePriceStatementRequest: statementFiguresRequest(statementDate!, figures),
    }),
    enabled: enabled && !!groundLeaseId && !!statementDate,
    retry: false,
  })
}

export function useRecordResalePriceStatement() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, statementDate, figures, delivery }: {
      groundLeaseId: string
      statementDate: CalendarDate
      figures: ResalePriceStatementFigures
      delivery: NoticeDelivery
    }) =>
      api.recordResalePriceStatement({
        groundLeaseId,
        recordResalePriceStatementRequest: { ...statementFiguresRequest(statementDate, figures), delivery },
      }),
    onSuccess: (_d, v) => {
      qc.invalidateQueries({ queryKey: ['ground-lease', v.groundLeaseId] })
      qc.invalidateQueries({ queryKey: ['resale-price-statements-due'] })
      qc.invalidateQueries({ queryKey: ['today'] })
    },
  })
}

/** The leases on the yearly resale price statement cycle: those due today or earlier, or all of them. */
export function useResalePriceStatementsDue(includeAll: boolean, search: string) {
  return useQuery({
    queryKey: ['resale-price-statements-due', includeAll, search],
    queryFn: () => api.getResalePriceStatementsDue({ includeAll, search: search || undefined }),
  })
}

/** One lease's figures in a round. A round has no improvements credit. */
export interface RoundLeaseFigures {
  groundLeaseId: string
  figures: ResalePriceStatementFigures
}

/** Every lease's resale price in a round, in one request; each lease gets its price or why it can't be worked out. */
export function useResalePriceStatementPreviews(
  statementDate: CalendarDate | null,
  leases: RoundLeaseFigures[],
  enabled: boolean,
) {
  return useQuery({
    queryKey: ['resale-price-statement-previews', statementDate, leases],
    queryFn: () => api.previewResalePriceStatements({
      previewResalePriceStatementsRequest: {
        statementDate: statementDate!,
        leases: leases.map(({ groundLeaseId, figures }) => ({
          groundLeaseId,
          marketValue: figures.marketValue,
          marketValueSource: figures.marketValueSource.trim() || null,
          indexValue: figures.indexValue,
        })),
      },
    }),
    enabled: enabled && !!statementDate && leases.length > 0,
    retry: false,
  })
}

export function useSetInsuranceRequirements() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ groundLeaseId, ...request }: {
      groundLeaseId: string
      replacementValue: number | null
      liabilityPerOccurrence: number | null
      effectiveFrom: CalendarDate
    }) => api.setInsuranceRequirements({ groundLeaseId, setInsuranceRequirementsRequest: request }),
    onSuccess: (_d, v) => {
      invalidateLease(qc, v.groundLeaseId)
      // The current certificate's check moves with the requirements, and with it the Today shortfall item.
      qc.invalidateQueries({ queryKey: ['today'] })
    },
  })
}
