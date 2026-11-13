import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { StewardshipApi } from '../api'
import { apiConfig } from '../lib/apiConfig'
import type { CalendarDate } from '../lib/calendarDate'

const api = new StewardshipApi(apiConfig)

// Every stewardship query key starts with 'stewardship', so any action refreshes both queues and the open case.
const STEWARDSHIP_KEY = ['stewardship'] as const

function useStewardshipMutation<T>(mutationFn: (variables: T) => Promise<unknown>) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn,
    onSuccess: () => qc.invalidateQueries({ queryKey: STEWARDSHIP_KEY }),
  })
}

// ── Queues ────────────────────────────────────────────────────────────────────

/** Every open stewardship case, with its lease's homeowner and address. */
export function useStewardshipQueue() {
  return useQuery({
    queryKey: [...STEWARDSHIP_KEY, 'queue', 'cases'],
    queryFn: () => api.getStewardshipQueue(),
  })
}

/** Every unresolved foreclosure prevention case (newest first), with its lease's homeowner and address. */
export function useForeclosurePreventionQueue() {
  return useQuery({
    queryKey: [...STEWARDSHIP_KEY, 'queue', 'fpc'],
    queryFn: () => api.getForeclosurePreventionQueue(),
  })
}

// ── Single cases ──────────────────────────────────────────────────────────────

export function useStewardshipCase(caseId: string | null) {
  return useQuery({
    queryKey: [...STEWARDSHIP_KEY, 'case', caseId],
    queryFn: () => api.getStewardshipCase({ caseId: caseId! }),
    enabled: !!caseId,
  })
}

export function useForeclosurePreventionCase(caseId: string | null) {
  return useQuery({
    queryKey: [...STEWARDSHIP_KEY, 'fpc', caseId],
    queryFn: () => api.getForeclosurePreventionCase({ caseId: caseId! }),
    enabled: !!caseId,
  })
}

/** The lease's unresolved foreclosure prevention case, or null. A lease has at most one. */
export function useUnresolvedForeclosurePreventionCase(groundLeaseId: string | null) {
  return useQuery({
    queryKey: [...STEWARDSHIP_KEY, 'fpc', 'lease', groundLeaseId],
    queryFn: async () => {
      const page = await api.getForeclosurePreventionCases({ groundLeaseId: groundLeaseId!, pageSize: 100 })
      return (page.items ?? []).find(c => c.status !== 'Resolved') ?? null
    },
    enabled: !!groundLeaseId,
  })
}

// ── Occupancy verification mutations ─────────────────────────────────────────

export function useInitiateOccupancyVerification() {
  return useStewardshipMutation(({ caseId, periodYear, surveyDeadline }: {
    caseId: string
    periodYear: number
    surveyDeadline?: CalendarDate
  }) =>
    api.initiateOccupancyVerification({
      caseId,
      initiateOccupancyVerificationRequest: {
        periodYear,
        surveyDeadline,
      },
    }))
}

export function useSubmitOccupancyVerification() {
  return useStewardshipMutation(({ caseId, verificationId, isPrimaryResident, subletDisclosed, majorChanges, submittedDate }: {
    caseId: string
    verificationId: string
    isPrimaryResident: boolean
    subletDisclosed: boolean
    majorChanges?: string
    submittedDate: CalendarDate
  }) =>
    api.submitOccupancyVerification({
      caseId,
      verificationId,
      submitOccupancyVerificationRequest: {
        isPrimaryResident,
        subletDisclosed,
        majorChanges,
        submittedDate: submittedDate,
      },
    }))
}

export function useMarkOccupancyVerificationOverdue() {
  return useStewardshipMutation(({ caseId, verificationId, deadline }: {
    caseId: string
    verificationId: string
    deadline: CalendarDate
  }) =>
    api.markOccupancyVerificationOverdue({
      caseId,
      verificationId,
      markOccupancyVerificationOverdueRequest: { deadline: deadline },
    }))
}

export function useVerifyOccupancy() {
  return useStewardshipMutation(({ caseId, verificationId, verifiedDate }: {
    caseId: string
    verificationId: string
    verifiedDate: CalendarDate
  }) =>
    api.verifyOccupancy({
      caseId,
      verificationId,
      verifyOccupancyRequest: { verifiedDate: verifiedDate },
    }))
}

export function useFailOccupancy() {
  return useStewardshipMutation(({ caseId, verificationId, reason, failedDate }: {
    caseId: string
    verificationId: string
    reason: string
    failedDate: CalendarDate
  }) =>
    api.failOccupancy({
      caseId,
      verificationId,
      failOccupancyRequest: { reason, failedDate: failedDate },
    }))
}

// ── Property inspection mutations ─────────────────────────────────────────────

export function useSchedulePropertyInspection() {
  return useStewardshipMutation(({ caseId, scheduledDate, inspectorName }: {
    caseId: string
    scheduledDate: CalendarDate
    inspectorName: string
  }) =>
    api.schedulePropertyInspection({
      caseId,
      schedulePropertyInspectionRequest: { scheduledDate: scheduledDate, inspectorName },
    }))
}

export function useCompletePropertyInspection() {
  return useStewardshipMutation(({ caseId, inspectionId, occupancyConfirmed, conditionNotes, completedDate }: {
    caseId: string
    inspectionId: string
    occupancyConfirmed: boolean
    conditionNotes?: string
    completedDate: CalendarDate
  }) =>
    api.completePropertyInspection({
      caseId,
      inspectionId,
      completePropertyInspectionRequest: {
        occupancyConfirmed,
        conditionNotes,
        completedDate: completedDate,
      },
    }))
}

export function useCancelPropertyInspection() {
  return useStewardshipMutation(({ caseId, inspectionId, reason }: {
    caseId: string
    inspectionId: string
    reason: string
  }) =>
    api.cancelPropertyInspection({
      caseId,
      inspectionId,
      cancelPropertyInspectionRequest: { reason },
    }))
}

// ── Lease violation mutations ─────────────────────────────────────────────────

export function useIdentifyViolation() {
  return useStewardshipMutation(({ caseId, violationType, description, identifiedDate }: {
    caseId: string
    violationType: string
    description: string
    identifiedDate: CalendarDate
  }) =>
    api.identifyViolation({
      caseId,
      identifyViolationRequest: { violationType, description, identifiedDate: identifiedDate },
    }))
}

export function useIssueViolationNotice() {
  return useStewardshipMutation(({ caseId, violationId, noticeDate, cureDeadline }: {
    caseId: string
    violationId: string
    noticeDate: CalendarDate
    cureDeadline: CalendarDate
  }) =>
    api.issueViolationNotice({
      caseId,
      violationId,
      issueViolationNoticeRequest: { noticeDate: noticeDate, cureDeadline: cureDeadline },
    }))
}

export function useCureViolation() {
  return useStewardshipMutation(({ caseId, violationId, curedDate }: {
    caseId: string
    violationId: string
    curedDate: CalendarDate
  }) =>
    api.cureViolation({
      caseId,
      violationId,
      cureViolationRequest: { curedDate: curedDate },
    }))
}

export function useEscalateViolation() {
  return useStewardshipMutation(({ caseId, violationId, reason, escalatedDate }: {
    caseId: string
    violationId: string
    reason: string
    escalatedDate: CalendarDate
  }) =>
    api.escalateViolation({
      caseId,
      violationId,
      escalateViolationRequest: { reason, escalatedDate: escalatedDate },
    }))
}

// ── Foreclosure prevention mutations ──────────────────────────────────────────

export function useOpenForeclosurePreventionCase() {
  return useStewardshipMutation((body: {
    groundLeaseId: string
    servicerName: string
    delinquentMonths: number
    recordedDate: CalendarDate
  }) =>
    api.openForeclosurePreventionCase({
      openForeclosurePreventionCaseRequest: { ...body, recordedDate: body.recordedDate },
    }))
}

/** Opens a case for the lease's unpaid notice of default; the API takes the figures from the fee ledger. */
export function useOpenForeclosurePreventionCaseForLeaseDefault() {
  return useStewardshipMutation((groundLeaseId: string) =>
    api.openForeclosurePreventionCaseForLeaseDefault({
      openForeclosurePreventionCaseForLeaseDefaultRequest: { groundLeaseId },
    }))
}

export function useRecordServicerDelinquency() {
  return useStewardshipMutation(({ caseId, ...body }: {
    caseId: string
    servicerName: string
    delinquentMonths: number
    recordedDate: CalendarDate
  }) =>
    api.recordServicerDelinquency({
      caseId,
      recordServicerDelinquencyRequest: { ...body, recordedDate: body.recordedDate },
    }))
}

export function useMakeCounselingReferral() {
  return useStewardshipMutation(({ caseId, agencyName, referralDate }: {
    caseId: string
    agencyName: string
    referralDate: CalendarDate
  }) =>
    api.makeCounselingReferral({
      caseId,
      makeCounselingReferralRequest: { agencyName, referralDate: referralDate },
    }))
}

export function useRecordCounselingOutcome() {
  return useStewardshipMutation(({ caseId, referralId, outcome, outcomeDate }: {
    caseId: string
    referralId: string
    outcome: string
    outcomeDate: CalendarDate
  }) =>
    api.recordCounselingOutcome({
      caseId,
      referralId,
      recordCounselingOutcomeRequest: { outcome, outcomeDate: outcomeDate },
    }))
}

export function useRequestForbearance() {
  return useStewardshipMutation(({ caseId, lenderName, requestedDate }: {
    caseId: string
    lenderName: string
    requestedDate: CalendarDate
  }) =>
    api.requestForbearance({
      caseId,
      requestForbearanceRequest: { lenderName, requestedDate: requestedDate },
    }))
}

export function useRecordForbearanceOutcome() {
  return useStewardshipMutation(({ caseId, requestId, outcome, outcomeDate, notes }: {
    caseId: string
    requestId: string
    outcome: string
    outcomeDate: CalendarDate
    notes?: string
  }) =>
    api.recordForbearanceOutcome({
      caseId,
      requestId,
      recordForbearanceOutcomeRequest: { outcome, outcomeDate: outcomeDate, notes },
    }))
}

export function useActivateCurePeriod() {
  return useStewardshipMutation(({ caseId, lraId, activationDate, curePeriodDays }: {
    caseId: string
    lraId: string
    activationDate: CalendarDate
    curePeriodDays: number
  }) =>
    api.activateCurePeriod({
      caseId,
      activateCurePeriodRequest: { lraId, activationDate: activationDate, curePeriodDays },
    }))
}

export function useResolveForeclosurePreventionCase() {
  return useStewardshipMutation(({ caseId, outcome, resolvedDate }: {
    caseId: string
    outcome: string
    resolvedDate: CalendarDate
  }) =>
    api.resolveForeclosurePreventionCase({
      caseId,
      resolveForeclosurePreventionCaseRequest: { outcome, resolvedDate: resolvedDate },
    }))
}
